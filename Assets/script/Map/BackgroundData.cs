using UnityEngine;

/// <summary>
/// BackgroundData
/// 
/// 描述一種「地圖背景」的資料（純資料）
/// - 不處理載入流程
/// - 不知道 Map
/// - 不知道 UI
/// 
/// 由 MapBootstrapper2D 依 MapData.backgroundId 套用
/// </summary>
[CreateAssetMenu(
    fileName = "BG_",
    menuName = "Map/Background Data",
    order = 20)]
public class BackgroundData : ScriptableObject
{
    [Header("ID")]
    public string backgroundId; // 供 MapData 記錄與查找用（必填）

    [Header("Visual")]
    public Sprite backgroundSprite; // 單張背景圖（GROUND 用）

    [Header("Render Settings")]
    public string sortingLayerName = "Background";
    public int sortingOrder = 0;

    // =========================
    // 未來可擴充（先不實作）
    // =========================
    // public bool useParallax;
    // public float parallaxFactor;
    // public Color tint;
}
