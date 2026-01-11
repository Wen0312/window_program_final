using UnityEngine;

/// <summary>
/// SniperShotAbility2D
/// 狙擊手：不移動、偵測玩家、蓄力顯示紅線閃爍LineRenderer / VFX，蓄力完成後以 hitscan 造成傷害
///
/// 設計原則（遵守專案責任邊界）：
/// - 只負責「狙擊行為」，不控制關卡流程 / UI / Camera
/// - 不改 EnemyHealth / EndGamePoint2D
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/Abilities/SniperShotAbility2D", fileName = "SniperShotAbility2D")]
public class SniperShotAbility2D : SpecialEnemyAbilityData2D
{
    [Header("Line of Sight (Block Aim if Behind Wall)")]
    [Tooltip("用來判斷『牆後就不能開始蓄力』的 Raycast Layer。通常包含 Player + Wall (+Obstacle)")]
    public LayerMask lineOfSightMask = ~0;

    [Tooltip("若蓄力中視線被擋住，是否要中斷蓄力")]
    public bool cancelAimIfLineBlocked = true;

    [Header("Telegraph Progress FX")]
    [Tooltip("蓄力開始時的線寬")]
    public float startLineWidth = 0.02f;

    [Tooltip("即將發射時的線寬")]
    public float endLineWidth = 0.08f;

    [Tooltip("蓄力開始時的顏色（暗紅）")]
    public Color startLineColor = new Color(0.6f, 0f, 0f, 0.6f);

    [Tooltip("即將發射時的顏色（亮紅）")]
    public Color endLineColor = new Color(1f, 0f, 0f, 1f);

    [Tooltip("蓄力開始時的閃爍間隔（秒）")]
    public float startBlinkInterval = 0.15f;

    [Tooltip("即將發射時的閃爍間隔（秒，越小越快）")]
    public float endBlinkInterval = 0.03f;

    [Header("Detect")]
    public float detectRange = 12f;

    [Header("Aim")]
    [Tooltip("看到玩家後，蓄力多久才開槍")]
    public float aimDuration = 0.8f;

    [Tooltip("開完槍後冷卻多久才能再進入瞄準")]
    public float fireCooldown = 1.4f;

    [Header("Fire")]
    public float damage = 3f;

    [Tooltip("Raycast 的最大距離（通常 = detectRange）")]
    public float maxRayDistance = 12f;

    [Tooltip("射線碰撞遮擋層（包含 Player / Wall / Obstacle 等）")]
    public LayerMask rayMask = ~0;

    [Header("FirePoint")]
    [Tooltip("如果敵人物件底下有子物件 FirePoint，就填這個名字；找不到就用 transform.position")]
    public string firePointName = "FirePoint";

    [Tooltip("找不到 FirePoint 時的世界座標偏移")]
    public Vector2 fallbackWorldOffset;

    [Header("Telegraph (LineRenderer)")]
    public bool enableTelegraph = true;

    [Tooltip("紅線閃爍間隔（秒）。0 表示不閃爍")]
    public float blinkInterval = 0.08f;

    public float lineWidth = 0.04f;
    public Color lineColor = Color.red;

    [Tooltip("LineRenderer 的 sorting layer name（不填就自動跟隨敵人 SpriteRenderer）")]
    public string sortingLayerName;

    public int sortingOrder = 50;

    [Tooltip("LineRenderer 材質（URP/2D 常需要指定，否則可能完全不顯示）。不填則自動用 Sprites/Default")]
    public Material lineMaterial;

    [Header("Telegraph VFX (optional)")]
    [Tooltip("瞄準時跟著 FirePoint 的特效（可選）")]
    public GameObject aimVfxPrefab;

    [Tooltip("命中點特效（可選）")]
    public GameObject impactVfxPrefab;

    [Header("SFX")]
    [Tooltip("開始蓄力（開始瞄準）那一刻播放一次")]
    public AudioClip aimChargeSfx;

    [Tooltip("射擊那一刻播放一次")]
    public AudioClip fireSfx;

    public override SpecialEnemyAbilityRuntime2D CreateRuntime()
    {
        return new Runtime(this);
    }

    class Runtime : SpecialEnemyAbilityRuntime2D
    {
        readonly SniperShotAbility2D data;

        Transform owner;
        Transform firePoint;

        // telegraph
        GameObject lineGO;
        LineRenderer line;
        GameObject aimVfxGO;

