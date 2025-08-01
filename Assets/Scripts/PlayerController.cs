// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class PlayerController : MonoBehaviour
// {

//     [Header("Speed")]
//     [Tooltip("Tốc độ đi bộ")]
//     public float walkSpeed = 3.5f;
//     [Tooltip("Tốc độ chạy")]
//     public float runSpeed = 7.0f;

//     [Header("Jump")]
//     [Tooltip("Vận tốc nhảy ban đầu theo trục Y")]
//     public float jumpVelocity = 12f;
//     [Tooltip("Rơi nhanh hơn khi đi xuống ( >1 )")]
//     public float fallGravityMultiplier = 1.8f;
//     [Tooltip("Nhả phím sớm thì rơi sớm ( >1 )")]
//     public float lowJumpGravityMultiplier = 2.2f;

//     [Header("Ground Check")]
//     public Transform groundCheck;
//     public float groundCheckRadius = 0.15f;
//     public LayerMask groundMask;

//     [Header("Run logic")]
//     [Tooltip("Khoảng thời gian để tính double-tap (giây)")]
//     public float doubleTapThreshold = 0.25f;
//     [Tooltip("Giữ trạng thái chạy thêm sau khi nhấn Dash (giây)")]
//     public float runHoldAfterDash = 0.5f;

//     Rigidbody2D _rb;
//     Animator _anim;

//     Vector2 _moveInput;         // giá trị Move từ Input System
//     bool _isGrounded;
//     bool _jumpPressedBuffered;
//     float _jumpBufferTimer;
//     const float JumpBuffer = 0.1f; // cho phép nhấn sớm

//     // Run state
//     bool _isRunning;
//     int _prevInputDir;          // -1, 0, 1 (trái, đứng, phải)
//     int _lastTapDir;            // hướng của lần tap trước
//     float _lastTapTime;         // thời điểm tap trước
//     float _runHoldTimer;        // đếm lùi sau Dash

//     void Awake()
//     {
//         _rb = GetComponent<Rigidbody2D>();
//         _anim = GetComponent<Animator>();
//     }

//     // ------------ INPUT (Send Messages) ------------
//     void OnMove(InputValue v)
//     {
//         _moveInput = v.Get<Vector2>();
//         HandleDoubleTapDetection(); // phát hiện double-tap khi có cạnh lên hướng
//     }

//     void OnJump()
//     {
//         // Buffer nhảy: không nhảy ngay trong OnJump để đồng bộ vật lý
//         _jumpPressedBuffered = true;
//         _jumpBufferTimer = JumpBuffer;
//     }

//     void OnDash()
//     {
//         // Dash cũng kích hoạt chạy
//         StartRunHoldTimer();
//     }

//     // ------------ UPDATE LOOP ------------
//     void Update()
//     {
//         // Ground check
//         _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

//         // Jump buffer countdown
//         if (_jumpPressedBuffered)
//         {
//             _jumpBufferTimer -= Time.deltaTime;
//             if (_jumpBufferTimer <= 0f) { _jumpPressedBuffered = false; }
//         }

//         // Run hold timer (sau khi Dash)
//         if (_runHoldTimer > 0f) _runHoldTimer -= Time.deltaTime;

//         // Quyết định có đang chạy không:
//         // - Nếu đang giữ hướng và (đang trong thời gian giữ sau Dash hoặc đã được kích hoạt bởi double-tap) thì chạy.
//         // - Nếu nhả hướng (dir == 0) => không chạy.
//         int dir = Mathf.Abs(_moveInput.x) > 0.1f ? (int)Mathf.Sign(_moveInput.x) : 0;
//         if (dir == 0)
//         {
//             _isRunning = false; // thả phím dừng thì về Walk/Idle
//         }
//         else
//         {
//             // Nếu còn thời gian giữ sau Dash -> chạy
//             if (_runHoldTimer > 0f) _isRunning = true;
//             // Trường hợp double-tap đã bật cờ chạy (Set ở HandleDoubleTapDetection)
//             // thì giữ nguyên _isRunning = true cho tới khi người chơi dừng (dir==0)
//         }

