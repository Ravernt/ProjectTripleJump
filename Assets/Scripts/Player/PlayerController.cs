using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AbilityType
{
    Dash,
    DoubleJump,
    Glide,
    WallJump,
    WallSlide,
    WallCling
}

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Falling,
    DoubleJumping,
    Dashing,
    Dead,
    WallSliding
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
    public bool ClingHeld;
    public Vector2 Move;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] ScriptableStats stats;
    [SerializeField] Health health;

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
    private bool isTouchingWall => isTouchingWallLeft || isTouchingWallRight;

    //Wall jump mechanics
    [SerializeField] private float wallJumpHorizontalForce = 20f;
    private bool hasWallJumped = false;

    //Wall cling mechanics
    [SerializeField] private bool wallClingEnabled = true;

    //Unlocking abilities/////////////////////
    private Dictionary<AbilityType, bool> unlockedAbilities = new Dictionary<AbilityType, bool>
    {
        { AbilityType.Dash, false },
        { AbilityType.DoubleJump, false },
        { AbilityType.Glide, false },
        { AbilityType.WallJump, false },
        { AbilityType.WallSlide, false },
        { AbilityType.WallCling, false }
    };

    public bool HasAbility(AbilityType type) => unlockedAbilities.ContainsKey(type) && unlockedAbilities[type];

    public void UnlockAbility(AbilityType type)
    {
        if (unlockedAbilities.ContainsKey(type))
        {
            unlockedAbilities[type] = true;
            Debug.Log($"[PICKUP] Unlocked ability: {type}");
        }
    }
    /// //////////////////////////////////////

    //Unlocks all abilities
    [SerializeField] private bool unlockAllAbilitiesAtStart = false;


    Rigidbody2D rb;
    CapsuleCollider2D col;
    Vector2 frameVelocity;

    PlayerState currentState = PlayerState.Idle;

    public FrameInput FrameInput { get; private set; }
    public Action<PlayerState> OnStateChange;

    bool canMove = true;
    bool isKnockedBack = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        Physics2D.queriesStartInColliders = false;

        health.OnRespawn += () => canMove = true;
        health.OnDeath += StopMovement;
        StartGround();

        if (unlockAllAbilitiesAtStart)
        {
            foreach (AbilityType ability in Enum.GetValues(typeof(AbilityType)))
            {
                UnlockAbility(ability);
            }
        }
    }

    void StopMovement()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        frameVelocity = Vector2.zero;
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

        // Press number keys 1–6 (top row) to toggle abilities
        if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleAbility(AbilityType.Dash);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleAbility(AbilityType.DoubleJump);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ToggleAbility(AbilityType.Glide);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ToggleAbility(AbilityType.WallJump);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ToggleAbility(AbilityType.WallSlide);
        if (Input.GetKeyDown(KeyCode.Alpha6)) ToggleAbility(AbilityType.WallCling);
    }

    void GatherInput()
    {
        //Saving current frame player input
        FrameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump"),
            JumpHeld = Input.GetButton("Jump"),
            DashDown = Input.GetKey(KeyCode.LeftShift),
            ClingHeld = Input.GetKey(KeyCode.LeftControl),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (FrameInput.JumpDown)
        {
            jumpToConsume = true;
            timeJumpWasPressed = Time.time;
        }
    }

    public void ToggleAbility(AbilityType type)
    {
        if (unlockedAbilities.ContainsKey(type))
        {
            unlockedAbilities[type] = !unlockedAbilities[type];
            Debug.Log($"{type} is now {(unlockedAbilities[type] ? "Enabled" : "Disabled")}");
        }
    }

    void FixedUpdate()
    {
        if (Time.time < 0.05f)
            return;

        CheckCollisions();

        if (canMove)
        {
            HandleJump();
            HandleDirection();
            HandleDash();
            HandleGlide();
            HandleWallSlide();
            HandleWallCling();
        }
        else
        {

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
            //Reset wall jump
            hasWallJumped = false;
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
        isTouchingWallLeft = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.left, wallCheckDistance, stats.CollisionLayer);
        isTouchingWallRight = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.right, wallCheckDistance, stats.CollisionLayer);
        if (!isTouchingWall)
        {
            hasWallJumped = false; //Reset wall jump
        }
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

        if (currentState != PlayerState.Dashing && rb.linearVelocity.y < 0)
        {
            ChangeState(PlayerState.Falling);
        }

        //if can't jump, return
        if (!jumpToConsume && !HasBufferedJump)
        {
            return;
        }

        //handle jumping from the ground or in the air (double, triple jump)
        if (grounded || CanUseCoyote)
        {
            HandleGroundedJump();
        }
        else if (HasAbility(AbilityType.DoubleJump) && airJumpsLeft > 0)
        {
            HandleAirJump();
        }
        //Wall jump
        else if (HasAbility(AbilityType.WallJump) && isTouchingWall && !grounded && !hasWallJumped)
        {
            HandleWallJump();
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
                ChangeState(PlayerState.Falling);
            }
            return;
        }

        bool canPerformDash = (canDash && FrameInput.Move.x != 0);

        if (HasAbility(AbilityType.Dash) && FrameInput.DashDown && canPerformDash)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = Time.time + dashDuration;
        dashDirection = Mathf.Sign(FrameInput.Move.x);
        ChangeState(PlayerState.Dashing);

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
            rb.linearVelocity = frameVelocity;
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

    //Glide mechanics
    void HandleGlide()
    {
        if (HasAbility(AbilityType.Glide) && 
            !grounded && 
            rb.linearVelocity.y < 0 && 
            FrameInput.JumpHeld && 
            currentState != PlayerState.Dashing && 
            Mathf.Abs(FrameInput.Move.x) > 0.1f)

        {
            isGliding = true;
            ChangeState(PlayerState.Falling); //Player state animation
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
        bool isFalling = rb.linearVelocity.y < 0;

        if (HasAbility(AbilityType.WallSlide) &&
            !grounded && 
            isTouchingWall 
            && pushingIntoWall 
            && isFalling 
            && currentState != PlayerState.Dashing)
        {
            frameVelocity.y = Mathf.Max(frameVelocity.y, -wallSlideSpeed);
            ChangeState(PlayerState.WallSliding);
            isGliding = false;
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

        //Push player away from wall
        if(isTouchingWallLeft)
        {
            frameVelocity.x = wallJumpHorizontalForce;
        }
        else
        {
            frameVelocity.x = -wallJumpHorizontalForce;
        }
        isGliding = false;
        hasWallJumped = true;
        ChangeState(PlayerState.Jumping);
    }

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
}