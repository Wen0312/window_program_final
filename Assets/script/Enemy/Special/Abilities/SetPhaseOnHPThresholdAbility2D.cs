using UnityEngine;

/// <summary>
/// SetPhaseOnHPThresholdAbility2D
/// 低血量切換 Boss Phase（HUD 顯示用）
/// - 不改 EnemyAI / EnemyHealth
/// - 透過 EnemyHealth.OnHPChanged 取得 current/max
/// - 只觸發一次
/// </summary>
[CreateAssetMenu(
    menuName = "Game/Enemy/Abilities/SetPhaseOnHPThresholdAbility2D",
    fileName = "SetPhaseOnHPThresholdAbility2D"
)]
public class SetPhaseOnHPThresholdAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Trigger")]
    [Range(0f, 1f)]
    public float hpPercentThreshold = 0.25f;

    [Tooltip("HUD 顯示的 Phase 名稱，例如：Berserker Mode")]
    public string phaseName = "Berserker Mode";

    [Header("Ability Control (optional)")]
    public int[] unlockAbilityIndices;
    public int[] lockAbilityIndices;

    [Header("Phase Enter SFX")]
    public AudioClip phaseEnterSfx;   //  在 Inspector 指定 boss scream

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly SetPhaseOnHPThresholdAbility2D data;

        bool triggered = false;

        public Runtime(SetPhaseOnHPThresholdAbility2D data)
        {
            this.data = data;
        }

        public override void OnHPChanged(SpecialEnemyContext2D ctx, float current, float max)
        {
            if (triggered) return;
            if (max <= 0f) return;

            float ratio = current / max;

            if (ratio <= data.hpPercentThreshold)
            {
                triggered = true;
                TriggerPhase(ctx);
            }
        }

        void TriggerPhase(SpecialEnemyContext2D ctx)
        {
            // =========================
            // 1️設定 Phase
            // =========================
            BossPhaseState2D phase = null;

            if (ctx.gameObject != null)
                phase = ctx.gameObject.GetComponentInParent<BossPhaseState2D>();

            if (phase != null)
            {
                phase.SetPhase(
                    string.IsNullOrEmpty(data.phaseName)
                        ? "Phase 3"
                        : data.phaseName
                );
            }

            // =========================
            // 2️ 播放 Phase 進入音效
            // =========================
            if (data.phaseEnterSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.phaseEnterSfx);
            }

            // =========================
            // 3️ Ability 解鎖 / 鎖定
            // =========================
            SpecialEnemyController2D controller = null;

            if (ctx.gameObject != null)
                controller = ctx.gameObject.GetComponentInParent<SpecialEnemyController2D>();

            if (controller != null)
            {
                if (data.lockAbilityIndices != null)
                {
                    foreach (int i in data.lockAbilityIndices)
                        controller.DisableAbilityByIndex(i);
                }

                if (data.unlockAbilityIndices != null)
                {
                    foreach (int i in data.unlockAbilityIndices)
                        controller.EnableAbilityByIndex(i);
                }
            }
        }
    }
}
