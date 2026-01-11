using UnityEngine;

public class EnemySpawner2D : MonoBehaviour
{
    [System.Serializable]
    public class WeightedPrefab
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;
    }

    [Header("Spawn")]
    public WeightedPrefab[] enemies;    // 有權重的敵人清單
    public float interval = 2f;
    public int maxAlive = 20;
    public float radius = 8f;

    [Header("Safety")]
    public float safeDistanceFromPlayer = 3f; // 與玩家的最小生成距離
    public int maxTryCount = 10;              // 嘗試找安全位置的最大次數

    float timer;
    Transform player;

    void Awake()
    {
        // 找玩家（只做一次，效能比較好）
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (enemies == null || enemies.Length == 0) return;
        if (CountAlive() >= maxAlive) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            SpawnOne();
        }
    }

    int CountAlive()
    {
        // 用 EnemyHealth 計數最穩（近戰/遠程都會有）
        return FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
    }

    void SpawnOne()
    {
        GameObject prefab = PickWeightedPrefab();
        if (prefab == null) return;

        Vector3 pos;
        if (!TryGetSafeSpawnPosition(out pos))
        {
            // 如果找不到安全位置，就放棄這次生成（避免怪貼臉）
            return;
        }

        Instantiate(prefab, pos, Quaternion.identity);
    }

    bool TryGetSafeSpawnPosition(out Vector3 pos)
    {
        // 預設給一個值，避免 compiler 抱怨
        pos = transform.position;

        for (int i = 0; i < maxTryCount; i++)
        {
            // 在半徑內隨機一個方向
            Vector2 offset = Random.insideUnitCircle.normalized * radius;
            Vector3 candidate = transform.position + new Vector3(offset.x, offset.y, 0f);

            // 如果沒有玩家，就直接用這個位置
            if (player == null)
            {
                pos = candidate;
                return true;
            }

            // 距離玩家夠遠，視為安全
            float distToPlayer = Vector2.Distance(candidate, player.position);
            if (distToPlayer >= safeDistanceFromPlayer)
            {
                pos = candidate;
                return true;
            }
        }

        // 嘗試多次仍找不到安全位置
        return false;
    }

    GameObject PickWeightedPrefab()
    {
        float total = 0f;

        // 計算總權重
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].prefab == null) continue;
            if (enemies[i].weight <= 0f) continue;
            total += enemies[i].weight;
        }
        if (total <= 0f) return null;

        // 抽權重
        float r = Random.value * total;
        float acc = 0f;

        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (e.prefab == null || e.weight <= 0f) continue;

            acc += e.weight;
            if (r <= acc) return e.prefab;
        }

        // 保底回傳最後一個有效 prefab
        for (int i = enemies.Length - 1; i >= 0; i--)
            if (enemies[i].prefab != null && enemies[i].weight > 0f)
                return enemies[i].prefab;

        return null;
    }
}
