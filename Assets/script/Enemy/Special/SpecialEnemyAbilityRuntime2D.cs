using UnityEngine;

/// <summary>
/// SpecialEnemyAbilityRuntime2D
/// 特殊能力「runtime instance」
/// - 允許保存每個敵人自己的狀態（cooldown/階段/是否已觸發...）
/// - 不要把邏輯塞進 EnemyAI Update；這裡才是擴充點
/// </summary>
public abstract class SpecialEnemyAbilityRuntime2D
{
    public virtual void Init(SpecialEnemyContext2D ctx) { }

    public virtual void Tick(SpecialEnemyContext2D ctx, float dt) { }

    public virtual void OnDamaged(SpecialEnemyContext2D ctx, float damage, Vector2 hitPoint, GameObject instigator) { }

    // 新增：有些能力（召喚/Phase）需要 HP 門檻資訊
    public virtual void OnHPChanged(SpecialEnemyContext2D ctx, float current, float max) { } // 新增

    public virtual void OnDeath(SpecialEnemyContext2D ctx) { }
}
