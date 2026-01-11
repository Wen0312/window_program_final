using UnityEngine;

/// <summary>
/// BossSniperPathing2D
/// 狙擊手 Boss 專用移動邏輯（距離驅動）
///
/// 行為規則：
/// - 距離 > fireRange：接近玩家
/// - retreatDistance < 距離 <= fireRange：站定狙擊
/// - 距離 <= retreatTriggerDistance：進入後退狀態，直到距離 >= retreatTargetDistance 才結束
///
/// 設計原則：
/// - 只負責移動與站位
/// - 不碰 UI / Camera / 關卡流程
/// - 不依賴 OnDamaged（純行為邏輯）
/// </summary>
public class BossSniperPathing2D : MonoBehaviour
{
    enum MoveState
    {
        Normal,
        BackOff
    }

    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform player;

    [Header("Acquire Player")]
    public string playerTag = "Player";

    [Header("Ranges")]
    [Tooltip("狙擊射程，進入後會站定")]
    public float fireRange = 10f;

    [Tooltip("進入後退的觸發距離（小於等於就開始退）")]
    public float retreatTriggerDistance = 5f;

    [Tooltip("後退要退到超過這個距離才停止（必須大於 retreatTriggerDistance）")]
    public float retreatTargetDistance = 7f;

    [Header("Move")]
    public float approachSpeed = 2.5f;
    public float backOffSpeed = 4f;

    [Header("Optional Sprite")]
    public SpriteRenderer spriteRenderer;

    MoveState state = MoveState.Normal;

    [Header("Wall Avoid (Right-Hand Rule)")]
    public LayerMask wallMask;
    public float wallCheckDistance = 0.6f;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        AcquirePlayerIfNeeded();
    }

    void AcquirePlayerIfNeeded()
    {
        if (player != null) return;

        GameObject go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (player == null)
        {
            AcquirePlayerIfNeeded();
            if (player == null) return;
        }

        if (retreatTargetDistance < retreatTriggerDistance + 0.01f)
            retreatTargetDistance = retreatTriggerDistance + 0.01f;

        Vector2 bossPos = rb.position;
        Vector2 playerPos = player.position;

        Vector2 toPlayer = playerPos - bossPos;
        float dist = toPlayer.magnitude;

        if (state == MoveState.BackOff)
        {
            if (dist >= retreatTargetDistance)
            {
                state = MoveState.Normal;
            }
            else
            {
                HandleBackOff(bossPos, playerPos);
                return;
            }
        }

        if (dist <= retreatTriggerDistance)
        {
            state = MoveState.BackOff;
            HandleBackOff(bossPos, playerPos);
            return;
        }

        if (dist <= fireRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toPlayer / Mathf.Max(0.0001f, dist);
        rb.linearVelocity = dir * approachSpeed;
        UpdateFacing(dir);
    }

    void HandleBackOff(Vector2 bossPos, Vector2 playerPos)
    {
        Vector2 awayDir = (bossPos - playerPos).normalized;

        if (!IsBlocked(bossPos, awayDir))
        {
            rb.linearVelocity = awayDir * backOffSpeed;
            UpdateFacing(awayDir);
            return;
        }

        Vector2 rightDir = new Vector2(awayDir.y, -awayDir.x);
        if (!IsBlocked(bossPos, rightDir))
        {
            rb.linearVelocity = rightDir * backOffSpeed;
            UpdateFacing(rightDir);
            return;
        }

        Vector2 leftDir = new Vector2(-awayDir.y, awayDir.x);
        if (!IsBlocked(bossPos, leftDir))
        {
            rb.linearVelocity = leftDir * backOffSpeed;
            UpdateFacing(leftDir);
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }

    bool IsBlocked(Vector2 pos, Vector2 dir)
    {
        return Physics2D.Raycast(pos, dir, wallCheckDistance, wallMask);
    }

    void UpdateFacing(Vector2 moveDir)
    {
        if (spriteRenderer == null) return;

        if (moveDir.x > 0.05f) spriteRenderer.flipX = false;
        else if (moveDir.x < -0.05f) spriteRenderer.flipX = true;
    }
}
