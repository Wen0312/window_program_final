using UnityEngine;

public class UIBGMTrigger_2D : MonoBehaviour
{
    [Header("UI BGM Type")]
    [SerializeField] private UIBGMType bgmType = UIBGMType.None;

    [Tooltip("Disable this UI BGM when object is disabled")]
    [SerializeField] private bool stopBGMOnDisable = true;

    private void OnEnable()
    {
        if (bgmType != UIBGMType.None)
        {
            AudioManager_2D.Instance?.PlayUIBGM(bgmType);
        }
    }

    private void OnDisable()
    {
        if (stopBGMOnDisable)
        {
            AudioManager_2D.Instance?.StopUIBGM();
        }
    }
}
