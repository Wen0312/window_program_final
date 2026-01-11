using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 5f;

    //事件：外部表現層用
    public event Action<float, float> OnHPChanged; // (current, max)
    public event Action<float, Vector2, GameObject> OnDamaged; // (damage, hitPoint, instigator)
    public event Action OnDead;

    public GameObject dropPrefab;      // 可選：死亡掉落
    public float dropChance = 0.3f;    // 可選：0~1
    bool dead = false;
    Collider2D[] cols;
    float hp;

    void Awake()
    {
        hp = maxHP;
        cols = GetComponentsInChildren<Collider2D>();
    }
    public void TakeDamage(float amount, Vector2 hitPoint, GameObject instigator)
    {
        if (dead) return;

        hp -= amount;
        OnDamaged?.Invoke(amount, hitPoint, instigator);
        OnHPChanged?.Invoke(hp, maxHP);

        if (hp <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        dead = true;

        //立刻關碰撞：避免「死了還能被打」
        foreach (var c in cols) c.enabled = false;
        OnDead?.Invoke();

        if (dropPrefab != null && UnityEngine.Random.value < dropChance)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}

