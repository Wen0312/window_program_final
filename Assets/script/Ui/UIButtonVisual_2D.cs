using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonVisual_2D : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Color")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.gray;

    [Header("Scale")]
    [SerializeField] private float clickScaleMultiplier = 0.9f;
    [SerializeField] private float scaleSpeed = 15f;

    private Image buttonImage;
    private Vector3 normalScale;
    private Vector3 targetScale;

    private bool isHovering;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        normalScale = transform.localScale;
        targetScale = normalScale;

        if (buttonImage != null)
            buttonImage.color = normalColor;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    // 滑鼠移到 Button 上
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = hoverColor;

        AudioManager_2D.Instance?.PlayUIHover();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = normalScale * clickScaleMultiplier;

        AudioManager_2D.Instance?.PlayUIClick();
    }


    // 滑鼠移出 Button
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (buttonImage != null)
            buttonImage.color = normalColor;

        targetScale = normalScale;
    }

    // 滑鼠放開
    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = normalScale;
    }
}