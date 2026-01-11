using UnityEngine;

public enum StatusStackRule
{
    RefreshDuration,   // 同效果再次套用：只刷新時間
    StackAdd,          // 疊加層數（可用於毒/燃燒）
    TakeMaxMagnitude   // 取最大強度（常用於 Slow：取最慢）
}

public enum StatusKind
{
    PoisonDot,
    Slow,
    Heal // 新增：回血
}

[CreateAssetMenu(menuName = "Game/Status/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    public string id = "poison";
    public StatusKind kind;

    [Header("Duration")]
    public float duration = 2.0f;          // 0 = 永久直到移除
    public StatusStackRule stackRule = StatusStackRule.RefreshDuration;
    public int maxStacks = 5;

    [Header("Poison DOT")]
    public float dps = 8f;
    public float tickInterval = 0.25f;

    [Header("Heal")]
    public float healPerTick = 2f;         // 新增：每 tick 回多少血（搭配 tickInterval）

    [Header("Slow")]
    [Range(0.05f, 1f)]
    public float slowMultiplier = 0.5f;    // 0.5 = 50% speed
}
