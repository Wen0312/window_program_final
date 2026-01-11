using System;
using UnityEngine;

/// <summary>
/// BossPhaseState2D
/// Boss 階段狀態（HUD 顯示用）
/// - Ability 可以呼叫 SetPhase() 更新階段字串
/// </summary>
public class BossPhaseState2D : MonoBehaviour
{
    [Header("Phase")]
    public string currentPhase = ""; // 新增

    public event Action<string> OnPhaseChanged; // 新增

    // 新增
    public void SetPhase(string phase)
    {
        if (currentPhase == phase) return;
        currentPhase = phase;
        OnPhaseChanged?.Invoke(currentPhase);
    }
}
