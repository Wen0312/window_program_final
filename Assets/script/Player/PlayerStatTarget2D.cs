using UnityEngine;

public class PlayerStatTarget2D : MonoBehaviour, IStatTarget2D
{
    public PlayerHealth health;
    public TopDownPlayerMove2D move;

    float baseSpeed;
    bool inited = false;

    // ㄓ方э硉程程篊
    readonly System.Collections.Generic.Dictionary<object, float> speedMult = new();

    void Awake()
    {
        if (health == null) health = GetComponent<PlayerHealth>();
        if (move == null) move = GetComponent<TopDownPlayerMove2D>();
        Init();
    }

    void Init()
    {
        if (inited || move == null) return;
        baseSpeed = move.moveSpeed;
        inited = true;
    }

    public void DealDamage(float amount, Vector2 hitPoint, GameObject instigator)
    {
        if (health == null) return;
        health.TakeDamage(amount, hitPoint, instigator); //  PlayerHealth 竒ǐ硂甅
    }

    // 穝糤才 IStatTarget2DHeal 篈ノ
    public void Heal(float amount)
    {
        if (health == null) return;
        health.Heal(amount);
    }

    public void SetMoveSpeedMultiplier(object source, float multiplier)
    {
        Init();
        if (move == null) return;

        speedMult[source] = Mathf.Clamp01(multiplier);
        ApplySpeed();
    }

    public void ClearMoveSpeedMultiplier(object source)
    {
        Init();
        if (move == null) return;

        speedMult.Remove(source);
        ApplySpeed();
    }

    void ApplySpeed()
    {
        float m = 1f;
        foreach (var kv in speedMult)
            m = Mathf.Min(m, kv.Value);

        move.moveSpeed = baseSpeed * m;
    }
}
