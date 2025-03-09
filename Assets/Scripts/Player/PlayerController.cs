using System;
using System.Collections;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Falling,
    DoubleJumping,
    Dashing
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
    [SerializeField] ScriptableStats stats;

    [SerializeField] private int maxAirJumps = 1;
    private int airJumpsLeft = 0;

    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.1f;
    private bool isDashing;
    private bool canDash = true;
    private float dashTime;
    private float dashDirection;

    Rigidbody2D rb;
    CapsuleCollider2D col;
    Vector2 frameVelocity;

    PlayerState currentState = PlayerState.Idle;

    public FrameInput FrameInput { get; private set; }
    public Action<PlayerState> OnStateChange;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        Physics2D.queriesStartInColliders = false;

        StartGround();
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

        HandleJump();
        HandleDirection();
        HandleGravity();
        HandleDash();

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

            airJumpsLeft = maxAirJumps;
        }

        // Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGrounded = Time.time;
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
        else if (airJumpsLeft > 0)
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

        if(currentState == PlayerState.Dashing)
        {
            frameVelocity.y = 0;
            return;
        }

        if (grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = stats.GroundingForce;
        }
        else
        {
            float inAirGravity = stats.FallAcceleration;
            if (endedJumpEarly && frameVelocity.y > 0)
            {
                inAirGravity *= stats.JumpEndEarlyGravityModifier;
            }

            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

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

        if (FrameInput.DashDown && canPerformDash)
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
        //applies calculated velocity to rigidbody
        rb.linearVelocity = frameVelocity;
    }
}