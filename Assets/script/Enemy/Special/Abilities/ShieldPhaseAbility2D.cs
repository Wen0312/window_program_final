using UnityEngine;

/// <summary>
/// ShieldPhaseAbility2D
/// 護盾 Phase：先打盾、再打血
/// - 不改 EnemyHealth
/// - OnDamaged 為事後通知，因此用 IHealable 嘗試補回（抵消）
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/Abilities/ShieldPhaseAbility2D", fileName = "ShieldPhaseAbility2D")]
public class ShieldPhaseAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Shield")]
    public float shieldHP = 20f;
    public bool invulnerableWhileShieldUp = true; // 護盾存在時嘗試抵消扣血

    [Header("Break Effects")]
    public AudioClip shieldBreakSfx;
    public GameObject shieldBreakVfx;

    [Header("Phase Unlock")]
    [Tooltip("護盾破裂後要解鎖的 Ability 索引（指向同一個 SpecialEnemyData2D.abilities）")]
    public int[] unlockAbilityIndices;

    [Header("Phase Lock (optional)")]
    [Tooltip("護盾存在時要鎖住的 Ability 索引（例如召喚/毒圈等）")]
    public int[] lockAbilityIndices; // 新增（可選）

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly ShieldPhaseAbility2D data;

        float currentShield;
        bool broken = false;
        bool locksApplied = false;

        public Runtime(ShieldPhaseAbility2D data)
        {
            this.data = data;
        }

        // 新增：統一從 Boss Root（parent chain）取 component，避免 ctx.gameObject 不是 root 時抓不到
        T GetFromBossRoot<T>(SpecialEnemyContext2D ctx) where T : Component
        {
            // SpecialEnemyContext2D 是 struct，不能 ctx == null

            // ctx.gameObject 可能是 Ability Host（子物件），因此用 InParent 往上找
            if (ctx.gameObject != null)
            {
                var c = ctx.gameObject.GetComponentInParent<T>();
                if (c != null) return c;
            }

            // fallback：用 transform 也往上找
            if (ctx.transform != null)
            {
                var c = ctx.transform.GetComponentInParent<T>();
                if (c != null) return c;
            }

            return null;
        }


        public override void Init(SpecialEnemyContext2D ctx)
        {
            currentShield = Mathf.Max(0f, data.shieldHP);

            // 新增：Boss 階段文字（如果有掛 BossPhaseState2D）
            var phase = GetFromBossRoot<BossPhaseState2D>(ctx); // 新增
            if (phase != null) phase.SetPhase("Shield");

            // 新增：護盾期先鎖能力（Phase 1）
            var controller = GetFromBossRoot<SpecialEnemyController2D>(ctx); // 新增
            if (controller != null && data.lockAbilityIndices != null && !locksApplied)
            {
                for (int i = 0; i < data.lockAbilityIndices.Length; i++)
                    controller.DisableAbilityByIndex(data.lockAbilityIndices[i]);
                locksApplied = true;
            }
        }

        public override void OnDamaged(SpecialEnemyContext2D ctx, float damage, Vector2 hitPoint, GameObject instigator)
        {
            if (broken) return;

            // 扣護盾
            currentShield -= Mathf.Max(0f, damage);

            // 嘗試抵消扣血（EnemyHealth 沒 Heal，用 IHealable）
            if (data.invulnerableWhileShieldUp && ctx.gameObject != null)
            {
                // interface 不能走 where T: Component 的泛型工具
                var healable = ctx.gameObject.GetComponentInParent<IHealable>();
                if (healable != null)
                {
                    healable.Heal(damage);
                }
            }

            if (currentShield <= 0f)
            {
                BreakShield(ctx);
            }
        }



        void BreakShield(SpecialEnemyContext2D ctx)
        {
            if (broken) return;
            broken = true;

            // SFX
            if (data.shieldBreakSfx != null)
                AudioManager_2D.Instance.PlayGameplaySFX(data.shieldBreakSfx);

            // VFX
            if (data.shieldBreakVfx != null && ctx.transform != null)
                Object.Instantiate(data.shieldBreakVfx, ctx.transform.position, Quaternion.identity);

            // 解鎖能力（Phase 2）
            var controller = GetFromBossRoot<SpecialEnemyController2D>(ctx); // 新增
            if (controller != null && data.unlockAbilityIndices != null)
            {
                for (int i = 0; i < data.unlockAbilityIndices.Length; i++)
                    controller.EnableAbilityByIndex(data.unlockAbilityIndices[i]);
            }

            // 新增：Boss 階段文字
            var phase = GetFromBossRoot<BossPhaseState2D>(ctx); // 新增
            if (phase != null) phase.SetPhase("Phase 2");
        }
    }
}
