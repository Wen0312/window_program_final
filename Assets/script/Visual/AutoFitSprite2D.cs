using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class AutoFitSprite2D : MonoBehaviour
{
    // 新增 Stretch：會變形、硬撐滿 targetSize
    public enum FitMode { KeepHeight, KeepWidth, FitInside, FitCover, Stretch }

    [Header("Target size in WORLD units")]
    public Vector2 targetSize = new Vector2(1f, 1f);

    [Header("How to fit the sprite into targetSize")]
    public FitMode fitMode = FitMode.KeepHeight;

    [Header("If true, use sprite bounds (world) and keep localScale clean")]
    public bool applyOnValidate = true;

    SpriteRenderer sr;
    Sprite lastSprite;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        Apply();
    }

    void OnValidate()
    {
        if (!applyOnValidate) return;
        sr = GetComponent<SpriteRenderer>();
        Apply();
    }

    void Update()
    {
#if UNITY_EDITOR
        // 在編輯器中你換 sprite 時也會即時套用
        if (sr != null && sr.sprite != lastSprite) Apply();
#endif
    }

    public void Apply()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) return;

        lastSprite = sr.sprite;

        // sprite 的「世界尺寸」(bounds.size) 會受 PPU 影響，但我們就是要把它 fit 到 targetSize
        Vector2 spriteWorldSize = sr.sprite.bounds.size;
        if (spriteWorldSize.x <= 0 || spriteWorldSize.y <= 0) return;

        float sx = targetSize.x / spriteWorldSize.x;
        float sy = targetSize.y / spriteWorldSize.y;

        switch (fitMode)
        {
            case FitMode.Stretch:
                // ★ 會變形：寬高各自縮放，硬撐滿 targetSize
                transform.localScale = new Vector3(sx, sy, 1f);
                break;

            case FitMode.KeepHeight:
                // 等比：以高度為準
                transform.localScale = new Vector3(sy, sy, 1f);
                break;

            case FitMode.KeepWidth:
                // 等比：以寬度為準
                transform.localScale = new Vector3(sx, sx, 1f);
                break;

            case FitMode.FitInside:
                // 等比：完整塞進去，不裁切（可能留空白）
                float sIn = Mathf.Min(sx, sy);
                transform.localScale = new Vector3(sIn, sIn, 1f);
                break;

            case FitMode.FitCover:
                // 等比：覆蓋滿（可能裁切）
                float sCov = Mathf.Max(sx, sy);
                transform.localScale = new Vector3(sCov, sCov, 1f);
                break;

            default:
                // 預設：以高度為準
                transform.localScale = new Vector3(sy, sy, 1f);
                break;
        }
    }
}
