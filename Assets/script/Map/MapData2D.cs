using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map/Map Data 2D", fileName = "MapData2D_")]
public class MapData2D : ScriptableObject
{
    [Header("Identity")]
    public string mapId = "map_001";
    public string displayName = "Map 001";

    [Header("Grid")]
    [Tooltip("Grid cell size (world units). Should match PlacementSystem2D grid size logic.")]
    public float gridSize = 1f;

    //Player spawn position in grid cell coordinates.
    [Header("Player Spawn")]
    [Tooltip("Player spawn position in grid cell coordinates.")]
    public Vector2Int playerSpawnCell = new Vector2Int(0, 0);

    [Tooltip("If true, use playerSpawnCell as spawn. If false, keep scene player's current position.")]
    public bool usePlayerSpawnCell = true;


    [Tooltip("Map size in cells (for editor/validation; runtime can ignore if you want).")]
    public Vector2Int mapSizeCells = new Vector2Int(30, 20);

    // =========================
    // Loadouts / Palettes
    // =========================

    [Header("Loadouts (Legacy / Default)")]
    [Tooltip("Legacy single set. Kept for backward compatibility with existing assets/scenes.")]
    public WeaponLoadoutData weaponLoadout;

    [Tooltip("Legacy single set. Kept for backward compatibility with existing assets/scenes.")]
    public BuildPaletteData buildPalette;

    [Header("Loadouts (Play)")]
    [Tooltip("Weapons allowed during gameplay.")]
    public WeaponLoadoutData playWeaponLoadout;

    [Tooltip("Obstacles allowed during gameplay/build mode.")]
    public BuildPaletteData playBuildPalette;

    [Header("Loadouts (Editor)")]
    [Tooltip("Weapons allowed during map editing (often empty or same as play).")]
    public WeaponLoadoutData editorWeaponLoadout;

    [Tooltip("Obstacles allowed during map editing (usually larger than play).")]
    public BuildPaletteData editorBuildPalette;

    // =========================
    // Mode
    // =========================

    [Header("Mode")]
    [Tooltip("If true, start in Build mode (useful for map editor).")]
    public bool startInBuildMode = false;

    [Tooltip("If true, use Editor loadouts/palette; otherwise use Play loadouts/palette.")]
    public bool useEditorLoadouts = false;

    // =========================
    // Preset Placements (hand-authored map)
    // =========================

    [System.Serializable]
    public class PresetObstacle
    {
        public ObstacleData obstacle;
        public Vector2Int cell;     // Grid cell coordinate
        [Tooltip("Rotation in degrees (0/90/180/270)")]
        public int rotation;        // 0/90/180/270
    }

    [Header("Preset Placements")]
    [Tooltip("Hand-authored obstacles placed on the map at start (not random).")]
    public PresetObstacle[] presetObstacles;

    [System.Serializable]
    public class PresetEnemy
    {
        public GameObject enemyPrefab;
        public Vector2 position;
        public float rotationDeg;
    }

    [Header("Preset Enemies")]
    public PresetEnemy[] presetEnemies;

    // =========================
    // 新增：背景設定
    // =========================
    [Header("Background")]
    public string backgroundId;

}