        float nextFireTime;
        float aimTimer;
        bool isAiming;
        float blinkTimer;

        bool chargeSfxPlayedThisAim = false;

        // 用來自動對齊 sorting（避免線被背景蓋掉）
        SpriteRenderer ownerSprite;

        public Runtime(SniperShotAbility2D data)
        {
            this.data = data;
        }

        public override void Init(SpecialEnemyContext2D ctx)
        {
            owner = ctx.transform;

            // 取一個 SpriteRenderer 當 sorting 參考（如果有）
            ownerSprite = ctx.transform.GetComponentInChildren<SpriteRenderer>();

            // 找 FirePoint（找不到就用 transform）
            firePoint = ctx.transform.Find(data.firePointName);
            if (firePoint == null) firePoint = ctx.transform;

            if (data.enableTelegraph)
                BuildTelegraph(ctx);
        }

        void BuildTelegraph(SpecialEnemyContext2D ctx)
        {
            lineGO = new GameObject("SniperTelegraph_Line");
            lineGO.transform.SetParent(ctx.transform, false);

            line = lineGO.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;

            // 2D 推薦：讓線對齊 Z 軸平面
            line.alignment = LineAlignment.TransformZ;

            line.widthMultiplier = Mathf.Max(0.001f, data.lineWidth);
            line.startColor = data.lineColor;
            line.endColor = data.lineColor;
            line.enabled = false;

            // 材質：URP/2D 常見不顯示原因
            // 沒材質時，在 URP/2D Renderer 下可能完全不畫
            if (data.lineMaterial != null)
            {
                line.material = data.lineMaterial;
            }
            else
            {
                // 最保險：Sprites/Default
                Shader sh = Shader.Find("Sprites/Default");
                if (sh != null)
                    line.material = new Material(sh);
            }

            // Sorting：避免被 Tile/背景蓋掉
            if (!string.IsNullOrEmpty(data.sortingLayerName))
            {
                line.sortingLayerName = data.sortingLayerName;
                line.sortingOrder = data.sortingOrder;
            }
            else if (ownerSprite != null)
            {
                // 自動跟隨敵人的 sprite layer，並略高一點
                line.sortingLayerName = ownerSprite.sortingLayerName;
                line.sortingOrder = ownerSprite.sortingOrder + 1;
            }
            else
            {
                // 沒參考時才用你預設
                line.sortingOrder = data.sortingOrder;
            }

            // Aim VFX（可選）
            if (data.aimVfxPrefab != null)
            {
                aimVfxGO = Object.Instantiate(data.aimVfxPrefab, firePoint.position, Quaternion.identity, firePoint);
                aimVfxGO.SetActive(false);
            }
        }

        public override void Tick(SpecialEnemyContext2D ctx, float dt)
        {
            if (ctx.player == null) return;

            if (Time.time < nextFireTime)
            {
                StopTelegraph();
                isAiming = false;
                aimTimer = 0;
                chargeSfxPlayedThisAim = false;
                return;
            }

            float dist = Vector2.Distance(ctx.transform.position, ctx.player.position);
            if (dist > data.detectRange)
            {
                StopTelegraph();
                isAiming = false;
                aimTimer = 0;
                chargeSfxPlayedThisAim = false;
                return;
            }

            // 玩家在牆後：不開始蓄力
            if (!isAiming)
            {
                if (!HasLineOfSightToPlayer(ctx))
                {
                    StopTelegraph();
                    aimTimer = 0;
                    chargeSfxPlayedThisAim = false;
                    return;
                }

                // 進入瞄準
                isAiming = true;
                aimTimer = 0;
                blinkTimer = 0;
                StartTelegraph();
            }
            else
            {
                // 已在蓄力中：如果視線被擋住就中斷（可選）
                if (data.cancelAimIfLineBlocked && !HasLineOfSightToPlayer(ctx))
                {
                    StopTelegraph();
                    isAiming = false;
                    aimTimer = 0;
                    chargeSfxPlayedThisAim = false;
                    return;
                }
            }

            aimTimer += dt;

            // telegraph 更新（包含閃爍 / line endpoint）
            UpdateTelegraph(ctx, dt);

            if (aimTimer >= data.aimDuration)
            {
                Fire(ctx);
                nextFireTime = Time.time + data.fireCooldown;

                isAiming = false;
                aimTimer = 0;
                StopTelegraph();
                chargeSfxPlayedThisAim = false;
            }
        }

