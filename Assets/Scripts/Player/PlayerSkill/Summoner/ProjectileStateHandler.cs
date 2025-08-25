using UnityEngine;
using System.Collections;

public class ProjectileStateHandler : MonoBehaviour
{
    private string attackType;
    private float facingDir;
    private Rigidbody2D rb2d;
    private bool hasStartedFalling = false;
    private float groundLevel = -20f / 16f; // MUGEN: pos y >= -20 converted to Unity units

    public void Initialize(string type, float facing)
    {
        attackType = type;
        facingDir = facing;
        rb2d = GetComponent<Rigidbody2D>();

        if (attackType == "up")
        {
            StartCoroutine(HandleUpAttackState());
        }
    }

    private Vector3 FindBestDropPosition()
    {
        // Tìm tất cả enemy trong phạm vi rộng
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Vector3 casterPos = transform.position;

        if (enemies.Length == 0)
        {
            // Không có enemy, dùng random position rộng
            float randomX = Random.Range(-15f, 15f); // Rộng ±15 units
            Vector3 randomPos = casterPos + new Vector3(facingDir * randomX, 0, 0);
            return randomPos;
        }

        // Tìm center của enemy cluster
        Vector3 enemyCenter = Vector3.zero;
        int validEnemies = 0;
        float maxRange = 25f; // Tăng range tìm enemy

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(casterPos, enemy.transform.position);
                if (distance < maxRange)
                {
                    enemyCenter += enemy.transform.position;
                    validEnemies++;
                }
            }
        }

        Vector3 targetPos;

        if (validEnemies > 0)
        {
            // Có enemy trong range, target vào center của enemy cluster
            enemyCenter /= validEnemies;

            // Add random spread dựa trên số lượng enemy
            float spreadRadius = Mathf.Max(8f, validEnemies * 2f); // Tối thiểu 8f, tăng theo số enemy
            Vector3 randomSpread = new Vector3(
                Random.Range(-spreadRadius, spreadRadius),
                0,
                0
            );

            targetPos = enemyCenter + randomSpread;
        }
        else
        {
            // Enemy quá xa, dùng position rộng theo hướng enemy gần nhất
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    float distance = Vector3.Distance(casterPos, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            if (nearestEnemy != null)
            {
                // Target về phía enemy nhưng với random spread lớn
                Vector3 dirToEnemy = (nearestEnemy.transform.position - casterPos).normalized;
                float randomDistance = Random.Range(10f, 20f);
                Vector3 randomOffset = new Vector3(Random.Range(-10f, 10f), 0, 0);

                targetPos = casterPos + dirToEnemy * randomDistance + randomOffset;
            }
            else
            {
                // Fallback: random rộng
                float randomX = Random.Range(-20f, 20f);
                targetPos = casterPos + new Vector3(facingDir * randomX, 0, 0);
            }
        }

        return targetPos;
    }

    private IEnumerator HandleUpAttackState()
    {
        // MUGEN State 1820 logic:

        // 1. Tìm vị trí tốt nhất để rơi (ưu tiên enemy)
        Vector3 originalPos = transform.position;
        Vector3 targetGroundPos = FindBestDropPosition();

        // 2. Teleport projectile lên trời ngay lập tức
        Vector3 skyPosition = new Vector3(targetGroundPos.x, originalPos.y + 31.25f, originalPos.z); // 500/16 = 31.25
        transform.position = skyPosition;

        // 3. Set initial velocity = 0 (MUGEN: velset = 0,0)
        if (rb2d)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.gravityScale = 2f; // Tăng gravity để rơi nhanh hơn, dễ thấy
        }

        // 4. Wait một frame rồi bắt đầu monitor vị trí
        yield return new WaitForSeconds(0.1f);

        // 5. Monitor position và chờ đến gần mặt đất
        float startY = transform.position.y;
        while (transform.position.y > groundLevel && transform.position.y > -10f) // Safety check
        {
            yield return null;
        }

        // 6. Khi chạm gần mặt đất, set velocity bay ngang
        // MUGEN: velset x = -8, y = -8 (khi pos y >= -20)
        if (rb2d && !hasStartedFalling)
        {
            rb2d.velocity = new Vector2(-8f * facingDir, -2f); // Tăng tốc độ ngang để bay xa
            rb2d.gravityScale = 0.5f; // Giảm gravity để bay xa hơn
            hasStartedFalling = true;
        }

        // 7. Transition to state 1830 (có thể là animation change hoặc behavior change)
        // Ở đây chỉ đơn giản là để projectile bay theo velocity đã set
    }

    void Update()
    {
        // Safety check: nếu projectile rơi quá thấp hoặc bay quá xa thì destroy
        if (transform.position.y < -20f || Mathf.Abs(transform.position.x) > 50f)
        {
            Destroy(gameObject);
        }
    }
}