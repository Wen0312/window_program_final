using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpecialEnemyController2D
/// 特殊敵人 runtime 控制器
/// - 只負責「把 SpecialEnemyData2D 的能力清單跑起來」
/// - 不改寫既有 EnemyAI / Attack 腳本
/// </summary>
public class SpecialEnemyController2D : MonoBehaviour
{
    [Header("Data")]
    public SpecialEnemyData2D data; // 新增

    [Header("Auto Find")]
    public EnemyHealth health;      // 新增

    SpecialEnemyContext2D ctx;

    // 新增：runtime 以「data.abilities 的 index」對齊，允許中間是 null
    readonly List<SpecialEnemyAbilityRuntime2D> runtimes = new(); // 會存同長度（含 null） // 新增

    [Header("Auto active")]
    // 新增：Ability 啟用狀態（以 data.abilities index 對齊）
    bool[] abilityEnabled; // 新增

    void Awake()
    {
        if (health == null) health = GetComponent<EnemyHealth>();

        ctx = new SpecialEnemyContext2D
        {
            gameObject = gameObject,
            transform = transform,
            health = health,
            rb = GetComponent<Rigidbody2D>(),
            player = null,
            selfStatTarget = GetComponent<IStatTarget2D>(),
        };

        // 找 player（不在 Enemy Awake 亂抓單例，只做一次）
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) ctx.player = p.transform;

        BuildRuntimes();
        HookHealthEvents();
    }

    void OnDestroy()
    {
        UnhookHealthEvents();
    }

    void Update()
    {
        if (runtimes.Count == 0) return;

        float dt = Time.deltaTime;
        for (int i = 0; i < runtimes.Count; i++)
        {
            if (abilityEnabled != null && i < abilityEnabled.Length && !abilityEnabled[i]) continue; // 新增
            var rt = runtimes[i];
            if (rt == null) continue; // 新增
            rt.Tick(ctx, dt);
        }
    }

    void BuildRuntimes()
    {
        runtimes.Clear();
        if (data == null || data.abilities == null) return;

        // 新增：以 data.abilities.Count 為主，runtimes 也要對齊同樣長度
        int n = data.abilities.Count; // 新增

        abilityEnabled = new bool[n]; // 新增
        for (int i = 0; i < n; i++)
            abilityEnabled[i] = true; // 預設啟用

        // 新增：先填滿 n 個位置（可為 null）
        for (int i = 0; i < n; i++) runtimes.Add(null); // 新增

        // 新增：同 index 位置建立 runtime（若能力是 null，該格保持 null）
        for (int i = 0; i < n; i++)
        {
            var a = data.abilities[i];
            if (a == null) continue;

            var rt = a.CreateRuntime();
            if (rt == null) continue;

            runtimes[i] = rt; // 新增：對齊 index
            rt.Init(ctx);
        }
    }

    void HookHealthEvents()
    {
        if (health == null) return;
        health.OnDamaged += HandleDamaged;
        health.OnDead += HandleDead;

        health.OnHPChanged += HandleHPChanged; // 新增
    }

    void UnhookHealthEvents()
    {
        if (health == null) return;
        health.OnDamaged -= HandleDamaged;
        health.OnDead -= HandleDead;

        health.OnHPChanged -= HandleHPChanged; // 新增
    }

    // 新增
    void HandleHPChanged(float current, float max)
    {
        for (int i = 0; i < runtimes.Count; i++)
        {
            if (abilityEnabled != null && i < abilityEnabled.Length && !abilityEnabled[i]) continue; // 新增
            var rt = runtimes[i];
            if (rt == null) continue; // 新增
            rt.OnHPChanged(ctx, current, max);
        }
    }

    void HandleDamaged(float damage, Vector2 hitPoint, GameObject instigator)
    {
        for (int i = 0; i < runtimes.Count; i++)
        {
            if (abilityEnabled != null && i < abilityEnabled.Length && !abilityEnabled[i]) continue; // 新增
            var rt = runtimes[i];
            if (rt == null) continue; // 新增
            rt.OnDamaged(ctx, damage, hitPoint, instigator);
        }
    }

    void HandleDead()
    {
        // 注意：EnemyHealth.Die() 內會 Destroy(gameObject)
        // 因此 OnDeath 只做最後一次觸發（不要在此 Instantiate 很重的東西）
        for (int i = 0; i < runtimes.Count; i++)
        {
            if (abilityEnabled != null && i < abilityEnabled.Length && !abilityEnabled[i]) continue; // 新增
            var rt = runtimes[i];
            if (rt == null) continue; // 新增
            rt.OnDeath(ctx);
        }
    }

    // 新增
    public void EnableAbilityByIndex(int index)
    {
        if (abilityEnabled == null) return;
        if (index < 0 || index >= abilityEnabled.Length) return;
        abilityEnabled[index] = true;
    }

    // 新增：給 ShieldPhase 用，破盾前先鎖能力
    public void DisableAbilityByIndex(int index) // 新增
    {
        if (abilityEnabled == null) return;
        if (index < 0 || index >= abilityEnabled.Length) return;
        abilityEnabled[index] = false;
    }
}
