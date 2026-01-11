using UnityEngine;
using System;


/// <summary>
/// BossInfo2D
/// 用來標記「這隻是 Boss」以及提供 HUD 顯示名字
///
/// 方案 C（最終定案）：
/// - Boss 只負責「存在宣告」：在 OnEnable() 觸發事件
/// - 不處理死亡、不處理流程、不處理 UI、不處理 Camera
/// </summary>
public class BossInfo2D : MonoBehaviour
{
    [Header("Boss UI")]
    public string bossName = "BOSS"; // 新增

    // =========================
    // Scheme C: Spawn Announce
    // =========================
    public static event Action<BossInfo2D> OnBossSpawned;

    void OnEnable()
    {
        // Boss 可能是 runtime spawn：OnEnable 一定會被呼叫
        // 只宣告「我出現了」，不做任何流程控制
        OnBossSpawned?.Invoke(this);
    }
}
