using UnityEngine;
using TMPro;

/// <summary>
/// WorldPromptUI2D
/// 最小可用的「顯示一行文字」UI（TMP 版本）
/// - 只提供 Show / Hide
/// - 不綁定任何互動或動畫
/// </summary>
public class WorldPromptUI2D : MonoBehaviour
{
    [Header("Refs")]
    public Canvas canvas;
    public TMP_Text textUI;

    void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        if (textUI == null)
            textUI = GetComponentInChildren<TMP_Text>(true);

        Hide();
    }

    public void Show(string msg)
    {
        if (canvas != null) canvas.enabled = true;
        if (textUI != null) textUI.text = msg;
    }

    public void Hide()
    {
        if (canvas != null) canvas.enabled = false;
    }
}
