using UnityEngine;

public class EnemyAttackMelee2D : MonoBehaviour
{
    public float damage = 1f;
    public float attackCooldown = 0.5f;

    float nextAttackTime = 0f;

    void OnTriggerStay2D(Collider2D other)
    {
        // 先確認有沒有進到 trigger
        Debug.Log($"[EnemyAttack] touching: {other.name}, tag={other.tag}");

        if (!other.CompareTag("Player")) return;

        if (Time.time < nextAttackTime) return;

        if (other.GetComponentInParent<PlayerHealth>() is PlayerHealth playerHealth)
        {
            if (Time.time < nextAttackTime) return;

            nextAttackTime = Time.time + attackCooldown;
            playerHealth.TakeDamage(damage, transform.position, gameObject);
        }
        // 看看找不找得到 IDamageable
        if (other.TryGetComponent<IDamageable>(out var dmgable))
        {
            Debug.Log("[EnemyAttack] Player is damageable -> deal damage");
            nextAttackTime = Time.time + attackCooldown;
            dmgable.TakeDamage(damage, transform.position, gameObject);
        }
        else
        {
            Debug.LogWarning("[EnemyAttack] Player has NO IDamageable on this collider object");
        }
    }
}
