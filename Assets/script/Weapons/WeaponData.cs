using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic")]
    public string weaponName = "Pistol";
    public Projectile2D projectilePrefab;
    public float projectileSpeed = 12f;
    public float fireCooldown = 0.15f;
    public float damage = 1f;

    [Header("Spread / Burst")]
    public int pellets = 1;          // ÄÅ¼u/¦hµo¡G¤@¦¸®g¥X´XÁû
    public float spreadAngle = 0f;   // ¦hµo®°§ÎÁ`¨¤«×¡]«×¡^

    // Step 2¡GÀH¾÷´²®g¡]¨CÁû¤l¼uÃB¥~¥[¤WÀH¾÷°¾²¾¡^
    // 0 = Ãö³¬ÀH¾÷´²®g¡]«O«ù¥Ø«e®°§Î±Æ¦C¡^
    [Min(0f)] public float randomSpreadAngle = 0f;    // ÀH¾÷´²®g³Ì¤j°¾²¾¨¤¡]«×¡^

    // Step 2¡GADS¡]ºË·Ç¡^®É´²®g­¿²v
    // ¨Ò¦p 0.5 ¥Nªí ADS ®É´²®g´î¥b¡F1 ¥Nªí¤£ÅÜ
    [Min(0f)] public float adsSpreadMultiplier = 0.5f;

    [Header("ADS Camera (Optional)")]
    [Tooltip("­Y > 0¡A«h ADS ®É¬Û¾÷ orthographicSize ¨Ï¥Î¦¹­ÈÂÐ¼g CameraFollow2D ªº adsOrthoSize¡C<= 0 ªí¥Ü¤£ÂÐ¼g¡C")]
    public float adsOrthoSizeOverride = 0f;

    // =========================
    // Weapon Visual (Optional)
    // =========================
    // ¥Øªº¡GªZ¾¹¹Ï¤ù/Prefab Åã¥Ü¦bª±®a¤â¤W¡]¸ê®Æ¤Æ¡^
    // - ªZ¾¹µøÄ±¤£À³¸Ó¥Ñ WeaponController2D ª½±µµw¼g¦º
    // - «áÄò¥Ñ WeaponVisual2D / Bridge ¥hÅª³o¨Ç¸ê®Æ¨Ã§ó·s SpriteRenderer
    [Header("Weapon Visual")]
    public Sprite weaponSprite;                       // ªZ¾¹¹Ï¤ù¡]¥i¬° null¡Anull=¤£Åã¥Ü¡^
    [Tooltip("Sprite ¥»¦a¦ì²¾¡]¬Û¹ï FirePoint¡^")]
    public Vector2 weaponSpriteLocalOffset = Vector2.zero; // ±¾ÂI¤Uªº¥»¦a°¾²¾¡]³æ¦ì¡GUnity world¡^
    [Tooltip("Sprite ¥»¦aÁY©ñ")]
    public Vector2 weaponSpriteLocalScale = Vector2.one;   // ±¾ÂI¤Uªº¥»¦aÁY©ñ
    [Tooltip("Sprite ¥»¦a±ÛÂà¨¤«×")]
    public float weaponSpriteAngleOffset = 0f;        // ¹ï»ô¥Î¡GÅý sprite ªº¡u´Â¥k¡v¹ï¤W AimDir¡]«×¡^
    public bool weaponFlipOnAimLeft = true;           // AimDir.x < 0 ®É¬O§_Â½Âà
    [Tooltip("Sprite ªº Sorting Order¡]«ØÄ³ > Player¡^")]
    public int weaponSortingOrder = 0;                // SpriteRenderer sortingOrder¡]Á×§KÀ£¦bª±®a¤U­±¡^

    // =========================
    // ADS Move (Optional)
    // =========================
    // ­Y > 0¡A«h ADS ®É²¾°Ê³t«×·|­¼¤W¦¹­¿²v¡A
    // ÂÐ¼g TopDownPlayerMove2D.adsSpeedMultiplier
    // ¨Ò¦p¡G
    // 0.8 = ´X¥G¤£´î³t¡]¤âºj¡^
    // 0.6 = ¨Bºj
    // 0.4 = ­«ªZ¾¹ / ª®À»
    // <= 0 ªí¥Ü¤£ÂÐ¼g¡A¨Ï¥Î¨¤¦â¹w³]­È
    [Header("ADS Move")]
    [Min(0f)] public float adsSpeedMultiplierOverride = 0f;

    [Header("Reload")]
    [Min(1)] public int magSize = 12;            // ¼u§X®e¶q
    [Min(0f)] public float reloadTime = 1.2f;    // ´«¼u¯Ó®É¡]¬í¡^
    public bool autoReloadOnEmpty = true;        // ªÅ¼u®É¦Û°Ê´«¼u

    [Header("Equip")]
    [Min(0f)] public float equipDelay = 0.1f;    // ´«ºj«áªº®gÀ»Âê©w®É¶¡¡]¬í¡^

    [Header("SFX")]
    public AudioClip shootSfx;                   // ®gÀ»­µ®Ä
    public AudioClip reloadSfx;                  // ´«¼u­µ®Ä
    public AudioClip equipSfx;
    public AudioClip adsSfx; // <--- 新增這一行：瞄準音效// ¤Áºj/¸Ë³Æ­µ®Ä

    [Header("Fire Mode")]
    public bool holdToFire = true;

    // =========================
    // Weapon Recoil (Visual)
    // =========================
    [Header("Recoil (Visual)")]

    [Tooltip("¨C¦¸¶}¤õªº«á®y¦ì²¾¶ZÂ÷¡]local units¡^")]
    public float recoilKickDistance = 0.08f;

    [Tooltip("«á®y¦ì²¾³Ì¤j²Ö¿n¶ZÂ÷")]
    public float recoilMaxDistance = 0.18f;

    [Tooltip("«á®y¦ì²¾¦^¥¿³t«×")]
    public float recoilReturnSpeed = 18f;

    [Tooltip("¨C¦¸¶}¤õªº±ÛÂà«á®y¡]«×¡^")]
    public float recoilKickRotationDeg = 2f;

    [Tooltip("±ÛÂà«á®y¦^¥¿³t«×")]
    public float recoilRotReturnSpeed = 25f;

    [Tooltip("±ÛÂà«á®y¬O§_ÀH¾÷¥ª¥k")]
    public bool recoilRandomRotSign = true;

    [Header("Recoil (ADS)")]
    [Tooltip("ADS ®É«á®y¤O­¿²v¡]1 = ¤£ÅÜ¡^")]
    [Min(0f)] public float adsRecoilMultiplier = 1f;

}
