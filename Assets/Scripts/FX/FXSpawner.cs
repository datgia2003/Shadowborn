// Ví dụ sử dụng cho light attack L1:
// Giả sử bạn đã có prefab fx40060, fx6201 (gán qua Inspector hoặc Resources.Load)
// Gọi ở frame đầu (bắt đầu đòn):
// FXSpawner.Spawn(fx40060, player.transform.position + new Vector3(10, 2), new Vector2(0.18f, 0.23f));

// Gọi ở frame 2 (vung đòn):
// FXSpawner.Spawn(fx6201, player.transform.position + new Vector3(35, -35), new Vector2(0.1f, 0.1f), -90f);

// Khi hitbox va chạm enemy (OnTriggerEnter2D trong Hitbox.cs):
// FXSpawner.Spawn(hitSpark, enemy.transform.position);
using UnityEngine;

public class FXSpawner : MonoBehaviour
{
    /// <summary>
    /// Spawn một FX prefab tại vị trí, scale, góc xoay tùy ý. FX sẽ tự hủy sau khi animation chạy xong.
    /// </summary>
    /// <param name="fxPrefab">Prefab FX đã gắn Animator và AnimationClip</param>
    /// <param name="position">Vị trí spawn (theo thế giới)</param>
    /// <param name="scale">Scale (Vector2), nếu null sẽ giữ nguyên scale prefab</param>
    /// <param name="angle">Góc xoay Z (độ)</param>
    /// <param name="flipX">Nếu true, đảo chiều offset.x (dùng cho player quay trái)</param>
    public static void Spawn(GameObject fxPrefab, Vector3 position, Vector2? scale = null, float angle = 0f, bool flipX = false)
    {
        if (fxPrefab == null) { return; }
        if (flipX) angle *= -1; // Đảo dấu góc nếu lật hướng


        var fx = Instantiate(fxPrefab, position, Quaternion.Euler(0, 0, angle));
        if (scale.HasValue)
        {
            Vector2 fxScale = scale.Value;
            if (flipX) fxScale.x *= -1;
            fx.transform.localScale = new Vector3(fxScale.x, fxScale.y, 1);
        }
        // Flip SpriteRenderer nếu có
        if (flipX)
        {
            var srs = fx.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
                sr.flipX = !sr.flipX;
        }

        // Nếu FX có Animator, tự hủy sau khi animation chạy xong
        var anim = fx.GetComponent<Animator>();
        if (anim != null)
        {
            AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
            float clipLength = (clips.Length > 0) ? clips[0].clip.length : 1f;
            Destroy(fx, clipLength);
        }
        else
        {
            Destroy(fx, 1f);
        }
    }
    // Overload cũ để không lỗi code cũ
    public static void Spawn(GameObject fxPrefab, Vector3 position, Vector2? scale, float angle)
    {
        Spawn(fxPrefab, position, scale, angle, false);
    }
    public static void Spawn(GameObject fxPrefab, Vector3 position, Vector2? scale)
    {
        Spawn(fxPrefab, position, scale, 0f, false);
    }
    public static void Spawn(GameObject fxPrefab, Vector3 position)
    {
        Spawn(fxPrefab, position, null, 0f, false);
    }
}
