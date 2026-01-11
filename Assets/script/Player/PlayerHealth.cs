using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 10f;
    public float iFrameTime = 0.25f; // 

    public event Action<float, float> OnHPChanged; // current, max
    public event Action<float> OnDamaged;          // damage
    public event Action<float> OnHealed;           // 新增：heal amount（實際補到多少）
    public event Action OnDead;

    float hp;
    float invulUntil = 0f;

    void Awake()
    {
        hp = maxHP;
        OnHPChanged?.Invoke(hp, maxHP);
    }

    public void TakeDamage(float amount, Vector2 hitPoint, GameObject instigator)
    {
        if (Time.time < invulUntil) return;
        if (amount <= 0f) return;
        if (Time.time < invulUntil) return;

        invulUntil = Time.time + iFrameTime;

        hp -= amount;
        OnDamaged?.Invoke(amount);
        OnHPChanged?.Invoke(hp, maxHP);

        if (hp <= 0f)
        {
            hp = 0f;
            OnHPChanged?.Invoke(hp, maxHP);

            Debug.Log("Player Dead");
            OnDead?.Invoke();
        }
    }

    // 新增：回血（不走 iFrame、不觸發 OnDamaged）
    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        float before = hp;
        hp = Mathf.Min(maxHP, hp + amount);

        // 只有真的補到血才更新（避免滿血時誤判沒效果）
        float healed = hp - before;
        if (healed > 0f)
        {
            OnHealed?.Invoke(healed);          // 新增：給 Feedback 用
            OnHPChanged?.Invoke(hp, maxHP);
        }
    }
}
