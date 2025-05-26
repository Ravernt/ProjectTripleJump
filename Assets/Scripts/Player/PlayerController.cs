using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Profiling.Memory;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
    public Vector2 Move;
}

public enum PlayerState
{
    Idle,
    Running,
    Dash,
    Jumping,
    DoubleJumping,
    Dashing,
    Dead,
    Gliding,
    WallSlidingLeft,
    WallSlidingRight,
    Falling
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject spikePrefab;
    [SerializeField] Transform spikePosition;
    [SerializeField] int amount;
    [Space]
    AudioManager audioManager;
    [SerializeField] ScriptableStats stats;
    [SerializeField] private Health health;
    [SerializeField] bool disableKeyInput = false;
    public PlayerAbilities Abilities { get; private set; }
    private float dashTime;
    private float dashDirection;

    private int airJumpTimes = 0;

    private float lastGlideSoundTime = 0f;
    private float glideSoundCooldown = 0.75f;

    private float lastWallSlideSoundTime = 0f;
    private float wallSlideSoundCooldown = 0.75f;

    //Glide mechanics
    public bool IsGliding { private set; get; } = false;

    //Wall slide mechanics
    private bool isTouchingWallLeft = false;
    private bool isTouchingWallRight = false;
    public bool IsWallSliding = false;
    public bool IsTouchingWall => isTouchingWallLeft || isTouchingWallRight;

    private bool isNearFloor = false;

    //Player state
    Rigidbody2D rb;
    CapsuleCollider2D col;
    public Vector2 frameVelocity;
    public Vector2 FrameVelocity { get { return frameVelocity; } }
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public FrameInput FrameInput { get; set; }
    public ScriptableStats Stats { get { return stats; } set { stats = value; } }
    public Action<PlayerState> OnStateChange;

    bool isKnockedBack = false;

    public delegate void VelocityCalculation(ref Vector2 velocity);
    public VelocityCalculation CalculateVelocity;

    bool dead = false;

    public bool IsDashing { get; set; } = false;
    public bool CanDash { get; set; } = true;

    int current = 0;

    void Awake()
    {


        // Initialize the player controller
        stats = Instantiate(stats);

        var manager = GameObject.FindGameObjectWithTag("Audio");
        if(manager != null)
            audioManager = manager.GetComponent<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        Abilities = GetComponent<PlayerAbilities>();
        Physics2D.queriesStartInColliders = false;

        if (health != null)
        {
            health.OnRespawn += () => dead = false;
            health.OnDeath += StopMovement;
        }

        GroundPlayer();
    }

    void GroundPlayer()
    {
        //Grounds the player at the start
        RaycastHit2D groundHit = Physics2D.Raycast(col.transform.position, Vector2.down, 1000, stats.CollisionLayer);
        col.transform.position = groundHit.point + new Vector2(0, col.size.y * 0.5f);
    }

