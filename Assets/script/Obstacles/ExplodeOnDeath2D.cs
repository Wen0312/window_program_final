using UnityEngine;

public class ExplodeOnDeath2D : MonoBehaviour
{
    [Header("Explosion")]
    public float radius = 2.5f;
    public float damage = 30f;
    public LayerMask hitMask;          // Player / Enemy / Destructible etc.
    public bool damageOwnerToo = false;

    [Header("VFX/SFX")]
    public GameObject explosionVfx;
    public AudioClip explosionSfx;

    // Optional: if you want to ignore the shooter who destroyed it
    [HideInInspector] public GameObject lastInstigator;

    // 確保只爆一次（避免遞迴/重複觸發）
    bool hasExploded = false;

    public void Explode()
    {
        //只爆一次
        if (hasExploded) return;
        hasExploded = true;

        // VFX
        if (explosionVfx != null)
            Instantiate(explosionVfx, transform.position, Quaternion.identity);

        // SFX（如果你有音效系統再接；沒有就先留著）
        if (explosionSfx != null)
            AudioSource.PlayClipAtPoint(explosionSfx, transform.position);

        // Damage
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);
        foreach (var h in hits)
        {
            if (h == null) continue;

            // 不要炸到自己（否則會自己傷害自己 -> 再觸發死亡 -> 無限遞迴）
            if (h.transform.root.gameObject == gameObject)
                continue;

            // 可選：不要炸到 instigator（例如玩家把油桶打爆，不想炸到自己）
            if (!damageOwnerToo && lastInstigator != null && h.transform.root.gameObject == lastInstigator)
                continue;

            var dmgable = h.GetComponentInParent<IDamageable>();
            if (dmgable != null)
            {
                dmgable.TakeDamage(damage, transform.position, gameObject);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
