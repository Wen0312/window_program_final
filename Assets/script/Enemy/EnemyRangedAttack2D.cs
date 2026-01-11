using UnityEngine;

public class EnemyRangedAttack2D : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Shoot")]
    public float projectileSpeed = 12f;
    public float damage = 1f;
    public float fireCooldown = 0.8f;

    [Header("Ranges")]
    public float shootRange = 8f;     // 看到玩家就開始射
    public float stopDistance = 5f;   // 追到這距離就停（交給你的追蹤AI用）

    float nextFireTime;
    Transform player;

    void Awake()
    {
        if (firePoint == null) firePoint = transform; // 沒設就從自己射（至少能動）
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || projectilePrefab == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > shootRange) return;

        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireCooldown;

        Vector2 dir = (player.position - firePoint.position);
        Shoot(dir);
    }

    void Shoot(Vector2 dir)
    {
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile2D proj = go.GetComponent<Projectile2D>();

        if (proj != null)
        {
            proj.Launch(dir, projectileSpeed, damage, gameObject); // owner = Enemy
        }
    }
}
