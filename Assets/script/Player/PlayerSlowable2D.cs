using UnityEngine;

public class PlayerSlowable2D : MonoBehaviour, ISlowable2D
{
    [Header("Target Move Script")]
    public TopDownPlayerMove2D move;   // 你現有移動腳本
    float baseSpeed;
    bool inited = false;

    void Awake()
    {
        if (move == null) move = GetComponent<TopDownPlayerMove2D>();
        Init();
    }

    void Init()
    {
        if (inited || move == null) return;
        baseSpeed = move.moveSpeed;    // 你的腳本欄位就是 moveSpeed（圖上看得到）
        inited = true;
    }

    public void SetSlowMultiplier(float multiplier)
    {
        Init();
        if (move == null) return;
        move.moveSpeed = baseSpeed * Mathf.Clamp01(multiplier);
    }

    public void ClearSlowMultiplier()
    {
        Init();
        if (move == null) return;
        move.moveSpeed = baseSpeed;
    }
}
