using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Obstacles/Obstacle Data", fileName = "ObstacleData_")]
public class ObstacleData : ScriptableObject
{
    [Header("Identity")]
    public string id = "wall";
    public string displayName = "Wall";

    [Header("Prefab")]
    public GameObject prefab;

    // =========================
    // Placement (Grid-based)
    // =========================

    [Header("Placement")]
    [Tooltip("Grid footprint in cells (Minecraft / Rimworld style).")]
    // 舊資產欄位名是 footprintSize (Vector2Int)，但新系統使用 footprintCells。
    // 用 FormerlySerializedAs 讓 Unity 自動把舊欄位搬到 footprintCells，避免佔格永遠是 1x1。
    [FormerlySerializedAs("footprintSize")]
    public Vector2Int footprintCells = Vector2Int.one;   //新增：佔幾格（1x1, 2x2...）

    [Tooltip("If true, placement will snap to a grid in the placer.")]
    public bool snapToGrid = true;

    [Tooltip("Grid cell size (world units). Usually 1.")]
    public float gridSize = 1f;

    [Tooltip("Seconds between placements for this obstacle.")]
    public float placeCooldown = 0.25f;

    [Tooltip("If true, obstacle blocks movement (wall/oil barrel).")]
    public bool blocksMovement = true;

    // =========================
    // Placement (World-based, legacy / compatibility)
    // =========================

    [Header("Placement (World / Legacy)")]
    [Tooltip("Used by placement overlap check (world units).")]
    // Legacy：舊版以世界座標尺寸做重疊檢查，保留但改名避免與舊資產欄位 footprintSize 撞名。
    public Vector2 legacyFootprintSize = new Vector2(1f, 1f);

    /// <summary>
    /// World-space footprint derived from grid footprint.
    /// Prefer this over footprintSize for new systems.
    /// </summary>
    public Vector2 FootprintWorld =>
        new Vector2(footprintCells.x * gridSize, footprintCells.y * gridSize);

    // =========================
    // Durability / Lifetime
    // =========================

    [Header("Durability")]
    [Tooltip("<= 0 means indestructible / no health (e.g., poison zone).")]
    public int maxHP = 50;

    [Header("Lifetime")]
    [Tooltip("<= 0 means infinite lifetime.")]
    public float lifetime = 0f;

    // =========================
    // Optional VFX / SFX
    // =========================

    [Header("SFX")]
    public AudioClip placeSfx;
    public AudioClip hitSfx;
    public AudioClip destroySfx;
}
