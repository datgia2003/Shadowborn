using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
// [RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Buffs")]
    public bool canDoubleJump = false;
    public int dodgeCharges = 1;
    public int maxHP = 100;
    public int maxMP = 50;
    public int currentHP = 100;
    public int currentMP = 50;
    public int statPoints = 0;

    public void AddStatPoints(int points)
    {
        statPoints += points;
        // Optionally, update UI or trigger stat panel refresh here
    }
    [Header("Movement")]
    public float walkSpeed = 7f;
    public float runSpeed = 18f;
    public float jumpForce = 20f;
    public float fallGravityMultiplier = 1.8f;
    public float lowJumpGravityMultiplier = 2.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    [HideInInspector]
    public bool canMove = true;    // Cho phép di chuyển và nhảy ngoài combat

    Rigidbody2D rb;
    public Animator anim;
    Vector2 moveInput;
    public bool isGrounded;

    // Public property to access moveInput from other scripts
    public Vector2 MoveInput => moveInput;

    // Flag to temporarily disable animator parameter updates during special moves
    public bool disableAnimatorUpdates = false;


    // Jump buffer
    bool jumpBuffered;
    float jumpBufferTimer;
    const float jumpBufferDuration = 0.1f;

    // Double jump logic
    int jumpCount = 0;

    // Run state via Shift hold
    bool isRunning;

    // Struct lưu thông tin collider cho từng frame
    [System.Serializable]
    public struct ColliderFrameData
    {
        public Vector2 offset;
        public Vector2 size;
    }

    // Danh sách các animation, mỗi animation là một mảng các frame collider
    [System.Serializable]
    public struct AnimationColliderData
    {
        [Tooltip("Tên animation để dễ nhận biết trong Inspector")]
        public string animationName;
        [Tooltip("Các frame collider cho animation này")]
        public ColliderFrameData[] frames;
    }

    [Header("Collider Data Per Animation")]
    public AnimationColliderData[] animationColliders;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // anim = GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        // Reset jump count when grounded
        if (isGrounded) jumpCount = 0;
        // Read run input by checking if shift is held
        if (canMove)
        {
            bool holdShift = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            bool holdRunButton = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
            isRunning = holdShift || holdRunButton;
        }

        HandleJumpBuffer();

        // Only update animator params if not disabled by special moves (like dodge)
        if (!disableAnimatorUpdates)
        {
            UpdateAnimatorParams();
        }
    }

    void FixedUpdate()
    {
        if (canMove)
            HandleMovement();
        HandleGravity();
    }

    // ---- Input callbacks (Send Messages) ----
    void OnMove(InputValue v)
    {
        moveInput = v.Get<Vector2>();
    }

    void OnJump(InputValue v)
    {
        if (!canMove || !v.isPressed) return;
        jumpBuffered = true;
        jumpBufferTimer = jumpBufferDuration;
    }

    // ❌ DISABLED: Direct input bypass - Dodge should go through UI system only!
    // This method was causing dodge to execute without mana/cooldown checks
    /*
    void OnDodge(InputValue v)
    {
        if (!canMove || !v.isPressed) return;
        // Trigger appropriate dodge animation based on grounded state
        if (isGrounded)
        {
            anim.SetTrigger("Dodge");
        }
        else
        {
            anim.SetTrigger("AirDodge");
        }
        // TODO: add invincibility frames and movement impulse
    }
    */

    /// <summary>
    /// Public method for UI system to trigger dodge
    /// </summary>
    public void PlayDodge()
    {
        // Skip if animator updates are disabled (DodgeSkill is handling it)
        if (disableAnimatorUpdates)
        {
            Debug.Log("PlayerController: PlayDodge skipped - DodgeSkill is handling animation");
            return;
        }

        if (!canMove) return;

        // Determine appropriate dodge animation based on grounded state
        if (isGrounded)
        {
            // Ground dodge animation
            anim.SetTrigger("Dodge");
            Debug.Log("PlayerController: Playing Dodge animation (grounded)");
        }
        else
        {
            // Air dodge animation
            anim.SetTrigger("AirDodge");
            Debug.Log("PlayerController: Playing AirDodge animation (in air)");
        }

        // TODO: add invincibility frames and movement impulse (handled by DodgeSkill)
    }

    // ---- Movement & Gravity ----
    void HandleMovement()
    {
        float dir = moveInput.x;
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);

        // Only handle sprite flipping if not during dodge (DodgeSkill handles its own flipping)
        if (Mathf.Abs(dir) > 0.01f && !disableAnimatorUpdates)
            transform.localScale = new Vector3(Mathf.Sign(dir), 1f, 1f);

        if (jumpBuffered)
        {
            // Ground jump
            if (isGrounded)
            {
                jumpBuffered = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount = 1;
            }
            // Double jump
            else if (canDoubleJump && jumpCount < 2)
            {
                jumpBuffered = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++;
                // Optionally: play double jump animation/effect here
            }
        }
    }

    void HandleGravity()
    {
        if (rb.velocity.y < 0f)
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallGravityMultiplier - 1f)) * Time.fixedDeltaTime;
        else if (rb.velocity.y > 0f && !IsJumpHeld())
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f)) * Time.fixedDeltaTime;
    }

    void HandleJumpBuffer()
    {
        if (jumpBuffered)
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f) jumpBuffered = false;
        }
    }

    bool IsJumpHeld()
    {
        return (Keyboard.current?.spaceKey.isPressed ?? false)
            || (Gamepad.current?.buttonSouth.isPressed ?? false);
    }

    void UpdateAnimatorParams()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("YVel", rb.velocity.y);
        anim.SetBool("IsRunning", isRunning);
    }



    // Hàm chỉnh collider theo animation và frame
    public void SetCharacterCollider(int animationIndex, int frameIndex)
    {
        if (animationColliders == null || animationIndex < 0 || animationIndex >= animationColliders.Length) return;
        var animData = animationColliders[animationIndex];
        if (animData.frames == null || frameIndex < 0 || frameIndex >= animData.frames.Length) return;
        var frameData = animData.frames[frameIndex];
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.offset = frameData.offset;
            box.size = frameData.size;
        }
    }

    // Ví dụ: Animation Event gọi SetCharacterCollider(animationIndex, frameIndex)
    // Trong Inspector, đặt tên animation rõ ràng để dễ quản lý

}