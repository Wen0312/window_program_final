using UnityEngine;

/// <summary>
/// ForceSorting2D
/// 用於保底：不論從哪個流程生成，Renderer 都會維持指定的 Sorting Layer / Order
/// </summary>
public class ForceSorting2D : MonoBehaviour
{
    [Header("Target")]
    public SpriteRenderer target; // 新增

    [Header("Sorting")]
    public string sortingLayerName = "FX"; // 新增
    public int sortingOrder = 50;          // 新增

    void Awake()
    {
        // 新增
        if (target == null) target = GetComponent<SpriteRenderer>();
        Apply();
    }

    void OnEnable()
    {
        // 新增：有些物件會 Disable/Enable，確保重啟也正確
        Apply();
    }

    // 新增
    void Apply()
    {
        if (target == null) return;
        if (!string.IsNullOrEmpty(sortingLayerName))
            target.sortingLayerName = sortingLayerName;

        target.sortingOrder = sortingOrder;
    }
}
