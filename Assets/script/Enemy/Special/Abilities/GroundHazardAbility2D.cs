using UnityEngine;

/// <summary>
/// GroundHazardAbility2D
/// 定期在地面生成危險區（毒 / 火 / 緩速）
/// - 不攻擊玩家
/// - 只控制場地（逼走位）
/// - Hazard 行為完全交給 prefab（StatusZone）
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/Abilities/GroundHazardAbility2D", fileName = "GroundHazardAbility2D")]
public class GroundHazardAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Hazard")]
    public GameObject hazardPrefab;   // PoisonZone / SlowZone / FireZone
    public float spawnRadius = 3f;

    [Header("Timing")]
    public float cooldown = 5f;
    public float firstDelay = 1f;

    [Header("Optional")]
    public AudioClip spawnSfx;
    public GameObject spawnVfx;

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly GroundHazardAbility2D data;
        float nextTime;

        public Runtime(GroundHazardAbility2D data)
        {
            this.data = data;
        }

        public override void Init(SpecialEnemyContext2D ctx)
        {
            nextTime = Time.time + Mathf.Max(0f, data.firstDelay);
        }

        public override void Tick(SpecialEnemyContext2D ctx, float dt)
        {
            if (ctx.transform == null) return;
            if (data.hazardPrefab == null) return;

            if (Time.time < nextTime) return;

            Vector3 center = ctx.transform.position;
            Vector2 offset = Random.insideUnitCircle * data.spawnRadius;
            Vector3 pos = center + (Vector3)offset;

            // SFX
            if (data.spawnSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.spawnSfx);
            }

            // VFX
            if (data.spawnVfx != null)
            {
                Object.Instantiate(data.spawnVfx, pos, Quaternion.identity);
            }

            Object.Instantiate(data.hazardPrefab, pos, Quaternion.identity);

            nextTime = Time.time + Mathf.Max(0.1f, data.cooldown);
        }
    }
}
