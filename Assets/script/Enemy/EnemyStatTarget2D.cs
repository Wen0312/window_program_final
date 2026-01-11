using UnityEngine;

public class EnemyStatTarget2D : MonoBehaviour, IStatTarget2D
{
    public EnemyHealth health;
    public EnemyAI_ChasePlayer2D chase;

    float baseSpeed;
    bool inited = false;

    readonly System.Collections.Generic.Dictionary<object, float> speedMult = new();

    void Awake()
    {
        if (health == null) health = GetComponent<EnemyHealth>();
        if (chase == null) chase = GetComponent<EnemyAI_ChasePlayer2D>();
        Init();
    }

    void Init()
    {
        if (inited || chase == null) return;
        baseSpeed = chase.moveSpeed;
        inited = true;
    }

    public void DealDamage(float amount, Vector2 hitPoint, GameObject instigator)
    {
        if (health == null) return;
        health.TakeDamage(amount, hitPoint, instigator);
    }

    // 新增：為了符合 IStatTarget2D 介面（目前敵人不需要回血可先留空）
    public void Heal(float amount)
    {
        // intentionally empty
        // 若未來敵人需要回血：在 EnemyHealth 增加 Heal(amount) 後，在此呼叫即可
        // if (health == null) return;
        // health.Heal(amount);
    }

    public void SetMoveSpeedMultiplier(object source, float multiplier)
    {
        Init();
        if (chase == null) return;

        speedMult[source] = Mathf.Clamp01(multiplier);
        ApplySpeed();
    }

    public void ClearMoveSpeedMultiplier(object source)
    {
        Init();
        if (chase == null) return;

        speedMult.Remove(source);
        ApplySpeed();
    }

    void ApplySpeed()
    {
        float m = 1f;
        foreach (var kv in speedMult)
            m = Mathf.Min(m, kv.Value);

        chase.moveSpeed = baseSpeed * m;
    }
}
