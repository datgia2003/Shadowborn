using UnityEngine;

[
    RequireComponent(typeof(Rigidbody2D))
]
public class BatController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public float chaseRange = 5f;
    public float attackRange = 0.65f;
    public float attackCooldown = 1.5f;
    public int maxHealth = 3;
    public int damage = 1;
    public float stopDistance = 0.15f; // khoảng cách đệm, enemy sẽ không tiến sát player quá

    [Header("References")]
    public Transform player;
    public Transform attackPoint;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private float lastAttackTime;
    private int currentHealth;
    private bool isDead = false;
    private Damageable damageable;

    enum State { Idle, Chasing, Attacking, Hurt, Dead }
    State currentState = State.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        damageable = GetComponent<Damageable>();
    }

    void Update()
    {
        if (damageable != null && damageable.IsStunned) return;
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Flip sprite đúng hướng
        if (player.position.x < transform.position.x)
            sprite.flipX = false; // nhìn trái
        else
            sprite.flipX = true; // nhìn phải

        // Đảm bảo attackPoint luôn nằm phía trước mặt enemy
        if (attackPoint != null)
        {
            Vector3 ap = attackPoint.localPosition;
            float absX = Mathf.Abs(ap.x);
            ap.x = sprite.flipX ? absX : -absX;
            attackPoint.localPosition = ap;
        }

        // Cập nhật bay nhanh/chậm
        bool isFlyingFast = dist < chaseRange;
        anim.SetBool("isFlyingFast", isFlyingFast);

        switch (currentState)
        {
            case State.Idle:
                rb.velocity = Vector2.zero;
                if (dist < chaseRange)
                {
                    currentState = State.Chasing;
                }
                break;

            case State.Chasing:
                if (dist > chaseRange)
                {
                    currentState = State.Idle;
                    rb.velocity = Vector2.zero;
                }
                else if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    currentState = State.Attacking;
                    rb.velocity = Vector2.zero;

                    // Random attack
                    int rand = Random.Range(0, 2);
                    string attackTrigger = (rand == 0) ? "Attack1" : "Attack2";
                    anim.SetTrigger(attackTrigger);

                    lastAttackTime = Time.time;
                }
                else if (dist > attackRange + stopDistance)
                {
                    Vector2 dir = (player.position - transform.position).normalized;
                    rb.velocity = dir * moveSpeed;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
                break;

            case State.Attacking:
                rb.velocity = Vector2.zero;
                break;

            case State.Hurt:
                rb.velocity = Vector2.zero;
                break;

            case State.Dead:
                rb.velocity = Vector2.zero;
                break;
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = State.Hurt;
            anim.SetTrigger("Hurt");
        }
    }

    void Die()
    {
        isDead = true;
        currentState = State.Dead;
        anim.SetTrigger("Die");
        rb.velocity = Vector2.zero;

        // Award experience to player
        var experienceSystem = FindObjectOfType<ExperienceSystem>();
        if (experienceSystem != null)
        {
            experienceSystem.GainExpFromEnemy("bat");
            Debug.Log("💀 Bat defeated → +50 EXP awarded to player");
        }
        else
        {
            Debug.LogWarning("⚠️ ExperienceSystem not found - no EXP awarded");
        }

        Destroy(gameObject, 1.5f); // Cho animation chết
    }

    // Animation Event
    public void OnAttackHit()
    {
        if (player == null) return;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, 1f, playerLayer);
        if (hit != null)
        {
            Debug.Log($"🦇 Bat hit detected: {hit.name} with damage {damage}");

            // Try PlayerResources first (primary system)
            PlayerResources playerRes = hit.GetComponent<PlayerResources>();
            if (playerRes != null)
            {
                playerRes.TakeDamage(damage);
                Debug.Log($"✅ Applied {damage} damage to PlayerResources");
                return;
            }

            // Try Damageable as backup
            Damageable dmg = hit.GetComponent<Damageable>();
            if (dmg != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)attackPoint.position).normalized;
                dmg.TakeHit(damage, dir, this.transform);
                Debug.Log($"✅ Applied {damage} damage to Damageable");
                return;
            }

            Debug.LogWarning($"❌ No damage component found on {hit.name}!");
        }
        else
        {
            Debug.Log($"🦇 Bat attack missed - no player in range");
        }
    }

    // Animation Event
    public void OnAttackEnd()
    {
        if (isDead) return;
        currentState = State.Chasing;
    }

    // Animation Event
    public void OnHurtEnd()
    {
        if (!isDead)
            currentState = State.Chasing;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 0.5f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
