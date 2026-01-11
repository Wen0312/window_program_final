using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPlaceObstacle2D : MonoBehaviour
{
    [Header("Obstacle Data")]
    public ObstacleData[] obstacleList;     // 1~n 種可放置物
    public int currentIndex = 0;            // 目前選到哪一個
    public ObstacleData currentObstacle;    // 目前障礙物（給舊邏輯用）

    [Header("Placement")]
    public LayerMask blockMask;             // Obstacle / Wall / Enemy / Player
    public Transform placeOrigin;           // optional

    [Header("Preview")]
    public bool previewEnabled = false;     // 由 PlayerInputMode2D 控制
    [Range(0.05f, 1f)]
    public float previewAlpha = 0.35f;      // Base sprite 的透明度
    [Range(0.05f, 1f)]
    public float previewTintAlpha = 0.25f;  // 紅/綠遮罩的透明度
    public int previewSortingOrder = 1000;  // 確保在最上層

    [Header("Rotation")]
    [Tooltip("Rotation in degrees (0 / 90 / 180 / 270)")]
    public int currentRotation = 0;

    [Header("Remove / Demolish")]
    [Tooltip("Seconds between removals to prevent spam-click.")]
    public float removeCooldown = 0.05f;

    // 一個預覽物件：底圖 + 遮罩
    GameObject previewGO;

    // 這次改成：直接複製 prefab 的 Visual（包含 AutoFitSprite2D / localScale）
    Transform previewBaseRoot;              // Base Visual root
    Transform previewTintRoot;              // Tint Visual root
    SpriteRenderer previewBaseSR;           // Base sprite renderer
    SpriteRenderer previewTintSR;           // Tint sprite renderer

    // cache：避免每次都重建
    ObstacleData cachedData;
    GameObject cachedVisualSourceGO;        // prefab 裡那個 SpriteRenderer 所在的 GO（通常就是 Visual）

    [Tooltip("Shared placement system (grid + occupancy).")]
    public PlacementSystem2D placementSystem;

    Camera cam;
    float nextPlaceTime;
    float nextRemoveTime;

    void Awake()
    {
        if (placementSystem == null) placementSystem = FindObjectOfType<PlacementSystem2D>();
        cam = Camera.main;
        if (placeOrigin == null) placeOrigin = transform;

        // 初始選擇
        SyncCurrentObstacle();

        // Create preview ghost (hidden by default)
        CreatePreviewGhost();
        SetPreviewActive(previewEnabled);

        // 先同步一次 visual（含 AutoFitSprite2D）
        RefreshPreviewVisual();
    }

    void SetIndex(int idx)
    {
        if (obstacleList == null) return;
        if (idx < 0 || idx >= obstacleList.Length) return;

        currentIndex = idx;
        SyncCurrentObstacle();
    }

    void SyncCurrentObstacle()
    {
        if (obstacleList != null && obstacleList.Length > 0)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, obstacleList.Length - 1);
            currentObstacle = obstacleList[currentIndex];
        }

        // 切換障礙物後刷新預覽 visual（含 AutoFitSprite2D）
        RefreshPreviewVisual();
    }

    Vector2 GetPlacePosition()
    {
        Vector3 mouse = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse.z = 0f;

        // 使用 PlacementSystem 的格子中心，確保「預覽位置」與「實際放置位置」一致
        if (placementSystem != null && currentObstacle != null && currentObstacle.snapToGrid)
        {
            Vector2Int cell = placementSystem.WorldToCell(mouse);
            return placementSystem.CellToWorldCenter(cell);
        }

        return mouse;
    }

    bool CanPlaceAt(Vector2 pos)
    {
        if (currentObstacle == null) return false;

        // 先用佔格系統判斷（主要規則）
        if (placementSystem != null)
        {
            Vector2Int cell = placementSystem.WorldToCell(pos);
            if (!placementSystem.CanPlace(currentObstacle, cell))
                return false;
        }

        // 再用物理當防呆（縮小一點避免貼邊誤判）
        Vector2 size = currentObstacle.FootprintWorld * 0.9f;
        Collider2D hit = Physics2D.OverlapBox(pos, size, 0f, blockMask);
        return hit == null;
    }

    // --------------------
    // Preview (Clone Prefab Visual + Tint Overlay)
    // --------------------

    void CreatePreviewGhost()
    {
        previewGO = new GameObject("PlacementPreview");
        previewGO.transform.SetParent(null);
        previewGO.SetActive(false);
    }

    void SetPreviewActive(bool active)
    {
        if (previewGO == null) return;

        if (!active)
        {
            previewGO.SetActive(false);
            return;
        }

        RefreshPreviewVisual();
        previewGO.SetActive(true);
    }

    // 核心：直接複製 prefab 裡的 Visual（包含 AutoFitSprite2D / localScale），確保預覽大小 == 實際大小
    void RefreshPreviewVisual()
    {
        if (previewGO == null) return;

        if (currentObstacle == null || currentObstacle.prefab == null)
        {
            cachedData = null;
            cachedVisualSourceGO = null;
            DestroyPreviewChildren();
            return;
        }

        if (cachedData == currentObstacle && cachedVisualSourceGO != null) return;

        cachedData = currentObstacle;

        var srcSR = currentObstacle.prefab.GetComponentInChildren<SpriteRenderer>();
        if (srcSR == null)
        {
            cachedVisualSourceGO = null;
            DestroyPreviewChildren();
            return;
        }

        cachedVisualSourceGO = srcSR.gameObject;

        DestroyPreviewChildren();

        // Base：原 sprite 半透明
        previewBaseRoot = Instantiate(cachedVisualSourceGO, previewGO.transform).transform;
        previewBaseRoot.name = "BaseVisual";
        previewBaseSR = previewBaseRoot.GetComponent<SpriteRenderer>();

        // Tint：同一份 Visual 再複製一份，用來疊色
        previewTintRoot = Instantiate(cachedVisualSourceGO, previewGO.transform).transform;
        previewTintRoot.name = "TintVisual";
        previewTintSR = previewTintRoot.GetComponent<SpriteRenderer>();

        ApplySorting(previewBaseRoot, previewSortingOrder);
        ApplySorting(previewTintRoot, previewSortingOrder + 1);

        DisableAllColliders(previewBaseRoot);
        DisableAllColliders(previewTintRoot);

        UpdatePreviewColors(true);
        ApplyPreviewRotation();
    }

    void DestroyPreviewChildren()
    {
        if (previewBaseRoot != null) Destroy(previewBaseRoot.gameObject);
        if (previewTintRoot != null) Destroy(previewTintRoot.gameObject);

        previewBaseRoot = null;
        previewTintRoot = null;
        previewBaseSR = null;
        previewTintSR = null;
    }

    void ApplySorting(Transform root, int baseOrder)
    {
        var srs = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            sr.sortingOrder = baseOrder + sr.sortingOrder;
        }
    }

    void DisableAllColliders(Transform root)
    {
        var cols = root.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) c.enabled = false;
    }

    void UpdatePreview()
    {
        if (previewGO == null) return;

        if (currentObstacle == null || currentObstacle.prefab == null)
        {
            previewGO.SetActive(false);
            return;
        }

        if (!previewGO.activeSelf) previewGO.SetActive(true);

        RefreshPreviewVisual();

        Vector2 pos = GetPlacePosition();
        previewGO.transform.position = pos;
        ApplyPreviewRotation();
        bool canPlace = CanPlaceAt(pos);
        UpdatePreviewColors(canPlace);
    }

    void UpdatePreviewColors(bool canPlace)
    {
        if (previewBaseSR == null || previewTintSR == null) return;

        Color baseC = Color.white;
        baseC.a = previewAlpha;
        previewBaseSR.color = baseC;

        Color tintC = canPlace ? Color.green : Color.red;
        tintC.a = previewTintAlpha;
        previewTintSR.color = tintC;
    }

    // =========================
    // Public API for InputMode
    // =========================

    public void SetIndex_Public(int idx)
    {
        SetIndex(idx);
    }

    public bool TryPlaceOnce_Public()
    {
        if (currentObstacle == null || currentObstacle.prefab == null) return false;
        if (Time.time < nextPlaceTime) return false;

        Vector2 placePos = GetPlacePosition();
        if (!CanPlaceAt(placePos)) return false;

        bool ok = placementSystem != null
            ? placementSystem.TryPlace(currentObstacle, placePos, currentRotation, out _)
            : false;

        if (placementSystem == null)
        {
            var go = Instantiate(currentObstacle.prefab, placePos, Quaternion.identity);
            var core = go.GetComponent<ObstacleCore2D>();
            if (core != null) core.data = currentObstacle;
            ok = true;
        }

        if (!ok) return false;

        nextPlaceTime = Time.time + currentObstacle.placeCooldown;
        return true;
    }

    /// <summary>
    /// Build 模式的「拆除」入口：拆掉滑鼠所在格子的已放置障礙物（不走血量）。
    /// 回傳 true 表示真的拆到東西。
    /// </summary>
    public bool TryRemoveOnce_Public()
    {
        if (placementSystem == null) return false;
        if (Time.time < nextRemoveTime) return false;

        // 用同一套滑鼠->格子流程，確保跟放置/預覽一致
        Vector2 pos = GetPlacePosition();
        Vector2Int cell = placementSystem.WorldToCell(pos);

        if (!placementSystem.TryGetAtCell(cell, out var core)) return false;
        if (core == null) return false;

        placementSystem.Remove(core);

        nextRemoveTime = Time.time + Mathf.Max(0f, removeCooldown);
        return true;
    }

    public void SetPreviewVisible_Public(bool visible)
    {
        previewEnabled = visible;
        SetPreviewActive(visible);
    }

    // 讓 InputMode 在 legacy input 關閉時，也能每幀更新預覽位置與紅/綠狀態
    public void UpdatePreview_Public()
    {
        if (!previewEnabled) return;
        UpdatePreview();
    }

    public void Rotate_Public(int delta)
    {
        // delta = +1 or -1
        currentRotation = (currentRotation + delta * 90) % 360;
        if (currentRotation < 0) currentRotation += 360;

        // 預覽要即時轉
        ApplyPreviewRotation();
    }

    void ApplyPreviewRotation()
    {
        if (previewGO == null) return;
        previewGO.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
}
