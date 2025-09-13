using UnityEngine;
using System.Collections;

public class SpriteAnimationModifier : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        originalRotation = Quaternion.identity; // Always start from identity rotation
        transform.rotation = originalRotation;  // Ensure we start clean
    }

    private void ResetRotation()
    {
        transform.rotation = originalRotation;
    }

    // Idle breathing animation
    public IEnumerator DoIdleBreathing()
    {
        while (true)
        {
            // Scale pattern: 1.06,1.04 -> 1.04,1.06 -> 1.03,1.07 -> 1.05,1.05
            yield return ScaleOverTime(new Vector2(1.06f, 1.04f), 0.2f);
            yield return ScaleOverTime(new Vector2(1.04f, 1.06f), 0.2f);
            yield return ScaleOverTime(new Vector2(1.03f, 1.07f), 0.2f);
            yield return ScaleOverTime(new Vector2(1.05f, 1.05f), 0.2f);
        }
    }

    // Walk forward animation modifiers
    public IEnumerator DoWalkModifier()
    {
        ResetRotation(); // Reset rotation first

        while (true)
        {
            // Tilt back and forth slightly along Y axis only
            yield return ScaleOverTime(new Vector2(1.02f, 0.98f), 0.1f);
            yield return ScaleOverTime(new Vector2(1f, 1f), 0.1f);
        }
    }

    // Cross slash animation modifiers
    public IEnumerator DoCrossSlashModifier()
    {
        bool originalFlipX = spriteRenderer.flipX;

        // Wind up
        yield return ScaleOverTime(new Vector2(0.95f, 1.05f), 0.05f);
        spriteRenderer.flipX = originalFlipX;

        // Attack scaling
        yield return ScaleOverTime(Vector2.one, 0.1f);
        spriteRenderer.flipX = originalFlipX;

        // Recovery
        yield return ScaleOverTime(new Vector2(0.95f, 1.05f), 0.1f);
        spriteRenderer.flipX = originalFlipX;
        yield return ScaleOverTime(Vector2.one, 0.1f);
        spriteRenderer.flipX = originalFlipX;
    }    // Slam animation modifiers
    public IEnumerator DoSlamModifier()
    {
        // Wind up
        yield return ScaleOverTime(new Vector2(0.95f, 1.05f), 0.1f);

        while (true) // LoopStart in original
        {
            yield return ScaleOverTime(Vector2.one, 0.05f);
        }
    }

    // Spin animation modifiers
    public IEnumerator DoSpinModifier()
    {
        ResetRotation();
        Vector3 enlargedScale = originalScale * 1.3f; // Giảm từ 1.5f xuống 1.3f
        transform.localScale = enlargedScale;

        float timer = 0f;
        while (timer < 2f) // Giảm từ 3f xuống 2f
        {
            timer += Time.deltaTime;

            // Scale effects nhẹ hơn
            yield return ScaleOverTime(new Vector2(1.1f, 1.2f), 0.08f); // Giảm intensity
            yield return ScaleOverTime(new Vector2(1.2f, 1.1f), 0.08f);
        }

        // QUAN TRỌNG: Reset về normal scale khi kết thúc
        transform.localScale = originalScale;
    }    // Slam attack animation modifiers
    public IEnumerator DoSlamAttackModifier()
    {
        // ĐẢM BẢO scale là bình thường và KHÔNG thay đổi
        transform.localScale = originalScale;

        // KHÔNG làm gì cả - để animation 1.5s tự chạy
        yield break;
    }

    // Wide slash animation modifiers
    public IEnumerator DoWideSlashModifier()
    {
        // Initial pose
        yield return new WaitForSeconds(0.1f);

        // Wind up
        yield return ScaleOverTime(new Vector2(1.1f, 0.9f), 0.02f);
        yield return ScaleOverTime(Vector2.one, 0.02f);

        // Attack motion with angle
        yield return StartCoroutine(CombinedTransform(
            new Vector2(0.96f, 1.04f), 3f, 0.02f));
        yield return StartCoroutine(CombinedTransform(
            new Vector2(0.9f, 1.1f), 8f, 0.04f));

        // Spin effect
        transform.localScale = originalScale * 1.4f;
        while (true) // LoopStart in original
        {
            yield return RotateOverTime(-360f, 0.01f);
        }
    }

    // Helper method for scaling over time
    private IEnumerator ScaleOverTime(Vector2 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(
            originalScale.x * targetScale.x,
            originalScale.y * targetScale.y,
            originalScale.z
        );

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }    // Helper method for rotation over time
    private IEnumerator RotateOverTime(float angle, float duration)
    {
        Quaternion startRotation = transform.rotation;
        // Chỉ rotate quanh trục Y để tránh bị quay chéo
        Quaternion endRotation = Quaternion.Euler(0, angle, 0);

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.rotation = endRotation;
    }    // Helper method for combined scale and rotation
    private IEnumerator CombinedTransform(Vector2 targetScale, float angle, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(
            originalScale.x * targetScale.x,
            originalScale.y * targetScale.y,
            originalScale.z
        );

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, angle);

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.localScale = endScale;
        transform.rotation = endRotation;
    }

    // Reset all modifications
    public void ResetModifiers()
    {
        StopAllCoroutines();

        // FORCE reset scale và rotation ngay lập tức
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        // Debug để đảm bảo scale đã được reset
        Debug.Log($"ResetModifiers: Scale reset to {originalScale}");
    }
}
