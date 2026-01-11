using System.Collections.Generic;
using UnityEngine;

public class StatusReceiver2D : MonoBehaviour
{
    // 每個狀態一個 runtime instance
    class RuntimeStatus
    {
        public StatusEffectData data;
        public float endTime;          // Time.time + duration；duration=0 時可不用
        public int stacks = 1;

        // DOT
        public float nextTickTime;

        // Slow：目前套用的倍率（取最大/最小規則用）
        public float magnitude;
    }

    [Header("Target")]
    [Tooltip("拖 PlayerStatTarget2D / EnemyStatTarget2D（Unity 不能序列化 interface，所以用 MonoBehaviour 轉型）")]
    [SerializeField] MonoBehaviour targetBehaviour;

    IStatTarget2D target; // PlayerStatTarget2D / EnemyStatTarget2D

    readonly Dictionary<string, RuntimeStatus> active = new Dictionary<string, RuntimeStatus>();

    void Awake()
    {
        // 先用你手動拖的（推薦）
        if (targetBehaviour != null)
            target = targetBehaviour as IStatTarget2D;

        // 沒拖就自動找
        if (target == null)
        {
            target = GetComponent<IStatTarget2D>();
            targetBehaviour = target as MonoBehaviour; // 方便你之後在 Inspector 看到
        }

        if (target == null)
            Debug.LogError($"[StatusReceiver2D] No IStatTarget2D found on {name}", this);
    }
    // ========================
    // Apply
    // ========================
    public void Apply(StatusEffectData data, GameObject instigator = null)
    {
        if (data == null) return;
        if (target == null) return;

        if (!active.TryGetValue(data.id, out var rs))
        {
            rs = new RuntimeStatus { data = data };
            active.Add(data.id, rs);

            // 初次套用：進行 enter 行為
            OnEnter(rs);
        }

        // 疊加規則
        switch (data.stackRule)
        {
            case StatusStackRule.RefreshDuration:
                rs.stacks = 1;
                break;

            case StatusStackRule.StackAdd:
                rs.stacks = Mathf.Min(rs.stacks + 1, data.maxStacks);
                break;

            case StatusStackRule.TakeMaxMagnitude:
                // 這規則通常用在 Slow：取「更慢」= 取更小 multiplier
                rs.magnitude = Mathf.Min(rs.magnitude, GetMagnitude(data));
                break;
        }

        // 刷新時間（duration=0 表示永久）
        if (data.duration > 0f)
            rs.endTime = Time.time + data.duration;

        // DOT 立即允許 tick（不一定要立刻扣，可自行改成 Time.time + tickInterval）
        if (data.kind == StatusKind.PoisonDot)
            rs.nextTickTime = Mathf.Min(rs.nextTickTime, Time.time);

        // 新增：Heal 立即允許 tick（模式同 DOT）
        if (data.kind == StatusKind.Heal)
            rs.nextTickTime = Mathf.Min(rs.nextTickTime, Time.time);

        // 更新持續型效果（如 Slow 取最大）
        OnRefresh(rs);
    }



    public void Remove(StatusEffectData data)
    {
        if (data == null) return;
        if (!active.TryGetValue(data.id, out var rs)) return;

        OnExit(rs);
        active.Remove(data.id);
    }

    // ========================
    // Update
    // ========================
    void Update()
    {
        if (active.Count == 0) return;

        float now = Time.time;

        // Snapshot keys（避免迭代期間移除）
        var keys = ListPool<string>.Get();
        keys.AddRange(active.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            var id = keys[i];
            if (!active.TryGetValue(id, out var rs)) continue;

            // 到期移除
            if (rs.data.duration > 0f && now >= rs.endTime)
            {
                OnExit(rs);
                active.Remove(id);
                continue;
            }

            // 持續行為（DOT）
            if (rs.data.kind == StatusKind.PoisonDot)
                confirmsPoison(rs, now);

            // 新增：持續行為（Heal）
            if (rs.data.kind == StatusKind.Heal)
                confirmsHeal(rs, now);
        }

        ListPool<string>.Release(keys);
    }



    void confirmsPoison(RuntimeStatus rs, float now)
    {
        var data = rs.data;

        if (now < rs.nextTickTime) return;

        float dmgPerTick = data.dps * data.tickInterval * rs.stacks;
        target.DealDamage(dmgPerTick, transform.position, gameObject);

        rs.nextTickTime = now + data.tickInterval;
    }
    // ========================
    //新增：Heal Tick（結構完全比照 Poison）
    // ========================
    void confirmsHeal(RuntimeStatus rs, float now)
    {
        var data = rs.data;

        if (now < rs.nextTickTime) return;

        float healPerTick = data.healPerTick * rs.stacks;
        target.Heal(healPerTick);

        rs.nextTickTime = now + data.tickInterval;
    }



    void OnEnter(RuntimeStatus rs)
    {
        // 設定初始強度
        rs.magnitude = GetMagnitude(rs.data);

        // Slow 一進來就套用
        if (rs.data.kind == StatusKind.Slow)
            target.SetMoveSpeedMultiplier(rs, rs.magnitude);
    }

    void OnRefresh(RuntimeStatus rs)
    {
        // Slow：依規則調整倍率
        if (rs.data.kind == StatusKind.Slow)
        {
            float mag = (rs.data.stackRule == StatusStackRule.TakeMaxMagnitude)
                ? rs.magnitude
                : GetMagnitude(rs.data);

            target.SetMoveSpeedMultiplier(rs, mag);
        }
    }

    void OnExit(RuntimeStatus rs)
    {
        if (rs.data.kind == StatusKind.Slow)
            target.ClearMoveSpeedMultiplier(rs);
    }

    float GetMagnitude(StatusEffectData data)
    {
        if (data.kind == StatusKind.Slow) return Mathf.Clamp01(data.slowMultiplier);
        return 1f;
    }

    // --- tiny pool ---
    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new Stack<List<T>>();
        public static List<T> Get() => pool.Count > 0 ? pool.Pop() : new List<T>(16);
        public static void Release(List<T> list) { list.Clear(); pool.Push(list); }
    }
}
