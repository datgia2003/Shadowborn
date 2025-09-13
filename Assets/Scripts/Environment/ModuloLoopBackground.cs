using UnityEngine;

/// <summary>
/// Modulo based endless background tiler:
/// - Không snap segment => không thấy "delay" khi tileWidth nhỏ hơn camera.
/// - Tính offset bằng phép modulo, di chuyển mượt.
/// - Chủ động tạo đủ số tile để phủ kín bề ngang camera (dynamic) + 2 tile buffer.
/// - Hỗ trợ parallaxFactor.
/// - Không Destroy/Instantiate mỗi frame (chỉ lúc init hoặc thay đổi size camera / sprite / scaleForce). 
///
/// Dùng cho nền 2D Orthographic.
/// </summary>
[ExecuteAlways]
public class ModuloLoopBackground : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("SpriteRenderer gốc (một tile). Sẽ được nhân ra.")]
    public SpriteRenderer sourceTile;

    [Header("Camera")]
    public Camera targetCamera;

    [Header("Parallax")]
    [Range(0f, 1f)] public float parallaxFactor = 0.4f;

    [Header("Auto Fit & Scaling")]
    [Tooltip("Tự scale chiều cao tile khớp camera.")]
    public bool fitHeight = true;
    [Tooltip("Nếu true và bề rộng tile (sau scale) nhỏ hơn camera width * minWidthCoverage => scale rộng.")]
    public bool ensureMinWidth = true;
    [Tooltip("Tỷ lệ tối thiểu tileWidth >= cameraWidth * hệ số này.")]
    public float minWidthCoverage = 0.6f;
    [Tooltip("Thêm buffer chiều rộng (world units) cho mỗi tile sau khi tính toán.")]
    public float widthPadding = 0f;
    [Tooltip("Adjust khoảng ghép (có thể âm nếu bị hở).")]
    public float gapAdjust = 0f;

    [Header("Runtime Control")]
    [Tooltip("Cập nhật lại tiles nếu thay đổi ở editor.")]
    public bool autoRebuildInEditor = true;
    [Tooltip("Log debug.")]
    public bool debugLog;

    private float _tileWidth;          // width của 1 tile sau scale
    private float _tileHeight;
    private int _neededTileCount;      // bao nhiêu tile để phủ camera + buffer
    private Sprite _lastSprite;
    private Vector2 _lastCamSize;
    private float _lastParallax;
    private readonly System.Collections.Generic.List<Transform> _tiles = new();
    private bool _initialized;

    void OnEnable() => Initialize();
    void Awake() => Initialize();

    void Initialize()
    {
        if (sourceTile == null) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        ComputeTileDimensions();
        BuildTiles();
        _initialized = true;
    }

    void ComputeTileDimensions()
    {
        if (sourceTile == null) return;
        var sr = sourceTile;
        var b = sr.bounds;
        _tileWidth = b.size.x;
        _tileHeight = b.size.y;

        if (!Application.isPlaying && _tileWidth <= 0.0001f && sr.sprite != null)
        {
            // Editor fallback khi bounds chưa update
            _tileWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            _tileHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        }

        if (fitHeight && targetCamera != null && targetCamera.orthographic)
        {
            float camHeight = targetCamera.orthographicSize * 2f;
            if (_tileHeight > 0.0001f)
            {
                float scaleY = camHeight / _tileHeight;
                var ls = sr.transform.localScale;
                ls.y = scaleY;
                sr.transform.localScale = ls;
                // re-fetch bounds
                b = sr.bounds;
                _tileWidth = b.size.x;
                _tileHeight = b.size.y;
            }
        }

        if (ensureMinWidth && targetCamera != null && targetCamera.orthographic)
        {
            float camWidth = targetCamera.orthographicSize * 2f * targetCamera.aspect;
            if (_tileWidth < camWidth * minWidthCoverage && _tileWidth > 0.0001f)
            {
                float needScale = (camWidth * minWidthCoverage) / _tileWidth;
                var ls = sourceTile.transform.localScale;
                ls.x *= needScale;
                sourceTile.transform.localScale = ls;
                b = sourceTile.bounds;
                _tileWidth = b.size.x;
                _tileHeight = b.size.y;
            }
        }

        _tileWidth += gapAdjust + widthPadding;
        if (_tileWidth <= 0.0001f) _tileWidth = 5f; // fail-safe
    }

    void BuildTiles()
    {
        if (targetCamera == null) return;

        float camWidth = targetCamera.orthographicSize * 2f * targetCamera.aspect;
        // Số tile cần để phủ camera + 1 tile mỗi bên buffer
        _neededTileCount = Mathf.CeilToInt(camWidth / _tileWidth) + 2;
        if (_neededTileCount < 3) _neededTileCount = 3;

        // Dọn mọi tile cũ (giữ tile gốc làm index 0)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c != sourceTile.transform)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.Undo.DestroyObjectImmediate(c.gameObject);
                else
                    Destroy(c.gameObject);
#else
                Destroy(c.gameObject);
#endif
            }
        }
        _tiles.Clear();

        sourceTile.transform.SetParent(transform, true);
        _tiles.Add(sourceTile.transform);

        // Tạo các bản sao còn lại
        for (int i = 1; i < _neededTileCount; i++)
        {
            var dup = Instantiate(sourceTile.gameObject, transform).transform;
            dup.name = sourceTile.gameObject.name + "_T" + i;
            _tiles.Add(dup);
        }

        if (debugLog)
            Debug.Log($"[ModuloLoopBackground] Built {_neededTileCount} tiles. tileWidth={_tileWidth:F2} camWidth={camWidth:F2}");
    }

    void LateUpdate()
    {
        if (!_initialized || sourceTile == null || targetCamera == null) return;

        // Rebuild conditions (editor or param changes)
        if (!Application.isPlaying && autoRebuildInEditor)
        {
            if (sourceTile.sprite != _lastSprite || Mathf.Abs(parallaxFactor - _lastParallax) > 0.0001f)
            {
                ComputeTileDimensions();
                BuildTiles();
            }
            else
            {
                // camera size change
                Vector2 camSizeNow = new(targetCamera.orthographicSize, targetCamera.aspect);
                if (camSizeNow != _lastCamSize)
                {
                    ComputeTileDimensions();
                    BuildTiles();
                }
            }
        }

        float camX = targetCamera.transform.position.x * parallaxFactor;
        // modulo offset
        if (_tileWidth <= 0.0001f) return;
        float offset = Mathf.Repeat(camX, _tileWidth);
        // muốn tile giữa lệch âm để fill bắt đầu từ -tileWidth
        float startX = camX - offset - _tileWidth;

        for (int i = 0; i < _tiles.Count; i++)
        {
            Vector3 p = _tiles[i].position;
            p.x = startX + _tileWidth * i;
            p.y = sourceTile.transform.position.y; // giữ nguyên y
            _tiles[i].position = p;
        }

        _lastSprite = sourceTile.sprite;
        _lastCamSize = new Vector2(targetCamera.orthographicSize, targetCamera.aspect);
        _lastParallax = parallaxFactor;
    }
}
