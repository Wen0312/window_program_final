using UnityEngine;
using System.Reflection;

/// <summary>
/// BossDeathUnlockEndGamePoint2D
/// Boss 死亡後，解鎖 EndGamePoint2D
///
/// 設計原則：
/// - 不改 EnemyHealth
/// - 不改 EndGamePoint2D 的結束流程
/// - Boss 只負責「條件成立」，不是流程控制
/// - 不使用 Update / while / coroutine 輪詢 Boss
/// - 採用方案 C：事件式 Boss Spawn 綁定
/// </summary>
public class BossDeathUnlockEndGamePoint2D : MonoBehaviour
{
    [Header("Auto Find")]
    [Tooltip("場上所有 EndGamePoint2D（Boss 死亡前應為關閉狀態）")]
    public bool autoFindEndPoints = true;

    [Header("Boss Death Camera FX")]
    [Tooltip("Boss 死亡後，停止 Phase 3 連續晃動（避免一直抖到結束/下一關）")]
    public bool stopBossCameraFXOnBossDead = true;

    [Tooltip("要掃描並停用的相機 FX Bridge 類名（不用硬綁；支援不同命名）")]
    public string[] bossCameraFXBridgeTypeNames = new string[]
    {
        "BossPhase3CameraFXBridge2D",
        "BossPhase3CameraFxBridge2D",
        "BossPhase3CameraFXBridge_2D",
        "BossPhase3CameraFxBridge_2D",
    };

    [Tooltip("若 Bridge 有提供 StopAllPhase3FX_Public() 或 StopContinuousShake() 之類方法，嘗試反射呼叫")]
    public bool tryInvokeStopMethodOnBridge = true;

    EnemyHealth bossHealth;
    EndGamePoint2D[] endPoints;
    bool unlocked = false;

    void OnEnable()
    {
        if (autoFindEndPoints)
        {
            endPoints = FindObjectsOfType<EndGamePoint2D>(true);
        }

        // 方案 C：事件式綁定 Boss
        BossInfo2D.OnBossSpawned += HandleBossSpawned;
    }

    void OnDisable()
    {
        BossInfo2D.OnBossSpawned -= HandleBossSpawned;

        if (bossHealth != null)
            bossHealth.OnDead -= HandleBossDead;
    }

    // =========================
    // Scheme C: Boss Spawn Bind
    // =========================
    void HandleBossSpawned(BossInfo2D bossInfo)
    {
        if (bossInfo == null) return;
        if (bossHealth != null) return; // 已綁定過就不重複綁

        // 先找同物件 / parent
        bossHealth = bossInfo.GetComponent<EnemyHealth>();
        if (bossHealth == null) bossHealth = bossInfo.GetComponentInParent<EnemyHealth>();

        // 再往 children 找（Boss 常把 Health 掛子物件）
        if (bossHealth == null) bossHealth = bossInfo.GetComponentInChildren<EnemyHealth>(true);

        if (bossHealth == null)
        {
            Debug.LogWarning("[BossDeathUnlockEndGamePoint2D] BossInfo2D found but EnemyHealth not found.");
            return;
        }

        bossHealth.OnDead -= HandleBossDead; // 保險
        bossHealth.OnDead += HandleBossDead;
    }

    // =========================
    // Boss Dead
    // =========================
    void HandleBossDead()
    {
        if (unlocked) return;
        unlocked = true;

        if (stopBossCameraFXOnBossDead)
        {
            StopBossCameraFX();
        }

        if (endPoints == null || endPoints.Length == 0)
        {
            Debug.LogWarning("[BossDeathUnlockEndGamePoint2D] No EndGamePoint2D found.");
            return;
        }

        foreach (var p in endPoints)
        {
            p.gameObject.SetActive(true);
        }
    }

    // =========================
    // Camera FX Stop (Boss Dead)
    // =========================
    void StopBossCameraFX()
    {
        // 1) 停止當前正在抖的效果
        if (CameraShake2D.Instance != null)
        {
            CameraShake2D.Instance.StopAll();
        }

        // 2) 停止會持續呼叫抖動的 Phase3 Bridge
        var all = FindObjectsOfType<MonoBehaviour>(true);
        for (int i = 0; i < all.Length; i++)
        {
            var mb = all[i];
            if (mb == null) continue;

            string typeName = mb.GetType().Name;
            bool match = false;

            for (int k = 0; k < bossCameraFXBridgeTypeNames.Length; k++)
            {
                if (typeName == bossCameraFXBridgeTypeNames[k])
                {
                    match = true;
                    break;
                }
            }

            if (!match) continue;

            if (tryInvokeStopMethodOnBridge)
            {
                TryInvokeStopMethods(mb);
            }

            mb.enabled = false;
        }
    }

    void TryInvokeStopMethods(MonoBehaviour bridge)
    {
        string[] methodNames = new string[]
        {
            "StopAllPhase3FX_Public",
            "StopAllPhase3FX",
            "StopContinuousShake",
            "StopContinuousShake_Public",
            "StopAll",
            "StopAll_Public",
        };

        var t = bridge.GetType();
        for (int i = 0; i < methodNames.Length; i++)
        {
            var m = t.GetMethod(methodNames[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m == null) continue;
            if (m.GetParameters().Length != 0) continue;

            try { m.Invoke(bridge, null); }
            catch { /* ignore */ }
        }
    }
}
