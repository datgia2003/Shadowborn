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
    //public Transform Owner;

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
            dmg.TakeHit(Damage);
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
