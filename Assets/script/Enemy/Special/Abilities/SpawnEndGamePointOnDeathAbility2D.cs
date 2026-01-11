using UnityEngine;

/// <summary>
/// SpawnEndGamePointOnDeathAbility2D
/// Boss 死亡時生成 EndGamePoint（過關點）
/// - 不改 EnemyHealth
/// - 不影響 EnemyHealth.dropPrefab（Boss 還是可以掉一般戰利品）
/// - 透過 SpecialEnemyController2D 的 OnDeath 觸發
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/Abilities/SpawnEndGamePointOnDeathAbility2D", fileName = "SpawnEndGamePointOnDeathAbility2D")]
public class SpawnEndGamePointOnDeathAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Prefab")]
    public GameObject endGamePointPrefab; // 拖 EndGamePoint.prefab 進來

    [Header("Offset")]
    public Vector2 spawnOffset = Vector2.zero; // 可選：生成偏移（例如稍微往上）

    [Header("Optional")]
    public AudioClip spawnSfx;     // 可選
    public GameObject spawnVfx;    // 可選

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly SpawnEndGamePointOnDeathAbility2D data;
        bool done = false;

        public Runtime(SpawnEndGamePointOnDeathAbility2D data)
        {
            this.data = data;
        }

        public override void OnDeath(SpecialEnemyContext2D ctx)
        {
            if (done) return;
            done = true;

            if (data.endGamePointPrefab == null) return;

            Vector3 pos = ctx.transform != null ? ctx.transform.position : Vector3.zero;
            pos += (Vector3)data.spawnOffset;

            // SFX（一次性）
            if (data.spawnSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.spawnSfx);
            }

            // VFX（可選）
            if (data.spawnVfx != null)
            {
                Object.Instantiate(data.spawnVfx, pos, Quaternion.identity);
            }

            // 生成 EndGamePoint
            Object.Instantiate(data.endGamePointPrefab, pos, Quaternion.identity);
        }
    }
}
