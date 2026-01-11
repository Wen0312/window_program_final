using UnityEngine;

public class EnemyRangedAvoidance2D : MonoBehaviour
{
    [Header("Avoid Settings")]
    public float avoidRadius = 0.8f;
    public float avoidStrength = 1.2f;
    public LayerMask avoidMask;

    [Header("Activation (Hysteresis)")]
    public float enterDistance = 6.2f; // <= 這個距離開始避讓
    public float exitDistance = 7.0f; // >= 這個距離才停止避讓（防微抖）

    [Header("Stability")]
    public float maxStepPerFixed = 0.05f; // 每個 FixedUpdate 最多移動多少（防抖）
    public float deadZone = 0.02f;        // 很小的推力直接忽略（防抖）

    Rigidbody2D rb;
    Transform player;

    bool active; // 避讓是否啟用

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(rb.position, player.position);

        // 遲滯：進入才開，離開才關（避免在門檻邊界反覆切）
        if (!active && distToPlayer <= enterDistance) active = true;
        if (active && distToPlayer >= exitDistance) active = false;
        if (!active) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, avoidRadius, avoidMask);

        Vector2 avoidDir = Vector2.zero;
        foreach (var hit in hits)
        {
            if (hit.attachedRigidbody == rb) continue;

            Vector2 diff = rb.position - (Vector2)hit.transform.position;
            float d = diff.magnitude;
            if (d > 0.01f) avoidDir += diff / d;
        }

        if (avoidDir == Vector2.zero) return;

        Vector2 step = avoidDir.normalized * avoidStrength * Time.fixedDeltaTime;

        // 小到看不出來的修正直接不做（抖動來源之一）
        if (step.magnitude < deadZone) return;

        // 每幀最大位移上限（避免左右來回抖）
        if (step.magnitude > maxStepPerFixed)
            step = step.normalized * maxStepPerFixed;

        rb.MovePosition(rb.position + step);
    }
}
