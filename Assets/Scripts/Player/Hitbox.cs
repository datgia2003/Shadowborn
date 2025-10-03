using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class Hitbox : MonoBehaviour
{
    [System.Serializable]
    public struct HitFXInfo
    {
        public GameObject fxPrefab;
        public Vector2 scale;
        public float angle;
    }

    public float Damage = 10f;
    public LayerMask TargetMask; // đặt = Enemy
    [Header("Knockback/Hit Pull Settings")]
    public float knockbackForce = 10f; // lực đẩy lùi (tùy chỉnh cho từng hitbox)
    public float hitPullStrength = 5f; // lực hút (tùy chỉnh cho từng hitbox)
    public bool knockUp = false; // hất lên
    public bool knockdownDiagonal = false; // knockdown chéo
    public Transform owner; // gán player làm owner

    [Header("FX khi đánh trúng (nhiều FX, scale, angle)")]
    public HitFXInfo[] hitFXs;
    public AudioClip hitSound;     // Sound khi đánh trúng
    [Range(0f, 30f)] public float hitSoundVolume = 30f; // Volume khi đánh trúng
    public CinemachineImpulseSource impulseSource; // Kéo vào Inspector
    [Header("AudioSource swing (nếu có, sẽ stop khi hit)")]
    [SerializeField] public AudioSource swingAudioSource;

    void OnEnable() { var col = GetComponent<Collider2D>(); if (col) col.enabled = true; }
    void OnDisable() { var col = GetComponent<Collider2D>(); if (col) col.enabled = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & TargetMask) == 0) return;
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            Vector2 hitDir = Vector2.right;
            if (owner != null)
                hitDir = owner.localScale.x > 0 ? Vector2.right : Vector2.left;
            // Nếu knockUp, set hướng lên
            if (knockUp)
                hitDir = new Vector2(hitDir.x, 1f).normalized;
            // Nếu knockdownDiagonal, set hướng chéo xuống
            if (knockdownDiagonal)
                hitDir = new Vector2(hitDir.x, -1f).normalized;

            // Truyền FULL parameters cho TakeHit mới với enhanced effects
            var damageable = dmg as Damageable;
            if (damageable != null)
            {
                // Gọi TakeHit với đầy đủ parameters cho enhanced effects
                damageable.TakeHit(Damage, hitDir, owner, knockbackForce, hitPullStrength);
            }
            else
            {
                // Thử gọi TakeHit enhanced cho boss/enemies khác
                try
                {
                    var bossIgris = dmg as Igris;
                    if (bossIgris != null)
                    {
                        bossIgris.TakeHit(Damage, hitDir, owner, knockbackForce, hitPullStrength);
                    }
                    else
                    {
                        // Fallback cho IDamageable thông thường
                        dmg.TakeHit(Damage);
                    }
                }
                catch
                {
                    dmg.TakeHit(Damage);
                }
            }

            // FX, Sound, Camera shake khi đánh trúng
            if (hitFXs != null)
            {
                foreach (var fx in hitFXs)
                {
                    if (fx.fxPrefab != null)
                        FXSpawner.Spawn(fx.fxPrefab, other.transform.position, fx.scale, fx.angle);
                }
            }
            // Stop swing sound nếu đang phát
            if (swingAudioSource != null && swingAudioSource.isPlaying)
                swingAudioSource.Stop();
            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, other.transform.position, hitSoundVolume);
            if (impulseSource != null)
                impulseSource.GenerateImpulse();
        }
    }
}
