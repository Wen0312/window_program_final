using UnityEngine;

public class LevelGoal2D : MonoBehaviour
{
    public bool isFinalVictory = false; // 是否為最後一關

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判斷是否為玩家
        if (other.CompareTag("Player"))
        {
            if (isFinalVictory)
            {
                VictoryUI vUI = FindFirstObjectByType<VictoryUI>();
                if (vUI != null) vUI.ShowVictory();
            }
            else
            {
                LevelClearUI cUI = FindFirstObjectByType<LevelClearUI>();
                if (cUI != null) cUI.ShowClear();
            }
        }
    }
}