//         // Animator params
//         _anim.SetFloat("Speed", Mathf.Abs(_rb.velocity.x));
//         _anim.SetBool("IsGrounded", _isGrounded);
//         _anim.SetFloat("YVel", _rb.velocity.y);
//         _anim.SetBool("IsRunning", _isRunning);

//         // Lật sprite theo input
//         if (dir != 0)
//             transform.localScale = new Vector3(dir, 1f, 1f);

//         _prevInputDir = dir; // lưu cho frame sau
//     }

//     void FixedUpdate()
//     {
//         // 1) Tốc độ mục tiêu: Walk hoặc Run
//         float targetSpeed = _isRunning ? runSpeed : walkSpeed;
//         float targetVX = _moveInput.x * targetSpeed;

//         // Di chuyển ngang
//         _rb.velocity = new Vector2(targetVX, _rb.velocity.y);

//         // 2) Xử lý nhảy (có buffer)
//         if (_jumpPressedBuffered && _isGrounded)
//         {
//             _jumpPressedBuffered = false; // tiêu buffer
//             var vel = _rb.velocity;
//             vel.y = jumpVelocity;
//             _rb.velocity = vel;
//         }

//         // 3) Trọng lực “đẹp” khi rơi / nhả phím sớm
//         if (_rb.velocity.y < 0f)
//         {
//             _rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallGravityMultiplier - 1f)) * Time.fixedDeltaTime;
//         }
//         else if (_rb.velocity.y > 0f && !IsJumpHeld())
//         {
//             _rb.velocity += Vector2.up * (Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f)) * Time.fixedDeltaTime;
//         }
//     }

//     // Phát hiện double-tap: khi từ trạng thái đứng yên (dir = 0) -> nhấn hướng (dir != 0)
//     void HandleDoubleTapDetection()
//     {
//         int dir = Mathf.Abs(_moveInput.x) > 0.01f ? (int)Mathf.Sign(_moveInput.x) : 0;

//         // Chỉ coi là "tap" khi có cạnh lên: từ 0 -> ±1
//         bool risingEdge = (_prevInputDir == 0 && dir != 0);
//         if (!risingEdge) return;

//         float t = Time.time;

//         if (dir == _lastTapDir && (t - _lastTapTime) <= doubleTapThreshold)
//         {
//             // Double-tap cùng hướng trong ngưỡng -> bật chạy
//             _isRunning = true;
//             // Không cần timer ở trường hợp này; chạy sẽ tắt khi người chơi dừng (dir=0)
//         }

//         _lastTapDir = dir;
//         _lastTapTime = t;
//     }

//     void StartRunHoldTimer()
//     {
//         _isRunning = true;
//         _runHoldTimer = runHoldAfterDash; // giữ chạy trong khoảng thời gian này (nếu vẫn giữ hướng)
//     }

//     // Kiểm tra đang giữ phím Jump với Input System
//     bool IsJumpHeld()
//     {
//         if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) return true;
//         if (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed) return true;
//         return false;
//     }

//     void OnDrawGizmosSelected()
//     {
//         if (groundCheck != null)
//         {
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
//         }
//     }
// }
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float jumpForce = 12f;
    public float fallGravityMultiplier = 1.8f;
    public float lowJumpGravityMultiplier = 2.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    [HideInInspector]
    public bool canMove = true;    // Cho phép di chuyển và nhảy ngoài combat

    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;
    bool isGrounded;

    // Jump buffer
    bool jumpBuffered;
    float jumpBufferTimer;
    const float jumpBufferDuration = 0.1f;

    // Run state via Shift hold
    bool isRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        // Read run input by checking if shift is held
        if (canMove)
        {
            bool holdShift = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            bool holdRunButton = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
            isRunning = holdShift || holdRunButton;
        }

        HandleJumpBuffer();
        UpdateAnimatorParams();
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

    void OnDodge(InputValue v)
    {
        if (!canMove || !v.isPressed) return;
        // Trigger dodge animation / perfect evasion logic
        anim.SetTrigger("DodgeTrigger");
        // TODO: add invincibility frames and movement impulse
    }

    // ---- Movement & Gravity ----
    void HandleMovement()
    {
        float dir = moveInput.x;
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);

        if (Mathf.Abs(dir) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(dir), 1f, 1f);

        if (jumpBuffered && isGrounded)
        {
            jumpBuffered = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
}