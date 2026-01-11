#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// MapData2DEditor
/// 
/// 提供地圖背景選擇（Editor Only）
/// - 下拉選 BackgroundData
/// - 寫入 map.backgroundId
/// - 即時預覽 Scene 的 GROUND
/// </summary>
[CustomEditor(typeof(MapData2D))]
public class MapData2DEditor : Editor
{
    MapData2D map;

    BackgroundCatalog backgroundCatalog;
    string[] backgroundNames;
    int currentIndex = -1;

    void OnEnable()
    {
        map = (MapData2D)target;
        LoadBackgroundCatalog();
        SyncCurrentIndex();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Background (Editor)", EditorStyles.boldLabel);

        if (backgroundCatalog == null || backgroundNames == null)
        {
            EditorGUILayout.HelpBox(
                "BackgroundCatalog not found.\n" +
                "Please create one and assign BackgroundData assets.",
                MessageType.Warning
            );
            return;
        }

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUILayout.Popup(
            "Background",
            currentIndex,
            backgroundNames
        );

        if (EditorGUI.EndChangeCheck())
        {
            ApplyBackgroundByIndex(newIndex);
        }

        // 顯示目前存的 backgroundId（debug 用）
        EditorGUILayout.LabelField(
            "Saved backgroundId",
            string.IsNullOrEmpty(map.backgroundId) ? "(none)" : map.backgroundId
        );
    }

    // =========================
    // Internal
    // =========================

    void LoadBackgroundCatalog()
    {
        string[] guids = AssetDatabase.FindAssets("t:BackgroundCatalog");
        if (guids.Length == 0) return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        backgroundCatalog = AssetDatabase.LoadAssetAtPath<BackgroundCatalog>(path);

        if (backgroundCatalog == null) return;

        backgroundNames = new string[backgroundCatalog.backgrounds.Count];
        for (int i = 0; i < backgroundNames.Length; i++)
        {
            var bg = backgroundCatalog.backgrounds[i];
            backgroundNames[i] = bg != null
                ? $"{bg.backgroundId} ({bg.name})"
                : "(null)";
        }
    }

    void SyncCurrentIndex()
    {
        if (backgroundCatalog == null) return;

        currentIndex = -1;
        for (int i = 0; i < backgroundCatalog.backgrounds.Count; i++)
        {
            var bg = backgroundCatalog.backgrounds[i];
            if (bg == null) continue;

            if (bg.backgroundId == map.backgroundId)
            {
                currentIndex = i;
                break;
            }
        }
    }

    void ApplyBackgroundByIndex(int index)
    {
        if (index < 0 || index >= backgroundCatalog.backgrounds.Count)
            return;

        var bg = backgroundCatalog.backgrounds[index];
        if (bg == null) return;

        Undo.RecordObject(map, "Change Map Background");
        map.backgroundId = bg.backgroundId;
        EditorUtility.SetDirty(map);

        currentIndex = index;

        // 即時套到 Scene 的 GROUND（Editor Preview）
        ApplyPreviewToScene(bg);
    }

    void ApplyPreviewToScene(BackgroundData bg)
    {
        var ground = GameObject.Find("GROUND");
        if (ground == null) return;

        var sr = ground.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sprite = bg.backgroundSprite;

        if (!string.IsNullOrEmpty(bg.sortingLayerName))
            sr.sortingLayerName = bg.sortingLayerName;

        sr.sortingOrder = bg.sortingOrder;

        SceneView.RepaintAll();
    }
}
#endif
