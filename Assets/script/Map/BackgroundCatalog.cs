using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BackgroundCatalog
/// 
/// 集中管理所有 BackgroundData
/// 提供 backgroundId → BackgroundData 的查找
/// </summary>
[CreateAssetMenu(
    fileName = "BackgroundCatalog",
    menuName = "Map/Background Catalog",
    order = 21)]
public class BackgroundCatalog : ScriptableObject
{
    public List<BackgroundData> backgrounds = new List<BackgroundData>();

    Dictionary<string, BackgroundData> lookup;

    void BuildLookupIfNeeded()
    {
        if (lookup != null) return;

        lookup = new Dictionary<string, BackgroundData>();
        foreach (var bg in backgrounds)
        {
            if (bg == null) continue;
            if (string.IsNullOrEmpty(bg.backgroundId)) continue;

            if (!lookup.ContainsKey(bg.backgroundId))
                lookup.Add(bg.backgroundId, bg);
        }
    }

    public BackgroundData GetById(string backgroundId)
    {
        if (string.IsNullOrEmpty(backgroundId))
            return null;

        BuildLookupIfNeeded();

        lookup.TryGetValue(backgroundId, out var bg);
        return bg;
    }
}
