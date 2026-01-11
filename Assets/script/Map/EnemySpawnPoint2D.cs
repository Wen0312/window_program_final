using UnityEngine;

public class EnemySpawnPoint2D : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Optional")]
    [Tooltip("If true, this spawn is only for Editor (map editing) preview and should not spawn in Play.")]
    public bool editorOnly = false;

    [Tooltip("Spawn rotation in degrees (Z).")]
    public float rotationDeg = 0f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.5f);
    }
}
