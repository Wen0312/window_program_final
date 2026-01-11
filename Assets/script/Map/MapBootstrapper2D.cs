using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapBootstrapper2D : MonoBehaviour
{
    [Header("Map")]
    public MapData2D map;

    [Header("Targets")]
    public PlayerWeaponInput weaponInput;
    public PlayerPlaceObstacle2D placer;
    public PlayerInputMode2D inputMode;
    public PlacementSystem2D placementSystem;

    [Header("Catalog")]
    public MapCatalog2D catalog;   // Ãö¥d²M³æ
    public int startMapIndex = 0;  // °_©l²Ä´XÃö

    [Header("Background")]
    public BackgroundCatalog backgroundCatalog;
    public SpriteRenderer groundRenderer; // «ü¦V GROUND ªº SpriteRenderer

    private bool shouldPlayBGM = false;

    void Awake()
    {
        // ¥Ñ GameManager ±±¨îÃö¥d¯Á¤Þ
        startMapIndex = GameManager.CurrentMapIndex;

        // ¦Û°Ê§ì¡]Á×§K Inspector º|©ì¡^
        if (weaponInput == null) weaponInput = FindObjectOfType<PlayerWeaponInput>();
        if (placer == null) placer = FindObjectOfType<PlayerPlaceObstacle2D>();
        if (inputMode == null) inputMode = FindObjectOfType<PlayerInputMode2D>();
        if (placementSystem == null) placementSystem = FindObjectOfType<PlacementSystem2D>();

        shouldPlayBGM = GameManager.IsLoadingNextLevel;
    }

    void Start()
    {
        // === Step 1: ±q Catalog ¨ú±o Map ===
        if (map == null && catalog != null && catalog.maps != null && catalog.maps.Length > 0)
        {
            startMapIndex = Mathf.Clamp(startMapIndex, 0, catalog.maps.Length - 1);
            map = catalog.maps[startMapIndex];
        }

        if (map == null)
        {
            Debug.LogWarning("MapBootstrapper2D: No map assigned.");
            return;
        }
        GameManager.MaxUnlockedMapIndex = GameManager.CurrentMapIndex;


        // =========================
        // Step 2: ®M¥Î­I´º¡]¤@©w­n³Ì¦­¡^
        // =========================
        ApplyBackground(map);

        // =========================
        // Step 3: ®M¥Î Loadout / Mode
        // =========================
        ApplyLoadouts(map.useEditorLoadouts);

        if (inputMode != null)
        {
            inputMode.mode = map.startInBuildMode
                ? PlayerInputMode2D.Mode.Build
                : PlayerInputMode2D.Mode.Combat;

            if (placer != null)
                placer.SetPreviewVisible_Public(map.startInBuildMode);
        }

        // =========================
        // Step 4: ¹w³]ª«¥ó / ª±®a / ¼Ä¤H
        // =========================
        ApplyPresetPlacements();

        if (map.usePlayerSpawnCell && placementSystem != null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 spawnPos = placementSystem.CellToWorldCenter(map.playerSpawnCell);
                player.transform.position = spawnPos;
            }
        }

        ApplyPresetEnemies();
        // ★★★ 在 Start 的最後面加入這一行 ★★★
        if (shouldPlayBGM)
        {
            if (AudioManager_2D.Instance != null)
            {
                AudioManager_2D.Instance.PlayBGMByLevelIndex(GameManager.CurrentMapIndex);
            }
        }
    }

    // =========================
    // Loadout
    // =========================
    public void ApplyLoadouts(bool useEditor)
    {
        if (map == null) return;

        GameRuntimeFlags2D.developerProfile = useEditor;
        map.useEditorLoadouts = useEditor;

        if (inputMode != null && inputMode.cameraFollow != null)
        {
            inputMode.cameraFollow.SetEditorProfile_Public(useEditor);
        }

        WeaponLoadoutData loadout = null;
        BuildPaletteData palette = null;

        if (useEditor)
        {
            loadout = map.editorWeaponLoadout != null ? map.editorWeaponLoadout : map.weaponLoadout;
            palette = map.editorBuildPalette != null ? map.editorBuildPalette : map.buildPalette;
        }
        else
        {
            loadout = map.playWeaponLoadout != null ? map.playWeaponLoadout : map.weaponLoadout;
            palette = map.playBuildPalette != null ? map.playBuildPalette : map.buildPalette;
        }

        if (weaponInput != null && loadout != null)
        {
            weaponInput.weapons = loadout.weapons;

            if (weaponInput.weaponController != null && weaponInput.weapons.Length > 0)
            {
                weaponInput.weaponController.SetWeapon(weaponInput.weapons[0]);
            }
        }

        if (placer != null && palette != null)
        {
            placer.obstacleList = palette.obstacles;
            placer.SetIndex_Public(0);
        }
    }

    // =========================
    // Background
    // =========================
    void ApplyBackground(MapData2D map)
    {
        if (map == null) return;
        if (backgroundCatalog == null) return;
        if (groundRenderer == null) return;

        var bg = backgroundCatalog.GetById(map.backgroundId);
        if (bg == null)
        {
            Debug.LogWarning($"[MapBootstrapper2D] Background not found: {map.backgroundId}");
            return;
        }

        // ®M Sprite
        groundRenderer.sprite = bg.backgroundSprite;

        // Sorting
        if (!string.IsNullOrEmpty(bg.sortingLayerName))
            groundRenderer.sortingLayerName = bg.sortingLayerName;

        groundRenderer.sortingOrder = bg.sortingOrder;

        // =========================
        // ·s¼W¡G¥Î Tiled ¾Qº¡¾ã±i¦a¹Ï
        // =========================
        groundRenderer.drawMode = SpriteDrawMode.Tiled;

        // Map ¥@¬É¤Ø¤o¡]¨Ì§Aªº MapData Äæ¦ì½Õ¾ã¡^
        // gridSize=1 ªº¸Ü´N¬O cell ¼Æ¡F¦pªG gridSize ¤£¬O 1¡A¥Nªí¨C®æªº¥@¬É³æ¦ì¤j¤p
        float cellWorldSize = Mathf.Max(0.0001f, map.gridSize); // §A MapData2D ¸Ì¬Ý°_¨Ó¦³ gridSize
        Vector2 worldSize = new Vector2(
            map.mapSizeCells.x * cellWorldSize,
            map.mapSizeCells.y * cellWorldSize
        );

        groundRenderer.size = worldSize;

        //¡]¥i¿ï¡^½T«O¦a¹Ï¤¤¤ß¹ï»ô
        groundRenderer.transform.position = Vector3.zero;
    }


    // =========================
    // Preset Obstacles
    // =========================
    void ApplyPresetPlacements()
    {
        if (map == null) return;
        if (placementSystem == null) return;
        if (map.presetObstacles == null || map.presetObstacles.Length == 0) return;

        foreach (var p in map.presetObstacles)
        {
            if (p == null || p.obstacle == null) continue;

            Vector2 worldPos = placementSystem.CellToWorldCenter(p.cell);
            placementSystem.TryPlace(p.obstacle, worldPos, p.rotation, out _);
        }
    }

    void ApplyPresetEnemies()
    {
        if (map == null) return;
        if (map.presetEnemies == null || map.presetEnemies.Length == 0) return;

        foreach (var e in map.presetEnemies)
        {
            if (e == null || e.enemyPrefab == null) continue;

            Quaternion rot = Quaternion.Euler(0f, 0f, e.rotationDeg);
            Instantiate(e.enemyPrefab, e.position, rot);
        }

        Debug.Log($"Spawned {map.presetEnemies.Length} preset enemies.");
    }

#if UNITY_EDITOR
    [ContextMenu("Bake Scene Obstacles To MapData2D")]
    void BakeSceneToMap()
    {
        if (map == null)
        {
            Debug.LogWarning("MapBootstrapper2D: map is null.");
            return;
        }

        if (placementSystem == null)
            placementSystem = FindObjectOfType<PlacementSystem2D>();

        if (placementSystem == null)
        {
            Debug.LogWarning("MapBootstrapper2D: placementSystem not found.");
            return;
        }

        var cores = FindObjectsOfType<ObstacleCore2D>();
        var list = new System.Collections.Generic.List<MapData2D.PresetObstacle>();

        foreach (var c in cores)
        {
            if (c == null || c.data == null) continue;

            var po = new MapData2D.PresetObstacle();
            po.obstacle = c.data;
            po.cell = placementSystem.WorldToCell(c.transform.position);
            po.rotation = c.rotation;
            list.Add(po);
        }

        map.presetObstacles = list.ToArray();
        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssets();

        Debug.Log($"Baked {map.presetObstacles.Length} obstacles into {map.name}");
    }
#endif
}