    public void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        frameVelocity = Vector2.zero;
        dead = true;
    }

    void Update()
    {
        if (Time.time < 0.05f)
            return;

        GatherInput();
        UpdateState();

        CheckCollisions();
        HandleGravity();

        if (!dead)
        {
            HandleJump();
            HandleDirection();
            HandleDash();
            HandleGlide();
            HandleWallSlide();
        }

        ApplyMovement();
    }

    void SpawnSpikes()
    {
        for(int i=0; i<amount; i++)
        {
            Instantiate(spikePrefab, spikePosition.position + new Vector3(0.5f * current, 0), Quaternion.Euler(0, 0, 0));
            current++;
        }
    }

    void GatherInput()
    {
        //Saving current frame player input

        if (!disableKeyInput)
        {
            FrameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump"),
                JumpHeld = Input.GetButton("Jump"),
                DashDown = Input.GetKey(KeyCode.LeftShift),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            };
        }

        if (FrameInput.JumpDown)
        {
            jumpToConsume = true;
            timeJumpWasPressed = Time.time;
        }
    }


    float frameLeftGrounded = float.MinValue;
    public bool Grounded { get; private set; } = true;

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
        if (!Grounded && groundHit)
        {
            if (audioManager != null && Time.time - lastWallSlideSoundTime > wallSlideSoundCooldown)
            {
                audioManager.PlaySFX(audioManager.landingOnGround);
                lastWallSlideSoundTime = Time.time;
            }
            Grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            //Reset air jumps
            airJumpTimes = 0;
        }

        // Left the Ground
        else if (Grounded && !groundHit)
        {
            Grounded = false;
            frameLeftGrounded = Time.time;
        }

        //Character is touching a wall
        Vector2 origin = col.bounds.center;
        Vector2 size = col.size;
        isTouchingWallLeft = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.left, stats.wallCheckDistance, stats.WallJumpCollisionLayer);
        isTouchingWallRight = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.right, stats.wallCheckDistance, stats.WallJumpCollisionLayer);
        isNearFloor = Physics2D.CapsuleCast(origin, size, col.direction, 0, Vector2.down, stats.GrounderDistance, stats.CollisionLayer);
    }

    bool jumpToConsume;
    bool bufferedJumpUsable;
    bool endedJumpEarly;
    bool coyoteUsable;
    float timeJumpWasPressed;

    bool HasBufferedJump => bufferedJumpUsable && Time.time < timeJumpWasPressed + stats.JumpBuffer;
    bool CanUseCoyote => coyoteUsable && !Grounded && Time.time < frameLeftGrounded + stats.CoyoteTime;

    void HandleJump()
    {
        //Did player let go jump button
        if (!endedJumpEarly && !Grounded && !FrameInput.JumpHeld && rb.linearVelocity.y > 0)
        {
            endedJumpEarly = true;
        }

        //if can't jump, return
        if (!jumpToConsume && !HasBufferedJump || IsWallSliding)
        {
            return;
        }

        //handle jumping from the ground or in the air (double, triple jump)
        if (Grounded || CanUseCoyote || IsWallSliding)
        {
            HandleGroundedJump();
        }
        else if (!IsWallSliding && HasEnoughAirJumps())
        {
            HandleAirJump();
        }

        jumpToConsume = false;
    }

    bool HasEnoughAirJumps()
    {
        if (Abilities.HasAbility(AbilityType.DoubleJump))
        {
            if (Abilities.HasAbility(AbilityType.TripleJump))
                return airJumpTimes < 2;

            return airJumpTimes < 1;
        }

        return false;
    }

    void HandleGroundedJump()
    {
        ExecuteJump();
    }
    void HandleAirJump()
    {
        airJumpTimes++;
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
            var deceleration = Grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.deltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, FrameInput.Move.x * stats.MaxSpeed, stats.Acceleration * Time.deltaTime);
        }
    }

    void ChangeState(PlayerState state)
    {
        if (CurrentState != state)
        {
            CurrentState = state;
            OnStateChange?.Invoke(state);
        }
    }

    private void HandleGravity()
    {
        //gravity stuff

        if (IsDashing)
        {
            frameVelocity.y = 0;
            return;
        }

        if (Grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = stats.GroundingForce;
        }

        // Check if gliding
        float inAirGravity = stats.FallAcceleration;
        if (IsGliding)
        {
            frameVelocity.y =
                Mathf.Max(frameVelocity.y - (inAirGravity * stats.glideGravityMultiplier * Time.deltaTime), -stats.glideFallSpeed);
        }
        else
        {
            if (endedJumpEarly && frameVelocity.y > 0)
            {
                inAirGravity *= stats.JumpEndEarlyGravityModifier;
            }

            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.deltaTime);
        }
    }

    //Dash mechanics
    void HandleDash()
    {
        if (IsDashing)
        {
            frameVelocity.x = stats.dashSpeed * dashDirection;
            if (Time.time >= dashTime)
            {
                IsDashing = false;
                frameVelocity.x *= 0.5f;
            }
            return;
        }

        bool canPerformDash = (CanDash && FrameInput.Move.x != 0);

        if (Abilities.HasAbility(AbilityType.Dash) && FrameInput.DashDown && canPerformDash)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        IsDashing = true;
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.dash);
        dashTime = Time.time + stats.dashDuration;
        dashDirection = Mathf.Sign(FrameInput.Move.x);
        frameVelocity.y = 0;

        CanDash = false;
        StartCoroutine(DashCooldownRoutine());
    }

    IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(stats.dashCooldown);
        CanDash = true;
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
    if (Abilities.HasAbility(AbilityType.Glide) &&
        !Grounded &&
        rb.linearVelocity.y < 0 &&
        FrameInput.JumpHeld &&
        CurrentState != PlayerState.Dashing)
    {
        IsGliding = true;

        if (audioManager != null && Time.time - lastGlideSoundTime > glideSoundCooldown)
        {
            audioManager.PlaySFX(audioManager.glideTurnOn);
            lastGlideSoundTime = Time.time;
        }
    }
    else
    {
        IsGliding = false;
    }
}

    //Wall slide mechanics
    void HandleWallSlide()
    {
        bool pushingIntoWall = (isTouchingWallLeft && FrameInput.Move.x < 0) ||
            (isTouchingWallRight && FrameInput.Move.x > 0);

        if (Abilities.HasAbility(AbilityType.WallJump) && !isNearFloor && pushingIntoWall && CurrentState != PlayerState.Dashing)
        {
            frameVelocity.y = -stats.wallSlideSpeed;
            IsGliding = false;
            IsWallSliding = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            frameLeftGrounded = Time.time + 0.45f;
            coyoteUsable = true;
        }
        else
        {
            IsWallSliding = false;
        }
    }

    void UpdateState()
    {
        ChangeState(GetCurrentState());
    }

    public PlayerState GetCurrentState()
    {
        if (dead)
        {
            return PlayerState.Dead;
        }

        if (IsDashing)
        {
            return PlayerState.Dashing;
        }

        if (IsWallSliding)
        {
            return isTouchingWallLeft ? PlayerState.WallSlidingLeft : PlayerState.WallSlidingRight;
        }


        if (frameVelocity.y < -1.5f && !Grounded)
        {
            return IsGliding ? PlayerState.Gliding : PlayerState.Falling;
        }
        if (frameVelocity.y > 0 && (CurrentState == PlayerState.Jumping || CurrentState == PlayerState.DoubleJumping))
        {
            return CurrentState;
        }

        if (frameVelocity.x != 0)
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
}
