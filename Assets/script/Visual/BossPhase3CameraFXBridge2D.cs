using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// BossPhase3CameraFXBridge2D
/// Phase 3 進入時的相機效果（連續震動 + 進入瞬間 Zoom Punch）
///
/// 設計原則：
/// - HUD 不操作 Camera
/// - Enemy/Boss 不直接操作 Camera Transform
/// - 只透過 Visual 系統（CameraShake2D / CameraFollow2D 的 Public API）觸發
/// - 這個腳本只做「訂閱狀態」與「觸發表現」，不改 Boss / UI 邏輯
/// </summary>
public class BossPhase3CameraFXBridge2D : MonoBehaviour
{
    [Header("Refs (Optional)")]
    public BossPhaseState2D phaseState;          // 可手動指定
    public CameraFollow2D cameraFollow;          // 可手動指定（Zoom Punch 用）

    [Header("Auto Find")]
    public bool autoFind = true;
    public float autoFindInterval = 0.25f;

    [Header("Phase 3 Keywords")]
    public string explicitPhase3NameA = "Phase 3";
    public string explicitPhase3NameB = "Phase3";
    public string explicitPhase3NameC = "Berserker Mode";

    [Header("Phase 3 Continuous Screen Shake")]
    public bool continuousShakeEnabled = true;
    public float shakeDuration = 0.25f;
    public float shakeAmplitude = 0.18f;
    public float shakeFrequency = 35f;
    public float shakeLoopInterval = 0.22f; // 建議略小於 duration，讓效果連續

    [Header("Phase 3 Enter Zoom Punch")]
    public bool zoomPunchEnabled = true;
    [Tooltip("正值=拉遠(orthographicSize變大)；負值=拉近")]
    public float zoomPunchDeltaOrthoSize = -0.25f;
    public float zoomPunchDuration = 0.18f;

    float nextFindTime;
    bool wasPhase3;
    Coroutine shakeLoopCo;

    void OnEnable()
    {
        TryBind();
    }

    void Update()
    {
        if (!autoFind) return;
        if (phaseState != null && cameraFollow != null) return;

        if (Time.time >= nextFindTime)
        {
            nextFindTime = Time.time + Mathf.Max(0.05f, autoFindInterval);
            TryBind();
        }
    }

    void OnDisable()
    {
        Unbind();
    }

    void TryBind()
    {
        if (phaseState == null)
            phaseState = FindFirstObjectByType<BossPhaseState2D>();

        if (cameraFollow == null)
        {
            // 先抓主相機，再退而求其次用 Find
            if (Camera.main != null)
                cameraFollow = Camera.main.GetComponent<CameraFollow2D>();
            if (cameraFollow == null)
                cameraFollow = FindFirstObjectByType<CameraFollow2D>();
        }

        if (phaseState == null) return;

        // 防止重複訂閱
        phaseState.OnPhaseChanged -= HandlePhaseChanged;
        phaseState.OnPhaseChanged += HandlePhaseChanged;

        // 同步一次現況
        wasPhase3 = IsPhase3(phaseState.currentPhase);
        if (wasPhase3)
            StartContinuousShake();
    }

    void Unbind()
    {
        if (phaseState != null)
            phaseState.OnPhaseChanged -= HandlePhaseChanged;

        StopContinuousShake();
        phaseState = null;
        cameraFollow = null;
        wasPhase3 = false;
    }

    void HandlePhaseChanged(string newPhase)
    {
        bool isPhase3 = IsPhase3(newPhase);

        // 新增：先更新狀態，避免協程 while(wasPhase3) 一開始就跳出
        if (isPhase3 && !wasPhase3)
        {
            wasPhase3 = true; // 新增：先設為 true

            // 進入 Phase 3
            StartContinuousShake();
            TriggerZoomPunch();
            return; // 新增：避免下面再覆寫
        }

        if (!isPhase3 && wasPhase3)
        {
            wasPhase3 = false; // 新增：先設為 false

            // 離開 Phase 3
            StopContinuousShake();
            return; // 新增：避免下面再覆寫
        }

        // 其他狀態同步
        wasPhase3 = isPhase3;
    }


    bool IsPhase3(string phaseRaw)
    {
        string phase = (phaseRaw ?? string.Empty).Trim();

        if (string.Equals(phase, explicitPhase3NameA, StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(phase, explicitPhase3NameB, StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(phase, explicitPhase3NameC, StringComparison.OrdinalIgnoreCase)) return true;

        if (phase.IndexOf("berserk", StringComparison.OrdinalIgnoreCase) >= 0) return true;

        if (phase.IndexOf("phase", StringComparison.OrdinalIgnoreCase) >= 0 &&
            phase.IndexOf("3", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        return false;
    }

    void StartContinuousShake()
    {
        if (!continuousShakeEnabled) return;
        if (CameraShake2D.Instance == null) return;
        if (shakeLoopCo != null) return;

        shakeLoopCo = StartCoroutine(ContinuousShakeLoop());
    }

    void StopContinuousShake()
    {
        if (shakeLoopCo != null)
        {
            StopCoroutine(shakeLoopCo);
            shakeLoopCo = null;
        }

        // 新增：離開 Phase 3 時，確保相機回到原位
        if (CameraShake2D.Instance != null)
            CameraShake2D.Instance.StopAll();
    }

    IEnumerator ContinuousShakeLoop()
    {
        while (wasPhase3)
        {
            if (CameraShake2D.Instance != null)
            {
                CameraShake2D.Instance.Shake(
                    Mathf.Max(0.02f, shakeDuration),
                    Mathf.Max(0f, shakeAmplitude),
                    Mathf.Max(0f, shakeFrequency)
                );
            }

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.02f, shakeLoopInterval)
            );
        }

        shakeLoopCo = null;
    }

    void TriggerZoomPunch()
    {
        if (!zoomPunchEnabled) return;
        if (cameraFollow == null) return;

        cameraFollow.ZoomPunch_Public(
            zoomPunchDeltaOrthoSize,
            zoomPunchDuration
        );
    }
    // 新增
    public void StopAllPhase3FX_Public()
    {
        StopContinuousShake();
    }

    // 新增
    public void StopContinuousShake_Public()
    {
        StopContinuousShake();
    }

}
