using UnityEngine;

[DisallowMultipleComponent]
public class AutoDestroyOnAnimationEnd : MonoBehaviour
{
    [Tooltip("Nếu true, hủy sau vòng lặp đầu tiên ngay cả khi clip loop.")]
    public bool destroyOnFirstLoop = true;
    [Tooltip("Thời gian chờ khởi động để tránh destroy sớm khi Particle/Animator chưa kịp chạy.")]
    public float startupGrace = 0.1f;

    [Tooltip("Thời gian tối đa đề phòng trường hợp không phát hiện được trạng thái kết thúc.")]
    public float fallbackLifetime = 5f;

    private Animator _anim;
    private Animation _legacyAnim;
    private ParticleSystem[] _particles;
    private float _t;
    private bool _legacyStarted;

    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        _legacyAnim = GetComponentInChildren<Animation>();
        _particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    void Update()
    {
        _t += Time.deltaTime;

        // Đợi qua thời gian khởi động tối thiểu
        if (_t < startupGrace)
            return;

        // 1) Particle: khi tất cả đã tắt => hủy
        if (_particles != null && _particles.Length > 0)
        {
            bool anyAlive = false;
            foreach (var ps in _particles)
            {
                if (ps != null && ps.IsAlive(true)) { anyAlive = true; break; }
            }
            if (!anyAlive)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 2) Animator: hủy khi qua 1 vòng (normalizedTime >= 1) hoặc clip non-loop kết thúc
        if (_anim != null)
        {
            var st = _anim.GetCurrentAnimatorStateInfo(0);
            // Nếu destroyOnFirstLoop: chỉ cần chạy xong 1 vòng là hủy
            if (destroyOnFirstLoop && st.normalizedTime >= 1.0f)
            {
                Destroy(gameObject);
                return;
            }
            // Nếu clip non-loop: hủy khi về cuối
            if (!st.loop && st.normalizedTime >= 0.99f)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 3) Legacy Animation: hủy khi không còn playing (sau khi đã bắt đầu)
        if (_legacyAnim != null)
        {
            if (!_legacyStarted)
            {
                _legacyStarted = _legacyAnim.isPlaying;
            }
            else
            {
                if (!_legacyAnim.isPlaying)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        // 4) Fallback thời gian tối đa
        if (_t >= fallbackLifetime)
        {
            Destroy(gameObject);
        }
    }
}
