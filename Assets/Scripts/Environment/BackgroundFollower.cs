using UnityEngine;

/// <summary>
/// Giữ 1 object nền (ví dụ: mặt trăng) đi theo camera với tuỳ chọn parallax hoặc cố định theo vị trí màn hình (viewport anchor).
/// Gắn script này lên GameObject chứa SpriteRenderer của bạn.
/// </summary>
public class BackgroundFollower : MonoBehaviour
{
    public enum FollowMode
    {
        Parallax,       // Dịch theo camera * parallaxFactor + offset (parallaxFactor nhỏ => di chuyển chậm hơn => cảm giác xa)
        ViewportAnchor  // Cố định tại 1 toạ độ viewport (0..1) => như UI nhưng vẫn ở world (có depth)
    }

    [Header("Camera")]
    public Camera targetCamera;               // Nếu null sẽ tự lấy MainCamera

    [Header("Mode")]
    public FollowMode mode = FollowMode.Parallax;

    [Header("Parallax Settings")]
    [Tooltip("0 = đứng yên so với camera (luôn trong tầm). 1 = bám y như camera. 0.2~0.5 cho nền xa.")]
    [Range(0f, 1f)] public float parallaxFactorX = 0.2f;
    [Range(0f, 1f)] public float parallaxFactorY = 0f; // thường mặt trăng ít đổi Y
    public Vector3 worldOffset = new Vector3(0f, 0f, 10f); // giữ Z khác camera nếu cần

    [Header("Viewport Anchor (Mode = ViewportAnchor)")]
    [Tooltip("Toạ độ viewport (0,0) góc trái dưới; (1,1) góc phải trên")]
    public Vector2 viewportPos = new Vector2(0.85f, 0.8f);
    [Tooltip("Khoảng cách từ camera theo trục Z (âm nếu camera nhìn -Z)")]
    public float depthFromCamera = 10f;

    [Header("Common Options")]
    public bool lockZ = true;                // Giữ nguyên Z ban đầu
    public bool smooth = true;               // Nội suy mượt
    [Range(0.01f, 10f)] public float smoothSpeed = 5f;

    [Header("Auto Setup")]
    public bool autoCaptureStartOffset = true; // Khi bật Parallax sẽ dùng vị trí hiện tại để tính offset ban đầu

    private Vector3 _initialWorldPos;
    private Vector3 _parallaxBaseOffset; // offset nội tại nếu auto capture
    private bool _initialized;
    private bool _offsetCaptured; // Flag để track nếu offset đã được capture

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        Initialize();
    }

    void Initialize()
    {
        if (_initialized) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        _initialWorldPos = transform.position;
        // Simplified - no more complex offset capture needed
        // Just use worldOffset directly for any static offset requirements
        _parallaxBaseOffset = worldOffset;

        _initialized = true;
        Debug.Log($"BackgroundFollower initialized in {mode} mode - simplified camera follow");
    }

    void LateUpdate()
    {
        if (!_initialized) Initialize();
        if (targetCamera == null) return;

        Vector3 targetPos = transform.position;

        if (mode == FollowMode.Parallax)
        {
            // Simplified: Just follow camera with parallax factors
            Vector3 camPos = targetCamera.transform.position;
            targetPos = new Vector3(
                camPos.x * parallaxFactorX,
                camPos.y * parallaxFactorY,
                lockZ ? transform.position.z : camPos.z
            );
            // Add world offset
            targetPos += new Vector3(worldOffset.x, worldOffset.y, lockZ ? 0f : worldOffset.z);
        }
        else if (mode == FollowMode.ViewportAnchor)
        {
            // Lấy world point từ viewport mong muốn
            float zOff = depthFromCamera;
            if (lockZ)
            {
                zOff = transform.position.z - targetCamera.transform.position.z; // giữ nguyên Z chênh lệch hiện tại
            }
            Vector3 viewTarget = new Vector3(viewportPos.x, viewportPos.y, Mathf.Abs(zOff));
            // Nếu camera dùng -Z (thường trong 2D), đảm bảo distance dương
            targetPos = targetCamera.ViewportToWorldPoint(viewTarget);
            if (lockZ)
                targetPos.z = transform.position.z;
            else
                targetPos.z = targetCamera.transform.position.z + (targetCamera.orthographic ? 0f : zOff);
        }

        if (smooth)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.position = targetPos;
        }
    }

    [ContextMenu("Reinitialize (Capture Offset)")]
    public void Reinitialize()
    {
        _initialized = false;
        _offsetCaptured = false; // Reset offset capture flag để force recapture
        Initialize();
    }

    /// <summary>
    /// Force recapture of parallax offset - now simplified to just reset to camera follow
    /// </summary>
    public void RecaptureOffset()
    {
        Debug.Log($"BackgroundFollower: Recapture called - now using simplified camera follow");
        // With simplified logic, no complex offset capture needed
        // Background will just follow camera based on parallax factors
    }

    /// <summary>
    /// Debug method để xem current status
    /// </summary>
    [ContextMenu("Debug Background Status")]
    public void DebugStatus()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        Debug.Log($"=== BackgroundFollower Debug ===");
        Debug.Log($"Mode: {mode}");
        Debug.Log($"AutoCapture: {autoCaptureStartOffset}");
        Debug.Log($"Initialized: {_initialized}");
        Debug.Log($"OffsetCaptured: {_offsetCaptured}");
        Debug.Log($"Current Position: {transform.position}");
        Debug.Log($"Camera Position: {(targetCamera ? targetCamera.transform.position.ToString() : "NULL")}");
        Debug.Log($"Parallax Factors: X={parallaxFactorX}, Y={parallaxFactorY}");
        Debug.Log($"Base Offset: {_parallaxBaseOffset}");
        Debug.Log($"World Offset: {worldOffset}");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (parallaxFactorX < 0f) parallaxFactorX = 0f;
        if (parallaxFactorY < 0f) parallaxFactorY = 0f;
        if (parallaxFactorX > 1f) parallaxFactorX = 1f;
        if (parallaxFactorY > 1f) parallaxFactorY = 1f;
    }
#endif
}
