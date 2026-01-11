using UnityEngine;

public class EnemySlowable2D : MonoBehaviour, ISlowable2D
{
    [Header("Target AI Script")]
    public EnemyAI_ChasePlayer2D chase;   // 你的追擊腳本

    float baseSpeed;
    bool inited = false;

    void Awake()
    {
        if (chase == null) chase = GetComponent<EnemyAI_ChasePlayer2D>();
        Init();
    }

    void Init()
    {
        if (inited) return;
        if (chase == null) return;

        baseSpeed = chase.moveSpeed; //假設你的欄位叫 moveSpeed
        inited = true;
    }

    public void SetSlowMultiplier(float multiplier)
    {
        Init();
        if (chase == null) return;

        chase.moveSpeed = baseSpeed * Mathf.Clamp01(multiplier);
    }

    public void ClearSlowMultiplier()
    {
        Init();
        if (chase == null) return;

        chase.moveSpeed = baseSpeed;
    }
}
