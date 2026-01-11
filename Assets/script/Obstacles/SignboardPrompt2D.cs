using UnityEngine;
using UnityEngine.UI; // 為了控制 Image

public class SignboardPrompt2D : MonoBehaviour
{
    [Header("Prompt Content")]
    [TextArea] public string message = "Press R to reload.";

    [Header("★ 圖片設定")]
    [Tooltip("請選一張圖片 (Sprite)")]
    public Sprite iconSprite;

    [Tooltip("請把 Canvas 下的 Image 物件拖進來")]
    public Image targetImageDisplay;

    [Header("Settings")]
    public string playerTag = "Player";
    public WorldPromptUI2D promptUI; // 原本的文字系統
    public float triggerRadius = 2.5f;
    public float checkInterval = 0.1f;
    public bool oneShot = false;
    public bool destroyAfterShown = false;

    // 內部變數
    Transform playerTf;
    float nextCheckTime = 0f;
    bool isShowing = false;
    bool hasShownOnce = false;

    void Awake()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null) playerTf = player.transform;

        if (promptUI == null) promptUI = GetComponentInChildren<WorldPromptUI2D>(true);

        // 初始化：先隱藏圖片
        if (targetImageDisplay != null)
        {
            targetImageDisplay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (GameRuntimeFlags2D.developerProfile) return;
        if (oneShot && hasShownOnce) return;
        if (checkInterval > 0f && Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        if (playerTf == null)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) playerTf = player.transform;
            if (playerTf == null) return;
        }

        float distSqr = (playerTf.position - transform.position).sqrMagnitude;
        bool inside = distSqr <= triggerRadius * triggerRadius;

        if (inside)
        {
            if (!isShowing)
            {
                // 1. 顯示文字 (原本的功能)
                if (promptUI != null) promptUI.Show(message);

                // 2. ★ 顯示圖片 (強制開啟)
                if (targetImageDisplay != null)
                {
                    // 如果有指定圖片，就換圖；沒指定就用原本 Image 上的圖
                    if (iconSprite != null) targetImageDisplay.sprite = iconSprite;

                    // ★ 強制修正：確保不是透明的
                    var c = targetImageDisplay.color;
                    targetImageDisplay.color = new Color(c.r, c.g, c.b, 1f);

                    // ★ 強制修正：確保大小不是 0
                    if (targetImageDisplay.transform.localScale == Vector3.zero)
                        targetImageDisplay.transform.localScale = Vector3.one;

                    targetImageDisplay.gameObject.SetActive(true);
                }

                isShowing = true;

                if (oneShot)
                {
                    hasShownOnce = true;
                    if (destroyAfterShown) Destroy(gameObject);
                }
            }
        }
        else
        {
            if (isShowing)
            {
                // 隱藏文字
                if (promptUI != null) promptUI.Hide();
                // 隱藏圖片
                if (targetImageDisplay != null) targetImageDisplay.gameObject.SetActive(false);

                isShowing = false;
            }
        }
    }

    void OnDestroy()
    {
        if (isShowing)
        {
            if (promptUI != null) promptUI.Hide();
            if (targetImageDisplay != null) targetImageDisplay.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}