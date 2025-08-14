using UnityEngine;

public class RockDebris : MonoBehaviour
{
    [Header("Sprites & Visual")]
    public Sprite[] rockSprites;        // List sprite đá
    public SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    public float minSpeedX = 1f;
    public float maxSpeedX = 3f;
    public float minSpeedY = 4f;
    public float maxSpeedY = 8f;
    public float gravity = 0.8f;
    public float bounceFactor = 0.2f;   // Giống VelMul y = -0.2
    public float friction = 0.8f;       // Giống VelMul x = 0.8
    public float groundY = 0f;          // Mốc mặt đất

    [Header("Rotation Settings")]
    public float minScaleFactor = 1f;
    public float maxScaleFactor = 2f;
    public float rotationSpeed = 120f;  // Độ/giây

    [Header("Lifetime")]
    public float lifeTime = 2f;         // Tự hủy sau n giây

    private Vector2 velocity;
    private float rotationAngle;

    private bool customScale = false;
    private Vector3 customScaleValue = Vector3.one;

    // Cho phép truyền scale động khi spawn
    public void Init(Vector3? scale = null)
    {
        if (scale.HasValue)
        {
            customScale = true;
            customScaleValue = scale.Value;
            transform.localScale = customScaleValue;
        }
    }

    void Start()
    {
        // Random sprite
        if (rockSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = rockSprites[Random.Range(0, rockSprites.Length)];
        }

        // Scale: nếu có custom thì dùng, không thì random
        if (!customScale)
        {
            float scaleFactor = Random.Range(minScaleFactor, maxScaleFactor) * 0.08f;
            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }


        // Random hướng & vận tốc ban đầu (bay tỏa ra nhiều hướng)
        // Chọn góc từ 60-120 độ (trái) hoặc 60-120 độ (phải), random lực
        float angleDeg = Random.Range(60f, 120f);
        if (Random.value < 0.5f)
            angleDeg = 180f - angleDeg; // tỏa sang trái hoặc phải
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float speed = Random.Range(minSpeedY, maxSpeedY); // dùng min/max Y làm lực tổng
        velocity.x = Mathf.Cos(angleRad) * speed;
        velocity.y = Mathf.Sin(angleRad) * speed;

        // Random góc xoay ban đầu
        rotationAngle = Random.Range(0f, 360f);

        // Tự hủy sau lifeTime
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Gravity
        if (transform.position.y > groundY || velocity.y < 0)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Cập nhật vị trí
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // Nếu chạm đất
        if (transform.position.y <= groundY)
        {
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

            // Nảy
            if (Mathf.Abs(velocity.y) > 0.2f)
            {
                velocity.y *= -bounceFactor;
            }
            else
            {
                velocity.y = 0;
            }

            // Giảm trượt ngang
            velocity.x *= friction;
        }

        // Xoay liên tục
        rotationAngle += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }
}