        void StartTelegraph()
        {
            if (line != null) line.enabled = true;
            if (aimVfxGO != null) aimVfxGO.SetActive(true);

            // 蓄力音效：每次開始瞄準只播一次
            if (!chargeSfxPlayedThisAim && data.aimChargeSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.aimChargeSfx);
                chargeSfxPlayedThisAim = true;
            }
        }

        void StopTelegraph()
        {
            if (line != null) line.enabled = false;
            if (aimVfxGO != null) aimVfxGO.SetActive(false);
        }

        void UpdateTelegraph(SpecialEnemyContext2D ctx, float dt)
        {
            if (line == null) return;

            // 蓄力進度（0 到 1）
            float t = Mathf.Clamp01(aimTimer / Mathf.Max(0.0001f, data.aimDuration));

            // 視覺插值
            float width = Mathf.Lerp(data.startLineWidth, data.endLineWidth, t);
            Color color = Color.Lerp(data.startLineColor, data.endLineColor, t);
            float blinkInterval = Mathf.Lerp(data.startBlinkInterval, data.endBlinkInterval, t);

            line.widthMultiplier = Mathf.Max(0.001f, width);
            line.startColor = color;
            line.endColor = color;

            // 閃爍（越接近發射，閃越快）
            if (blinkInterval > 0f)
            {
                blinkTimer += dt;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    line.enabled = !line.enabled;
                }
            }
            else
            {
                line.enabled = true;
            }

            // 線段方向
            Vector2 origin = GetFirePointPosition();
            Vector2 dir = ((Vector2)ctx.player.position - origin).normalized;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, data.maxRayDistance, data.rayMask);
            Vector2 end = hit.collider != null ? hit.point : origin + dir * data.maxRayDistance;

            line.SetPosition(0, origin);
            line.SetPosition(1, end);

            if (aimVfxGO != null)
                aimVfxGO.transform.position = origin;
        }

        Vector2 GetFirePointPosition()
        {
            if (firePoint != null) return (Vector2)firePoint.position;
            if (owner != null) return (Vector2)owner.position + data.fallbackWorldOffset;
            return data.fallbackWorldOffset;
        }

        void Fire(SpecialEnemyContext2D ctx)
        {
            // 射擊音效：射擊那一刻播放一次
            if (data.fireSfx != null)
            {
                AudioManager_2D.Instance.PlayGameplaySFX(data.fireSfx);
            }

            Vector2 origin = GetFirePointPosition();
            Vector2 dir = ((Vector2)ctx.player.position - origin).normalized;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, data.maxRayDistance, data.rayMask);
            if (hit.collider == null) return;

            Transform root = hit.collider.transform.root;
            if (!root.CompareTag("Player")) return;

            var dmgable = hit.collider.GetComponentInParent<IDamageable>();
            if (dmgable != null)
            {
                dmgable.TakeDamage(data.damage, hit.point, ctx.gameObject);
            }

            if (data.impactVfxPrefab != null)
            {
                Object.Instantiate(data.impactVfxPrefab, hit.point, Quaternion.identity);
            }
        }

        public override void OnDeath(SpecialEnemyContext2D ctx)
        {
            StopTelegraph();
            chargeSfxPlayedThisAim = false;

            if (aimVfxGO != null)
            {
                Object.Destroy(aimVfxGO);
                aimVfxGO = null;
            }

            if (lineGO != null)
            {
                Object.Destroy(lineGO);
                lineGO = null;
                line = null;
            }
        }

        bool HasLineOfSightToPlayer(SpecialEnemyContext2D ctx)
        {
            if (ctx.player == null) return false;

            Vector2 origin = GetFirePointPosition();
            Vector2 toPlayer = (Vector2)ctx.player.position - origin;

            // 太近避免零向量
            if (toPlayer.sqrMagnitude < 0.0001f) return true;

            float dist = toPlayer.magnitude;
            Vector2 dir = toPlayer / dist;

            // 只要第一個打到的不是 Player，就視為被牆/障礙擋住
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, data.lineOfSightMask);
            if (hit.collider == null) return false;

            // 允許打到 Player 本體或 Player 子物件
            Transform root = hit.collider.transform.root;
            return root.CompareTag("Player");
        }
    }
}
