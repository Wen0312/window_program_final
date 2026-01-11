using UnityEngine;

public class EnemyAI_ChasePlayer2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Tooltip("與玩家保持的最小距離（<= 這個距離就停止）")]
    public float stopDistance = 0.8f;

    Rigidbody2D rb;
    Transform player;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Top-down 遊戲：不受重力、不旋轉
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Start()
    {
        // 找玩家（只找一次，效能安全）
        var p = FindFirstObjectByType<TopDownPlayerMove2D>();
        if (p != null)
            player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // 指向玩家的向量
        Vector2 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        // === 安全距離判定 ===
        // 近戰：stopDistance 小（貼近）
        // 遠程：stopDistance 大（保持距離）
        if (dist <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero; // 完全停住
            return;
        }

        // 正規化方向（避免距離影響速度）
        Vector2 dir = toPlayer / dist;

        // 使用 MovePosition，避免物理穿透
        rb.MovePosition(
            rb.position + dir * moveSpeed * Time.fixedDeltaTime
        );
    }
}
