using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile2D : MonoBehaviour
{
    public float lifeTime = 2f;

    Rigidbody2D rb;
    float damage;
    GameObject owner;
    bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //統一用這個
    public void Launch(Vector2 dir, float speed, float dmg, GameObject ownerGO = null)
    {
        damage = dmg;
        owner = ownerGO;

        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifeTime);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //只命中一次
        if (hasHit) return;

        // 避免打到自己
        if (owner != null && other.transform.root.gameObject == owner) return;

        // ---------- Wall / Blocking Obstacle ----------
        // 撞到牆或「會阻擋的障礙物」→ 子彈消失
        int layer = other.gameObject.layer;
        bool isWall = layer == LayerMask.NameToLayer("Wall");

        var obstacleCore = other.GetComponentInParent<ObstacleCore2D>();
        bool blocksByObstacle =
            obstacleCore != null &&
            obstacleCore.data != null &&
            obstacleCore.data.blocksMovement;

        if (isWall || blocksByObstacle)
        {
            hasHit = true;

            // 有血量才扣（牆 / 油桶）
            var dmgableObs = other.GetComponentInParent<IDamageable>();
            if (dmgableObs != null)
            {
                dmgableObs.TakeDamage(damage, transform.position, owner);
            }

            Destroy(gameObject);
            return;
        }
        // ---------------------------------------------

        // ---------- Non-blocking Obstacle (Wire / Poison) ----------
        // 不阻擋的區域：完全忽略，不影響子彈
        if (obstacleCore != null &&
            obstacleCore.data != null &&
            obstacleCore.data.blocksMovement == false)
        {
            return;
        }
        // ----------------------------------------------------------

        //依 owner 決定打誰
        var root = other.transform.root;

        if (owner != null)
        {
            if (owner.CompareTag("Player") && !root.CompareTag("Enemy")) return;
            if (owner.CompareTag("Enemy") && !root.CompareTag("Player")) return;
        }

        // 真正命中敵人 / 玩家
        var dmgable = other.GetComponentInParent<IDamageable>();
        if (dmgable != null)
        {
            hasHit = true;   //只有這裡才標記命中
            dmgable.TakeDamage(damage, transform.position, owner);
            Destroy(gameObject);
        }
    }




}
