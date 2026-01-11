using UnityEngine;

public class EnemySpawnTrigger2D : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject enemyPrefab;
    public int spawnCount = 3;

    [Tooltip("Spawn within this radius around the trigger.")]
    public float spawnRadius = 1.5f;

    [Header("Proximity Trigger")]
    [Tooltip("Trigger when player is within this radius.")]
    public float triggerRadius = 4f;

    [Tooltip("How often we check distance (seconds). 0 = every frame.")]
    public float checkInterval = 0.1f;

    [Tooltip("Player tag.")]
    public string playerTag = "Player";

    [Header("Activation")]
    [Tooltip("How many times this trigger can activate (1 = one-shot).")]
    public int maxActivations = 1;

    [Tooltip("Seconds between activations (when maxActivations > 1).")]
    public float cooldown = 2f;

    [Header("Lifetime")]
    [Tooltip("Destroy this obstacle after reaching maxActivations.")]
    public bool destroyAfterDone = true;

    Transform playerTf;
    int activations = 0;
    float nextActivateTime = 0f;
    float nextCheckTime = 0f;

    void Awake()
    {
        // 自動抓玩家（省得你拖）
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null) playerTf = player.transform;
    }

    void Update()
    {
        // 1) 開發者模式不生效（你要的：開發者/編輯檔不觸發）
        if (GameRuntimeFlags2D.developerProfile) return;

        // 2) 次數檢查
        if (maxActivations > 0 && activations >= maxActivations) return;

        // 3) 冷卻
        if (Time.time < nextActivateTime) return;

        // 4) 降頻檢查距離（省效能）
        if (checkInterval > 0f && Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        // 5) 找不到玩家就嘗試重抓（玩家可能重生/換場）
        if (playerTf == null)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) playerTf = player.transform;
            if (playerTf == null) return;
        }

        // 6) 距離判斷（靠近觸發）
        float r = triggerRadius;
        float distSqr = (playerTf.position - transform.position).sqrMagnitude;
        if (distSqr > r * r) return;

        // 觸發生成
        SpawnEnemies();

        activations++;
        nextActivateTime = Time.time + cooldown;

        // 用完自毀（ObstacleCore2D.OnDestroy 會釋放佔格）
        if (maxActivations > 0 && activations >= maxActivations && destroyAfterDone)
        {
            Destroy(gameObject);
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = transform.position + (Vector3)offset;
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 編輯時可視化：觸發半徑 / 生成半徑
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
