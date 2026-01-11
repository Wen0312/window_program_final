using System.Collections.Generic;
using UnityEngine;

public class PoisonZone2D_Legacy : MonoBehaviour
{
    [Header("Damage Over Time")]
    [Tooltip("Damage per second")]
    public float dps = 8f;

    [Tooltip("How often to apply damage (seconds). Smaller = smoother, bigger = cheaper.")]
    public float tickInterval = 0.25f;

    [Header("Targets")]
    public LayerMask affectMask; // Player / Enemy

    // 目標 -> 下一次 tick 的時間
    readonly Dictionary<IDamageable, float> nextTickTime = new Dictionary<IDamageable, float>();

    // 佇列：避免在 Update 枚舉時，被 OnTriggerEnter/Exit 修改 Dictionary
    readonly HashSet<IDamageable> pendingAdd = new HashSet<IDamageable>();
    readonly HashSet<IDamageable> pendingRemove = new HashSet<IDamageable>();

    void OnTriggerEnter2D(Collider2D other)
    {
        // Layer 過濾（避免子彈/雜物進來）
        if (((1 << other.gameObject.layer) & affectMask.value) == 0)
            return;

        var dmgable = other.GetComponentInParent<IDamageable>();
        if (dmgable == null) return;

        // 進入：先排入佇列，交給 Update 統一處理
        pendingAdd.Add(dmgable);
        pendingRemove.Remove(dmgable);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var dmgable = other.GetComponentInParent<IDamageable>();
        if (dmgable == null) return;

        // 離開：先排入佇列，交給 Update 統一處理
        pendingRemove.Add(dmgable);
        pendingAdd.Remove(dmgable);
    }

    void Update()
    {
        // 先套用離開
        if (pendingRemove.Count > 0)
        {
            foreach (var d in pendingRemove)
                nextTickTime.Remove(d);
            pendingRemove.Clear();
        }

        // 再套用進入（初次進入：允許立刻 tick）
        if (pendingAdd.Count > 0)
        {
            foreach (var d in pendingAdd)
            {
                if (!nextTickTime.ContainsKey(d))
                    nextTickTime.Add(d, Time.time);
            }
            pendingAdd.Clear();
        }

        if (nextTickTime.Count == 0) return;

        float now = Time.time;
        float dmgPerTick = dps * tickInterval;

        //用 key snapshot，避免 Dictionary 在過程中被修改時炸掉
        var keys = ListPool<IDamageable>.Get();
        keys.AddRange(nextTickTime.Keys);

        var toRemove = ListPool<IDamageable>.Get();

        for (int i = 0; i < keys.Count; i++)
        {
            var dmgable = keys[i];

            // 目標可能已 Destroy（Unity null）
            var mb = dmgable as MonoBehaviour;
            if (mb == null)
            {
                toRemove.Add(dmgable);
                continue;
            }

            // 可能剛好被移除（離開毒區），跳過
            if (!nextTickTime.TryGetValue(dmgable, out float due))
                continue;

            if (now < due) continue;

            dmgable.TakeDamage(dmgPerTick, transform.position, gameObject);

            // 更新下一次 tick 時間
            nextTickTime[dmgable] = now + tickInterval;
        }

        // 清掉 Destroy 的目標
        for (int i = 0; i < toRemove.Count; i++)
            nextTickTime.Remove(toRemove[i]);

        ListPool<IDamageable>.Release(keys);
        ListPool<IDamageable>.Release(toRemove);
    }

    void OnDisable()
    {
        nextTickTime.Clear();
        pendingAdd.Clear();
        pendingRemove.Clear();
    }

    // --- 超輕量 ListPool（避免每幀 new List 造成 GC） ---
    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new Stack<List<T>>();

        public static List<T> Get()
        {
            return pool.Count > 0 ? pool.Pop() : new List<T>(16);
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
