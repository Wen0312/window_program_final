using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject victoryPanel;

    [Header("Audio")]
    public AudioClip victorySFX;

    void Awake()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    // --- 預留給地塊或 Boss 死亡觸發的接口 ---
    public void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (victorySFX != null && AudioManager_2D.Instance != null)
        {
            AudioManager_2D.Instance.PlayGameplaySFX(victorySFX);
        }

        Time.timeScale = 0f;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay2D");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}