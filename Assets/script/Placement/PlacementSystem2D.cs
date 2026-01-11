using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem2D : MonoBehaviour
{
    [Header("Grid")]
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    // 佔格：每格對應一個 obstacle（方便做拆除/查詢）
    readonly Dictionary<Vector2Int, ObstacleCore2D> occupied = new();

    void Awake()
    {
        // (可選但很實用) 場景一開始就把現有障礙物註冊進佔格
        // 這樣你用「地圖預擺」或 Scene 裡先放好的牆/桶/zone，不會被當成空格
        RegisterExistingObstacles();
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 p = world - origin;
        int x = Mathf.FloorToInt(p.x / cellSize);
        int y = Mathf.FloorToInt(p.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = (cell.x + 0.5f) * cellSize;
        float y = (cell.y + 0.5f) * cellSize;
        return origin + new Vector3(x, y, 0f);
    }

    static IEnumerable<Vector2Int> EnumerateFootprint(Vector2Int originCell, Vector2Int size)
    {
        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                yield return new Vector2Int(originCell.x + dx, originCell.y + dy);
    }

    public bool CanPlace(ObstacleData data, Vector2Int cell)
    {
        if (data == null) return false;
        var size = data.footprintCells;

        foreach (var c in EnumerateFootprint(cell, size))
            if (occupied.ContainsKey(c)) return false;

        return true;
    }
    public bool TryPlace(ObstacleData data, Vector3 worldPos, int rotation, out ObstacleCore2D placed)
    {
        placed = null;
        if (data == null || data.prefab == null) return false;

        Vector2Int cell = WorldToCell(worldPos);

        if (!CanPlace(data, cell)) return false;

        // 位置：用系統 cellSize，不用 data.gridSize（避免同場景多套 gridSize）
        Vector3 spawnPos = data.snapToGrid ? CellToWorldCenter(cell) : worldPos;

        Quaternion rotQ = Quaternion.Euler(0f, 0f, rotation);
        GameObject go = Instantiate(data.prefab, spawnPos, rotQ);

        var core = go.GetComponent<ObstacleCore2D>();
        if (core == null) core = go.AddComponent<ObstacleCore2D>();

        core.data = data;
        core.placedCell = cell;
        core.rotation = rotation;

        //關鍵：讓 core 知道自己屬於哪個 PlacementSystem（OnDestroy 會用來釋放佔格）
        core.ownerSystem = this;

        foreach (var c in EnumerateFootprint(cell, data.footprintCells))
            occupied[c] = core;

        placed = core;
        return true;
    }


    public bool TryGetAtCell(Vector2Int cell, out ObstacleCore2D core) =>
        occupied.TryGetValue(cell, out core);

    public void Remove(ObstacleCore2D core)
    {
        if (core == null || core.data == null) return;

        Unregister(core);
        Destroy(core.gameObject);
    }

    /// (可選但很實用) 場景一開始就把現有障礙物註冊進佔格

    public void RegisterExistingObstacles()
    {
        var cores = FindObjectsOfType<ObstacleCore2D>();
        foreach (var core in cores)
        {
            if (core == null || core.data == null) continue;

            // ★新增：預擺障礙也要綁定 ownerSystem，OnDestroy 才能清佔格
            core.ownerSystem = this;

            // 若 core 沒填 placedCell，就用目前位置推
            if (core.placedCell == default)
                core.placedCell = WorldToCell(core.transform.position);

            foreach (var c in EnumerateFootprint(core.placedCell, core.data.footprintCells))
                occupied[c] = core;
        }
    }

    // 只清佔格，不 Destroy（給 OnDestroy 用）
    public void Unregister(ObstacleCore2D core)
    {
        if (core == null || core.data == null) return;

        foreach (var c in EnumerateFootprint(core.placedCell, core.data.footprintCells))
        {
            if (occupied.TryGetValue(c, out var cur) && cur == core)
                occupied.Remove(c);
        }
    }

}
