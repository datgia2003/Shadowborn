using UnityEngine;

public class TrapTilemapTrigger : MonoBehaviour
{
    public Transform safeSpawnPoint; // Vị trí dịch chuyển lên khi dính trap
    public int damage = 20;
    public float invincibleTime = 2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerResources>();
            var controller = other.GetComponent<PlayerController>();
            if (player != null && controller != null)
            {
                player.TakeDamage(damage);
                other.transform.position = controller.lastJumpPosition;
                player.StartInvincible(invincibleTime);
            }
        }
    }
}
