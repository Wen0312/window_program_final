using UnityEngine;

/// <summary>
/// ExplodeOnDeathAbility2D
/// 死亡爆炸（自爆型）
/// - 不改 EnemyHealth.Die()
/// - 透過 EnemyHealth.OnDead 觸發
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/Abilities/ExplodeOnDeathAbility2D", fileName = "ExplodeOnDeathAbility2D")]
public class ExplodeOnDeathAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Explosion")]
    public float radius = 2.5f;
    public float damage = 3f;
    public LayerMask affectMask;
    public GameObject vfxPrefab;

    [Header("SFX")] // 新增
    public AudioClip explodeSfx; // 新增

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly ExplodeOnDeathAbility2D data;
        bool done = false;

        public Runtime(ExplodeOnDeathAbility2D data)
        {
            this.data = data;
        }

        public override void OnDeath(SpecialEnemyContext2D ctx)
        {
            if (done) return;
            done = true;

            Vector3 pos = ctx.transform != null ? ctx.transform.position : Vector3.zero;

            // ===== SFX（一次性） =====
            if (data.explodeSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.explodeSfx);
            }

            // ===== VFX =====
            if (data.vfxPrefab != null)
            {
                Object.Instantiate(data.vfxPrefab, pos, Quaternion.identity);
            }

            // ===== Damage =====
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, data.radius, data.affectMask);
            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (col == null) continue;
                if (ctx.gameObject != null && col.gameObject == ctx.gameObject) continue;

                var dmg = col.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(data.damage, pos, ctx.gameObject);
                }
            }
        }
    }
}
