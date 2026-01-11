using UnityEngine;

/// <summary>
/// SpecialEnemyContext2D
/// 特殊敵人能力共用上下文（只放引用，不做邏輯）
/// </summary>
public struct SpecialEnemyContext2D
{
    public GameObject gameObject;
    public Transform transform;

    public EnemyHealth health;
    public Rigidbody2D rb;

    public Transform player;

    // Status / Damage 走既有接口
    public IStatTarget2D selfStatTarget;
}
