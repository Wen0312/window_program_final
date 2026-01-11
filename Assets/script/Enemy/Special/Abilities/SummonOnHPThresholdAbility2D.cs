using UnityEngine;

/// <summary>
/// SummonOnHPThresholdAbility2D
/// 血量門檻召喚：HP 低於某比例後開始召喚小怪
/// - 不改 EnemyAI / EnemyHealth
/// - 透過 EnemyHealth.OnHPChanged 取得 current/max
/// - 可設定：只觸發一次 or 低血量持續召喚（cooldown）
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/Abilities/SummonOnHPThresholdAbility2D", fileName = "SummonOnHPThresholdAbility2D")]
public class SummonOnHPThresholdAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Trigger")]
    [Range(0f, 1f)]
    public float hpPercentThreshold = 0.5f;   // 例如 0.5 = 50% 以下啟動
    public bool summonOnce = true;            // true：只在第一次跌破門檻召一次

    [Header("Summon")]
    public GameObject[] minionPrefabs;        // 要召的敵人 prefab（可多個隨機）
    public int summonCount = 2;
    public float spawnRadius = 2.0f;

    [Tooltip("低血量持續召喚時的冷卻（秒）；summonOnce=true 時仍會用到 nextTime 防止連續觸發")]
    public float cooldown = 6f;

    [Tooltip("最多召喚次數；0 表示不限制")]
    public int maxActivations = 0;

    [Header("Spawn Safety (optional)")]
    public LayerMask blockedMask;            // 若想避免生成在牆內，可指定 blocking layers
    public float blockedCheckRadius = 0.15f; // OverlapCircle 檢查半徑
    public int maxTryPerMinion = 8;          // 每隻 minion 嘗試幾次找點

    [Header("VFX/SFX (optional)")]
    public GameObject summonVfxPrefab;
    public AudioClip summonSfx;

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly SummonOnHPThresholdAbility2D data;

        float hpRatio = 1f;
        bool thresholdReached = false;
        bool didOnce = false;
        float nextTime = 0f;
        int activations = 0;

        public Runtime(SummonOnHPThresholdAbility2D data)
        {
            this.data = data;
        }

        public override void OnHPChanged(SpecialEnemyContext2D ctx, float current, float max)
        {
            if (max <= 0f) return;
            hpRatio = current / max;

            // 第一次跌破門檻：標記
            if (!thresholdReached && hpRatio <= data.hpPercentThreshold)
            {
                thresholdReached = true;
            }
        }

        public override void Tick(SpecialEnemyContext2D ctx, float dt)
        {
            if (!thresholdReached) return;

            if (data.summonOnce && didOnce) return;

            if (data.maxActivations > 0 && activations >= data.maxActivations) return;

            if (Time.time < nextTime) return;

            // summonOnce：第一次觸發後就結束；否則持續召喚（cooldown）
            DoSummon(ctx);

            activations++;
            didOnce = data.summonOnce;

            nextTime = Time.time + Mathf.Max(0.1f, data.cooldown);
        }

        void DoSummon(SpecialEnemyContext2D ctx)
        {
            if (ctx.transform == null) return;
            if (data.minionPrefabs == null || data.minionPrefabs.Length == 0) return;

            Vector3 center = ctx.transform.position;

            // SFX
            if (data.summonSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.summonSfx);
            }

            // VFX（中心點）
            if (data.summonVfxPrefab != null)
            {
                Object.Instantiate(data.summonVfxPrefab, center, Quaternion.identity);
            }

            int count = Mathf.Max(1, data.summonCount);

            for (int i = 0; i < count; i++)
            {
                var prefab = data.minionPrefabs[Random.Range(0, data.minionPrefabs.Length)];
                if (prefab == null) continue;

                Vector3 spawnPos = FindSpawnPos(center);

                Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            }
        }

        Vector3 FindSpawnPos(Vector3 center)
        {
            // 沒有設定 blockedMask：直接隨機點
            if (data.blockedMask.value == 0)
            {
                Vector2 off = Random.insideUnitCircle * data.spawnRadius;
                return center + (Vector3)off;
            }

            // 有 blockedMask：找一個不重疊的位置
            int tries = Mathf.Max(1, data.maxTryPerMinion);
            for (int t = 0; t < tries; t++)
            {
                Vector2 off = Random.insideUnitCircle * data.spawnRadius;
                Vector3 p = center + (Vector3)off;

                var hit = Physics2D.OverlapCircle(p, data.blockedCheckRadius, data.blockedMask);
                if (hit == null) return p;
            }

            // 找不到就退回中心附近
            return center;
        }
    }
}
