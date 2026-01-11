using UnityEngine;


public class ObstacleCore2D : MonoBehaviour
{
    [Header("Data")]
    public ObstacleData data;

    // =========================
    // Placement / MapData info
    // =========================

    [HideInInspector]
    public Vector2Int placedCell;   //©ñ¦b­þ­Ó grid cell¡]MapData / Occupancy ¥Î¡^

    [HideInInspector]
    public int rotation;            // 0 / 90 / 180 / 270¡]¥ý«O¯d¡A¤§«á¤ä´©±ÛÂà¡^

    // =========================
    // PlacementSystem reference
    // =========================

    [HideInInspector]
    public PlacementSystem2D ownerSystem;   // ¥Ñ PlacementSystem2D.TryPlace() «ü¬£¡]¥Î¨ÓÄÀ©ñ¦û®æ¡^

    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        if (data == null) return;

        // --- 修改開始 ---
        // 判斷場景載入時間。如果小於 0.5 秒，視為地圖初始化生成，不播放音效。
        // 如果你是用動態加載場景，這個時間通常也足夠涵蓋初始生成。
        if (Time.timeSinceLevelLoad > 0.5f)
        {
            // 1. 播放放置音效
            if (data.placeSfx != null && AudioManager_2D.Instance != null)
                AudioManager_2D.Instance.PlayGameplaySFX(data.placeSfx);
        }
        // --- 修改結束 ---

        // 2. 初始化血量
        var hpComp = GetComponent<ObstacleHealth2D>();
        if (hpComp != null)
            hpComp.Initialize(data);

        if (data.lifetime > 0f)
        {
            Destroy(gameObject, data.lifetime);
        }
    }

    void OnDestroy()
    {
        // ¡¹ÃöÁä¡G¤£ºÞ¬O³Q¥´Ãz¡B®É¶¡¨ì¡B©Î¥ô¦ó¤è¦¡ Destroy¡A³£­n§â¦û®æÄÀ©ñ
        // ¦pªG¬O¨« PlacementSystem2D.Remove(core) ·|¥ý Unregister ¦A Destroy¡F
        // ³o¸Ì·|¦A¶]¤@¦¸¡A¦ý Unregister ¤º¦³¨¾§b¡]¥u²¾°£¦Û¤v¦ûªº®æ¤l¡^¡A©Ò¥H¦w¥þ¡C
        if (ownerSystem != null)
        {
            ownerSystem.Unregister(this);
        }
    }
}
