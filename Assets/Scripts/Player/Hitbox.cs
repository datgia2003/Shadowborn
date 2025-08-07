using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    public float Damage = 10f;
    public LayerMask TargetMask; // đặt = Enemy
    public Transform Owner;

    void OnEnable() { var col = GetComponent<Collider2D>(); if (col) col.enabled = true; }
    void OnDisable() { var col = GetComponent<Collider2D>(); if (col) col.enabled = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & TargetMask) == 0) return;
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeHit(Damage);
        }
    }
}
