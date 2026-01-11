using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smooth = 10f;

    [Header("Camera (Optional)")]
    public Camera targetCamera; // 新增：允許 Camera 在子物件（CameraRig 結構）

    [Header("ADS Zoom (Orthographic Size)")]
    public float normalOrthoSize = 5f;  // 平常鏡頭大小
    public float adsOrthoSize = 7f;     // ADS 時鏡頭大小（orthographicSize 變大 = 拉遠）
    public float zoomSmooth = 10f;      // 縮放平滑速度

    [Header("Base View")]
    public float playBaseOrthoSize = 3.5f;
    public float editorBaseOrthoSize = 10f;

    Camera cam;
    bool isADS = false;

    // =========================
    // Zoom Punch（Runtime Offset）
    // =========================
    float zoomPunchOffset = 0f;         // 新增
    Coroutine zoomPunchCo;              // 新增

    // 若 > 0，ADS 時會優先用這個值（可由 PlayerInputMode2D 依武器丟進來）
    float adsOrthoSizeOverride = 0f;

    // =========================
    // Safety / Init Guard
    // =========================
    [Header("Safety Guard")] // 新增
    [Tooltip("target 太離譜時先不要追（避免初始化期間把相機拉飛）。0 = 關閉")] // 新增
    public float maxTargetDistanceFromCamera = 200f; // 新增：依你地圖尺度可調，例如 200~500

    [Tooltip("第一次抓到合理 target 後，直接把相機位置對齊一次，避免慢慢滑過去")] // 新增
    public bool snapOnFirstValidTarget = true; // 新增

    bool hasSnapped = false; // 新增

    void Awake()
    {
        // 新增：支援 Camera 在子物件（Rig 結構）
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
                targetCamera = GetComponentInChildren<Camera>();
        }

        cam = targetCamera;

        // 若相機是 Orthographic，且 normalOrthoSize 沒特別設，就用目前相機 size 當預設
        if (cam != null && cam.orthographic)
        {
            if (normalOrthoSize <= 0f)
                normalOrthoSize = cam.orthographicSize;
        }
    }

    // 由 PlayerInputMode2D 統一入口呼叫（Combat 才會設定；Build 忽略）
    public void SetADS_Public(bool ads)
    {
        isADS = ads;
    }

    // 由 PlayerInputMode2D 依武器資料設定（WeaponData.adsOrthoSizeOverride）
    // <= 0 代表不覆寫，回到 CameraFollow2D.adsOrthoSize
    public void SetADSOrthoSizeOverride_Public(float size)
    {
        adsOrthoSizeOverride = size;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ===== Safety Guard：避免初始化期間追到錯座標把相機拉飛 =====
        // 新增：只有在 maxTargetDistanceFromCamera > 0 才啟用
        if (maxTargetDistanceFromCamera > 0f)
        {
            Vector2 camXY = new Vector2(transform.position.x, transform.position.y);
            Vector2 tarXY = new Vector2(target.position.x, target.position.y);
            float dist = Vector2.Distance(camXY, tarXY);

            if (dist > maxTargetDistanceFromCamera)
            {
                // 目標太離譜：先不追，等 target 回到合理位置（通常是 MapBootstrapper 完成後）
                return;
            }

            // 第一次進入合理範圍，選擇「直接對齊一次」避免慢慢滑
            if (snapOnFirstValidTarget && !hasSnapped)
            {
                Vector3 p = transform.position;
                Vector3 tp = target.position;
                tp.z = p.z;
                transform.position = tp;
                hasSnapped = true;
            }
        }

        // ===== Follow（Rig 本體跟隨）=====
        Vector3 cur = transform.position;
        Vector3 tpos = target.position;
        tpos.z = cur.z; // 保持相機 Z 不變

        transform.position = Vector3.Lerp(cur, tpos, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        // ===== ADS Zoom (Orthographic only) =====
        if (cam != null && cam.orthographic)
        {
            float adsSize = (adsOrthoSizeOverride > 0f) ? adsOrthoSizeOverride : adsOrthoSize;
            float targetSize = isADS ? adsSize : normalOrthoSize;

            // Zoom Punch offset（不破壞 ADS / normal）
            targetSize += zoomPunchOffset;

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetSize,
                1f - Mathf.Exp(-zoomSmooth * Time.deltaTime)
            );
        }
    }

    // =========================
    // Public API
    // =========================
    public void ZoomPunch_Public(float deltaOrthoSize, float duration)
    {
        if (cam == null || !cam.orthographic) return;

        if (zoomPunchCo != null)
            StopCoroutine(zoomPunchCo);

        zoomPunchCo = StartCoroutine(ZoomPunchRoutine(deltaOrthoSize, Mathf.Max(0.05f, duration)));
    }

    System.Collections.IEnumerator ZoomPunchRoutine(float delta, float duration)
    {
        float inTime = duration * 0.25f;
        float outTime = duration - inTime;

        float t = 0f;
        while (t < inTime)
        {
            t += Time.unscaledDeltaTime;
            float k = (inTime <= 0.0001f) ? 1f : Mathf.Clamp01(t / inTime);
            zoomPunchOffset = Mathf.Lerp(0f, delta, k);
            yield return null;
        }

        t = 0f;
        while (t < outTime)
        {
            t += Time.unscaledDeltaTime;
            float k = (outTime <= 0.0001f) ? 1f : Mathf.Clamp01(t / outTime);
            float eased = 1f - Mathf.Pow(1f - k, 2f); // easeOut
            zoomPunchOffset = Mathf.Lerp(delta, 0f, eased);
            yield return null;
        }

        zoomPunchOffset = 0f;
        zoomPunchCo = null;
    }

    public void SetEditorProfile_Public(bool useEditor)
    {
        float target = useEditor ? editorBaseOrthoSize : playBaseOrthoSize;
        normalOrthoSize = target;
    }
}
