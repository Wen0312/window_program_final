using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// EndGamePoint2D
/// 
/// ¡i¥Î³~¡j
/// §@¬°¡uÃö¥d²×ÂI / ¹LÃöÂI¡vªºÄ²µo¾¹¡C
/// ª±®a¶i¤J«á¡AÄ²µoÃö¥dµ²§ô¬ÛÃö¨Æ¥ó¡]Win / Load Next / Åã¥Ü UI¡^¡C
///
/// ¡i³]­p­ì«h¡j
/// - ¥u­t³d¡u°»´úª±®a¶i¤J¡v
/// - ¤£³B²z UI¡B¤£¤Á³õ´º¡B¤£±±¨î TimeScale
/// - ¤£¨Ì¿à Status / Health ¥H¥~ªº¨t²Î
///
/// ¡i¬°¤°»ò¤£¥Î Tag §PÂ_ Player¡j
/// - Tag ®e©ö³Q»~¥Î©Î§ïÃa
/// - ¥»±M®×¥H PlayerHealth §@¬°¡uª±®a¥»Åé¡vªº³Ì¤p§PÂ_¨Ì¾Ú
///
/// ¡i¨å«¬±µªk¡j
/// - Inspector ¡÷ onReached
///   - ±µ Win UI
///   - ±µ Scene Loader
///   - ±µ GameFlow / GameManager
///
/// ¡iDebug µ¦²¤¡j
/// - ¶i¤J Trigger¡B³Q¾×¤U¡B¦¨¥\Ä²µo¡A³£·|¦L log
/// - ½T«O¡u¬Ý±o¥X¨Ó¥d¦b­þ¤@Ãö¡v
/// </summary>
public class EndGamePoint2D : MonoBehaviour
{
    [Header("Filter")]
    [Tooltip("只允許指定 Layer（通常只勾 Player）")]
    public LayerMask affectMask;          // Player layer

    [Header("Behavior")]
    [Tooltip("是否只允許觸發一次（避免來回走反覆觸發）")]
    public bool triggerOnce = true;

    [Header("SFX (optional)")]
    [Tooltip("玩家抵達終點時播放的音效（可留空）")]
    public AudioClip reachedSfx;

    [Header("Events")]
    [Tooltip("抵達終點時要做的事情（在 Inspector 接）")]
    public UnityEvent onReached;

    // ¬O§_¤w¸gÄ²µo¹L¡]·f°t triggerOnce¡^
    bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // ¶i¤J Trigger ªº²Ä¤@®É¶¡¡A¥ý¦L¥X¬O½Ö¶i¨Ó
        Debug.Log($"[EndGamePoint2D] Trigger enter by: {other.name}", this);

        // ¤wÄ²µo¹L¥B¥u¤¹³\¤@¦¸
        if (triggerOnce && triggered)
        {
            Debug.Log("[EndGamePoint2D] Already triggered, ignored.", this);
            return;
        }

        // Layer ¹LÂo¡]Á×§K¼Ä¤H / ¤l¼u / ¯S®Ä»~Ä²¡^
        if (((1 << other.gameObject.layer) & affectMask.value) == 0)
        {
            Debug.Log("[EndGamePoint2D] Layer not allowed, ignored.", this);
            return;
        }

        // §PÂ_¬O¤£¬O Player¡G
        // ¥H PlayerHealth §@¬°³Ì¤p¥i«H¨Ì¾Ú¡A¦Ó¤£¬O Tag
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.Log("[EndGamePoint2D] Not player (no PlayerHealth), ignored.", this);
            return;
        }

        Debug.Log("[EndGamePoint2D] Player detected, triggering end point.", this);
        Trigger();
    }

    /// <summary>
    /// ¹ê»ÚÄ²µo²×ÂI¦æ¬°
    /// - ³]©w triggered
    /// - ¼½©ñ­µ®Ä¡]­Y¦³¡^
    /// - ©I¥s onReached ¨Æ¥ó
    /// </summary>
    void Trigger()
    {
        if (triggerOnce) triggered = true;

        Debug.Log("[EndGamePoint2D] Trigger() Invoked.");

        // 1. 執行原本 Inspector 綁定的事件 (保留原本接口)
        onReached?.Invoke();

        // 2. --- 新增：通知 GameManager 判斷過關或是勝利 ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerLevelComplete();
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot trigger level complete logic.");
        }
    }
}
