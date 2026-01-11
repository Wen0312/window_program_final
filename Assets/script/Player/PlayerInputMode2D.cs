using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMode2D : MonoBehaviour
{
    public enum Mode { Combat, Build }

    [Header("Mode")]
    public Mode mode = Mode.Combat;

    [Header("Refs")]
    public WeaponController2D weaponController;   // 直接用現有 WeaponController2D
    public PlayerWeaponInput weaponInput;         // 用來切武器（沿用你的 weapons[]）
    public PlayerPlaceObstacle2D placer;          // 用來放置與切牆

    //新增：Move / Aim 也由 InputMode 統一餵值（子系統不再讀 Input）
    public TopDownPlayerMove2D mover;
    public PlayerAim2D aimer;

    [Header("Scroll Wheel")]
    public float wheelDeadZone = 0.05f;  // 避免很小的滾輪抖動
    public bool invertWheel = false;     // 需要反向就勾
    
    [Header("Map")]
    public MapBootstrapper2D mapBootstrapper;
    [Header("ADS camera")]
    public CameraFollow2D cameraFollow;

    [Header("Camera Base View (Play/Editor x Combat/Build)")]
    [Tooltip("Play Loadout + Combat 的基礎視野 (Orthographic Size)")]
    public float playCombatOrthoSize = 3.5f;

    [Tooltip("Editor Loadout + Combat 的基礎視野 (Orthographic Size)")]
    public float editorCombatOrthoSize = 10f;

    [Tooltip("Play Loadout + Build 的基礎視野 (Orthographic Size)")]
    public float playBuildOrthoSize = 7f;

    [Tooltip("Editor Loadout + Build 的基礎視野 (Orthographic Size)")]
    public float editorBuildOrthoSize = 9f;




    void Awake()
    {
        // 自動抓（省得你漏拖）
        if (weaponInput == null) weaponInput = GetComponent<PlayerWeaponInput>();
        if (weaponController == null && weaponInput != null) weaponController = weaponInput.weaponController;
        if (placer == null) placer = GetComponent<PlayerPlaceObstacle2D>();

        // Move / Aim 也自動抓
        if (mover == null) mover = GetComponent<TopDownPlayerMove2D>();
        if (aimer == null) aimer = GetComponent<PlayerAim2D>();

        if (mapBootstrapper == null)
            mapBootstrapper = UnityEngine.Object.FindFirstObjectByType<MapBootstrapper2D>();

        // CameraFollow：通常掛在 MainCamera
        if (cameraFollow == null)
            cameraFollow = UnityEngine.Object.FindFirstObjectByType<CameraFollow2D>();
    }


    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        // ======================
        // Unified Move / Aim Input
        // ======================

        // --- Move (WASD / 方向鍵) ---
        Vector2 move = Vector2.zero;

        // WASD
        if (Keyboard.current.wKey.isPressed) move.y += 1;
        if (Keyboard.current.sKey.isPressed) move.y -= 1;
        if (Keyboard.current.aKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        // 方向鍵（可選，保留你原本習慣）
        if (Keyboard.current.upArrowKey.isPressed) move.y += 1;
        if (Keyboard.current.downArrowKey.isPressed) move.y -= 1;
        if (Keyboard.current.leftArrowKey.isPressed) move.x -= 1;
        if (Keyboard.current.rightArrowKey.isPressed) move.x += 1;

        move = move.normalized;


        if (mover != null)
            mover.SetMoveInput(move);

        // --- Aim (Mouse world position) ---
        if (aimer != null)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouse.z = 0;
            aimer.SetAimWorldPosition(mouse);
        }

        // Alt：切換模式 + 同時開關預覽
        if (Keyboard.current.leftAltKey.wasPressedThisFrame || Keyboard.current.rightAltKey.wasPressedThisFrame)
        {
            ToggleMode();
        }

        if (mode == Mode.Combat) HandleCombat();
        else HandleBuild();

        // ===== Loadout Switch (Editor / Play) =====
        // Hotkey: O
        if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
        {
            if (mapBootstrapper != null && mapBootstrapper.map != null)
            {
                bool next = !mapBootstrapper.map.useEditorLoadouts;
                mapBootstrapper.ApplyLoadouts(next);

                Debug.Log("Loadout switched to: " + (next ? "EDITOR" : "PLAY"));
            }
            else
            {
                Debug.LogWarning("Loadout switch failed: mapBootstrapper or map is null.");
            }
        }


    }

    void ToggleMode()
    {
        if (mode == Mode.Combat)
        {
            // =========================
            // Combat → Build
            // =========================
            mode = Mode.Build;

            if (placer != null)
                placer.SetPreviewVisible_Public(true);

            // Build 不允許 ADS（只影響行為，不動相機）
            if (weaponController != null)
                weaponController.SetADS_Public(false);

            if (cameraFollow != null)
            {
                cameraFollow.SetADSOrthoSizeOverride_Public(0f);
                cameraFollow.SetADS_Public(false);
            }

            if (mover != null)
            {
                mover.SetADS_Public(false);
                mover.SetADSSpeedMultiplier_Public(0f);
            }
        }
        else
        {
            // =========================
            // Build → Combat
            // =========================
            mode = Mode.Combat;

            if (placer != null)
                placer.SetPreviewVisible_Public(false);

            // 回 Combat 只清 ADS 狀態，不動視野
            if (weaponController != null)
                weaponController.SetADS_Public(false);

            if (cameraFollow != null)
            {
                cameraFollow.SetADSOrthoSizeOverride_Public(0f);
                cameraFollow.SetADS_Public(false);
            }

            if (mover != null)
            {
                mover.SetADS_Public(false);
                mover.SetADSSpeedMultiplier_Public(0f);
            }
        }
    }







    // ======================
    // Combat（射擊模式）
    // 左鍵=射擊、滾輪=切武器、右鍵保留
    // ======================
    void HandleCombat()
    {
        if (weaponController == null) return;
        if (weaponController.currentWeapon == null) return;

        // ADS: 右鍵按住進入，放開退出（僅 Combat Mode）
        if (Mouse.current != null && cameraFollow != null)
        {
            bool ads = Mouse.current.rightButton.isPressed;

            // =========================
            // Weapon ADS（影響散射）
            // =========================
            weaponController.SetADS_Public(ads);

            // =========================
            // Move ADS（影響移動速度）
            // =========================
            if (mover != null)
            {
                mover.SetADS_Public(ads);

                if (ads)
                {
                    // ADS 時：若武器有設定移動倍率，則覆寫
                    mover.SetADSSpeedMultiplier_Public(
                        weaponController.currentWeapon.adsSpeedMultiplierOverride
                    );
                }
                else
                {
                    // 放開 ADS：清掉覆寫，回到角色預設值
                    mover.SetADSSpeedMultiplier_Public(0f);
                }
            }

            // =========================
            // Camera ADS（鏡頭拉遠）
            // =========================
            if (ads)
            {
                cameraFollow.SetADSOrthoSizeOverride_Public(
                    weaponController.currentWeapon.adsOrthoSizeOverride
                );
            }
            else
            {
                cameraFollow.SetADSOrthoSizeOverride_Public(0f);
            }

            cameraFollow.SetADS_Public(ads);
        }

        // Reload: R
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            weaponController.TryReload();
        }

        // 射擊（依武器設定決定是否連射）
        bool fireInput = weaponController.currentWeapon.holdToFire
            ? Mouse.current.leftButton.isPressed
            : Mouse.current.leftButton.wasPressedThisFrame;

        if (fireInput)
        {
            Vector2 dir = GetAimDir();
            weaponController.TryShoot(dir);
        }

        // 滾輪切換武器
        int w = ReadWheelDelta();
        if (w != 0) TrySwitchWeapon(w);
    }






    // ======================
    // Build（建造模式）
    // 左鍵=放置、滾輪=切牆、Q/E=旋轉、右鍵保留
    // ======================
    void HandleBuild()
    {
        if (placer == null) return;

        placer.UpdatePreview_Public();

        // ===== 旋轉（Q / E）=====
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            placer.Rotate_Public(-1); // 逆時針
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            placer.Rotate_Public(+1); // 順時針
        }

        // ===== 滾輪切障礙物 =====
        int w = ReadWheelDelta();
        if (w != 0)
        {
            CycleObstacle(w);
        }

        // ===== 右鍵拆除（只允許 Editor Loadout）=====
        // 只在 map.useEditorLoadouts == true 時才允許拆
        bool editorLoadout =
            mapBootstrapper != null &&
            mapBootstrapper.map != null &&
            mapBootstrapper.map.useEditorLoadouts;

        if (editorLoadout && Mouse.current.rightButton.wasPressedThisFrame)
        {
            placer.TryRemoveOnce_Public();
        }

        // ===== 左鍵放置 =====
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceOnce();
        }
    }


    // ---- helpers ----

    Vector2 GetAimDir()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse.z = 0;
        return (Vector2)(mouse - weaponController.firePoint.position);
    }

    void TrySwitchWeapon(int delta)
    {
        if (weaponInput == null) return;

        weaponInput.SwitchWeapon_Public(delta);
    }

    void HandleDigitSwitch()
    {
        // 最小可用：直接沿用你 PlayerPlaceObstacle2D 的 SetIndex
        if (Keyboard.current.digit1Key.wasPressedThisFrame) placer.SetIndex_Public(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) placer.SetIndex_Public(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) placer.SetIndex_Public(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) placer.SetIndex_Public(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) placer.SetIndex_Public(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) placer.SetIndex_Public(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) placer.SetIndex_Public(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) placer.SetIndex_Public(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) placer.SetIndex_Public(8);
    }

    void TryPlaceOnce()
    {
        // 讓 placer 自己算位置/格子
        placer.TryPlaceOnce_Public();
    }

    void TrySetPreview(bool on)
    {
        // 你的新版 placer 有 previewEnabled/SetPreviewActive；舊版沒有。
        // 這裡用 SendMessage 防止編譯失敗（有就呼叫，沒有就略過）。
        if (placer == null) return;

        placer.SendMessage("SetPreviewActive", on, SendMessageOptions.DontRequireReceiver);
    }

    int ReadWheelDelta()
    {
        if (Mouse.current == null) return 0;

        float y = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(y) < wheelDeadZone) return 0;

        int delta = y > 0 ? 1 : -1;
        if (invertWheel) delta = -delta;
        return delta;
    }

    void CycleObstacle(int delta)
    {
        if (placer == null) return;
        if (placer.obstacleList == null || placer.obstacleList.Length == 0) return;

        int len = placer.obstacleList.Length;
        int next = placer.currentIndex + delta;

        // wrap around
        if (next < 0) next = len - 1;
        if (next >= len) next = 0;

        placer.SetIndex_Public(next);

        // 讓預覽立刻更新（你已經有 UpdatePreview_Public）
        placer.UpdatePreview_Public();
    }
}
