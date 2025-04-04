using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Falling,
    DoubleJumping,
    Dashing,
    Dead,
    WallSlidingLeft,
    WallSlidingRight,
    Gliding
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
    public Vector2 Move;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private ScriptableStats stats;
    [SerializeField] private Health health;
    [SerializeField] private PlayerAbilities abilities;

    [SerializeField] private int maxAirJumps = 1;
    private int airJumpsLeft = 0;

    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.1f;
    private bool isDashing;
    private bool canDash = true;
    private float dashTime;
    private float dashDirection;

    //Glide mechanics
    [SerializeField] private float glideFallSpeed = 5f;
    [SerializeField] private float glideGravityMultiplier = 0.3f;
    private bool isGliding = false;

    //Wall slide mechanics
    [SerializeField] private float wallSlideSpeed = 5f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    private bool isTouchingWallLeft = false;
    private bool isTouchingWallRight = false;
    private bool isWallSliding = false;
    private bool isTouchingWall => isTouchingWallLeft || isTouchingWallRight;

    private bool isNearFloor = false;

    //Player state
    Rigidbody2D rb;
    CapsuleCollider2D col;
    Vector2 frameVelocity;
    PlayerState currentState = PlayerState.Idle;
    public FrameInput FrameInput { get; private set; }
    public Action<PlayerState> OnStateChange;

    bool isKnockedBack = false;

    public delegate void VelocityCalculation(ref Vector2 velocity);
    public VelocityCalculation CalculateVelocity;

    bool dead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        Physics2D.queriesStartInColliders = false;

        health.OnRespawn += () => dead = false;
        health.OnDeath += StopMovement;
        StartGround();
    }

    void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        frameVelocity = Vector2.zero;
        dead = true;
    }

    void StartGround()
    {
        //Grounds the player at the start
        RaycastHit2D groundHit = Physics2D.Raycast(col.transform.position, Vector2.down, 1000, stats.CollisionLayer);
        col.transform.position = groundHit.point + new Vector2(0, col.size.y * 0.5f);
    }

    void Update()
    {
        GatherInput();
        UpdateState();
    }

    void GatherInput()
    {
        //Saving current frame player input
        FrameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump"),
            JumpHeld = Input.GetButton("Jump"),
            DashDown = Input.GetKey(KeyCode.LeftShift),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (FrameInput.JumpDown)
        {
            jumpToConsume = true;
            timeJumpWasPressed = Time.time;
        }
    }


    void FixedUpdate()
    {
        if (Time.time < 0.05f)
            return;

        CheckCollisions();

        if (!dead)
        {
            HandleJump();
            HandleDirection();
            HandleDash();
            HandleGlide();
            HandleWallSlide();
        }

        HandleGravity();
        ApplyMovement();
    }

    float frameLeftGrounded = float.MinValue;
    bool grounded = true;

    void CheckCollisions()
    {
        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.down, stats.GrounderDistance, stats.CollisionLayer);
        bool ceilingHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.up, stats.GrounderDistance, stats.CollisionLayer);

        // Hit a Ceiling
        if (ceilingHit)
        {
            frameVelocity.y = Mathf.Min(0, frameVelocity.y);
        }

        // Landed on the Ground
        if (!grounded && groundHit)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            //Reset air jumps
            airJumpsLeft = maxAirJumps;
        }

        // Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGrounded = Time.time;
        }

        //Character is touching a wall
        Vector2 origin = col.bounds.center;
        Vector2 size = col.size;
        isTouchingWallLeft = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.left, wallCheckDistance, stats.WallJumpCollisionLayer);
        isTouchingWallRight = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.right, wallCheckDistance, stats.WallJumpCollisionLayer);
        isNearFloor = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.down, stats.GrounderDistance, stats.CollisionLayer);
    }

    bool jumpToConsume;
    bool bufferedJumpUsable;
    bool endedJumpEarly;
    bool coyoteUsable;
    float timeJumpWasPressed;

    bool HasBufferedJump => bufferedJumpUsable && Time.time < timeJumpWasPressed + stats.JumpBuffer;
    bool CanUseCoyote => coyoteUsable && !grounded && Time.time < frameLeftGrounded + stats.CoyoteTime;

    void HandleJump()
    {
        //Did player let go jump button
        if (!endedJumpEarly && !grounded && !FrameInput.JumpHeld && rb.linearVelocity.y > 0)
        {
            endedJumpEarly = true;
        }

        //if can't jump, return
        if (!jumpToConsume && !HasBufferedJump || isWallSliding)
        {
            return;
        }

        //handle jumping from the ground or in the air (double, triple jump)
        if (grounded || CanUseCoyote || isWallSliding)
        {
            HandleGroundedJump();
        }
        else if (abilities.HasAbility(AbilityType.DoubleJump) && !isWallSliding && airJumpsLeft > 0)
        {
            HandleAirJump();
        }

        jumpToConsume = false;
    }

    void HandleGroundedJump()
    {
        ExecuteJump();
    }
    void HandleAirJump()
    {
        airJumpsLeft--;
        ExecuteJump();
        ChangeState(PlayerState.DoubleJumping);
    }

    void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        frameVelocity.y = stats.JumpPower;
        ChangeState(PlayerState.Jumping);
    }

    void HandleDirection()
    {
        //handles player acceleration and deceleration
        if (FrameInput.Move.x == 0)
        {
            var deceleration = grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, FrameInput.Move.x * stats.MaxSpeed, stats.Acceleration * Time.fixedDeltaTime);
        }

        /*
        if (grounded && (currentState == PlayerState.Idle || currentState == PlayerState.Running || currentState == PlayerState.Falling))
        {
            if (Mathf.Abs(FrameInput.Move.x) > 0.1f)
            {
                ChangeState(PlayerState.Running);
            }
            else
            {
                ChangeState(PlayerState.Idle);
            }
        }
        */
    }

    void ChangeState(PlayerState state)
    {
        if (currentState != state)
        {
            currentState = state;
            OnStateChange?.Invoke(state);
        }
    }

    private void HandleGravity()
    {
        //gravity stuff

        if (currentState == PlayerState.Dashing)
        {
            frameVelocity.y = 0;
            return;
        }

        if (grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = stats.GroundingForce;
        }

        // Check if gliding
        float inAirGravity = stats.FallAcceleration;
        if (isGliding)
        {
            frameVelocity.y = Mathf.Max(frameVelocity.y - (inAirGravity * glideGravityMultiplier * Time.fixedDeltaTime), -glideFallSpeed);
        }

        else
        {
            if (endedJumpEarly && frameVelocity.y > 0)
            {
                inAirGravity *= stats.JumpEndEarlyGravityModifier;
            }

            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    //Dash mechanics
    void HandleDash()
    {
        if (isDashing)
        {
            frameVelocity.x = dashSpeed * dashDirection;
            if (Time.time >= dashTime)
            {
                isDashing = false;
                frameVelocity.x *= 0.5f;
            }
            return;
        }

        bool canPerformDash = (canDash && FrameInput.Move.x != 0);

        if (abilities.HasAbility(AbilityType.Dash) && FrameInput.DashDown && canPerformDash)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = Time.time + dashDuration;
        dashDirection = Mathf.Sign(FrameInput.Move.x);

        canDash = false;
        StartCoroutine(DashCooldownRoutine());
    }

    IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void ApplyMovement()
    {
        //applies calculated velocity to rigidbody'
        if (!isKnockedBack)
        {
            Vector2 otherForces = Vector2.zero;
            CalculateVelocity?.Invoke(ref otherForces);
            rb.linearVelocity = frameVelocity + otherForces;
        }
    }

    //Glide mechanics
    void HandleGlide()
    {
        if (abilities.HasAbility(AbilityType.Glide) && 
            !grounded && 
            rb.linearVelocity.y < 0 && 
            FrameInput.JumpHeld && 
            currentState != PlayerState.Dashing)

        {
            isGliding = true;
        }
        else
        {
            isGliding = false;
        }
    }

    //Wall slide mechanics
    void HandleWallSlide()
    {
        bool pushingIntoWall = (isTouchingWallLeft && FrameInput.Move.x < 0) || 
            (isTouchingWallRight && FrameInput.Move.x > 0);
        bool isFalling = rb.linearVelocity.y < 0 || true;

        if (abilities.HasAbility(AbilityType.WallJump) &&
            !isNearFloor && 
            isTouchingWall 
            && pushingIntoWall 
            && isFalling 
            && currentState != PlayerState.Dashing)
        {
            frameVelocity.y = Mathf.Max(frameVelocity.y, -wallSlideSpeed);
            isGliding = false;
            isWallSliding = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            frameLeftGrounded = Time.time + 0.45f;
            coyoteUsable = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    //Wall jump mechanics
    void HandleWallJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;

        frameVelocity.y = stats.JumpPower;

        isGliding = false;
        ChangeState(PlayerState.Jumping);
    }

    void UpdateState()
    {
        ChangeState(GetCurrentState());
    }

    PlayerState GetCurrentState()
    {
        if (dead)
        {
            return PlayerState.Dead;
        }

        if (isDashing)
        {
            return PlayerState.Dashing;
        }

        if(isWallSliding)
        {
            return isTouchingWallLeft ? PlayerState.WallSlidingLeft : PlayerState.WallSlidingRight;
        }


        if (frameVelocity.y < -1.5f && !grounded)
        {
            return isGliding? PlayerState.Gliding : PlayerState.Falling;
        }

        if (frameVelocity.y > 0 && (currentState == PlayerState.Jumping || currentState == PlayerState.DoubleJumping))
        {
            return currentState;
        }

        if(frameVelocity.x != 0)
        {
            return PlayerState.Running;
        }

        return PlayerState.Idle;
    }

    public void ApplyForce(Vector2 force, float duration)
    {
        isKnockedBack = true;
        rb.AddForce(force, ForceMode2D.Impulse);
        Invoke(nameof(ResetKnockback), duration);
    }

    void ResetKnockback()
    {
        isKnockedBack = false;
    }

    /*
    //Wall cling mechanics
    void HandleWallCling()
    {
        if(!wallClingEnabled)
        {
            return;
        }

        bool isClinging =
            (HasAbility(AbilityType.WallCling) &&
            !grounded &&
            isTouchingWall &&
            FrameInput.ClingHeld &&
            (currentState != PlayerState.Dashing));

        if (isClinging)
        {
            frameVelocity.y = 0f;
            isGliding = false;
            ChangeState(PlayerState.WallSliding);
        }
    }
    */
}