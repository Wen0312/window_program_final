using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectLevelUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject selectLevelPanel;

    [Header("Level Buttons (index = level-1)")]
    public Button[] levelButtons;

    void Awake()
    {
        // 一開始隱藏 Panel
        if (selectLevelPanel != null)
            selectLevelPanel.SetActive(false);
    }

    void OnEnable()
    {
        Refresh();
    }

    // === 對外接口 ===
    public void Show()
    {
        if (selectLevelPanel != null)
            selectLevelPanel.SetActive(true);
    }

    public void Hide()
    {
        if (selectLevelPanel != null)
            selectLevelPanel.SetActive(false);
    }

    // === 刷新關卡解鎖狀態 ===
    void Refresh()
    {
        int maxUnlocked = GameManager.MaxUnlockedMapIndex;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool unlocked = i <= maxUnlocked;
            levelButtons[i].interactable = unlocked;
        }
    }

    // === 給 Level Button 呼叫 ===
    public void OnSelectLevel(int levelIndex)
    {
        // levelIndex = 0-based
        GameManager.Instance.LoadMapByIndex(levelIndex);
        Hide();
    }

    // === Back Button ===
    public void OnBack()
    {
        Hide();
    }
    void LoadLevel(int index)
    {
        GameManager.CurrentMapIndex = index;
        GameManager.IsLoadingNextLevel = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
