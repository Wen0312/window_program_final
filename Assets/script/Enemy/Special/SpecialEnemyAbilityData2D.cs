using UnityEngine;

/// <summary>
/// SpecialEnemyAbilityData2D
/// 特殊能力「資料」
/// - ScriptableObject 只放參數，不存 runtime 狀態
/// - 每個敵人實例由 CreateRuntime() 產生一份 runtime instance
/// </summary>
public abstract class SpecialEnemyAbilityData2D : ScriptableObject
{
    // 新增：每個敵人生成一份 runtime（避免把狀態塞進 SO）
    public abstract SpecialEnemyAbilityRuntime2D CreateRuntime();
}
