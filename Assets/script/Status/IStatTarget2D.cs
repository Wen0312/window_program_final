public interface IStatTarget2D
{
    // 讓效果能造成傷害（DOT / 爆炸後燃燒都用得到）
    void DealDamage(float amount, UnityEngine.Vector2 hitPoint, UnityEngine.GameObject instigator);

    // 新增：讓效果能造成回血（Heal Zone / Regen）
    void Heal(float amount);

    // 讓效果能調整移動速度（Slow / Haste）
    void SetMoveSpeedMultiplier(object source, float multiplier);
    void ClearMoveSpeedMultiplier(object source);
}
