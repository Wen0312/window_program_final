using UnityEngine;
using System.Diagnostics; // 新增

public class CameraShake2D : MonoBehaviour
{
    public static CameraShake2D Instance { get; private set; }

    [Header("Debug / Lock")] // 新增
    public bool lockShake = false; // 新增
    public bool debugLogWhenLocked = true; // 新增

    // ====== Runtime ====== // 新增
    Vector3 originalLocalPos;        // 新增
    Coroutine shakeRoutine;          // 新增

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        lockShake = false; // 新增：保險起見，避免 Inspector/Prefab 不小心鎖到

        originalLocalPos = transform.localPosition; // 新增
    }

    public void SetShakeLocked(bool locked) // 新增
    {
        lockShake = locked;
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        if (lockShake)
        {
            if (debugLogWhenLocked)
            {
                var st = new StackTrace(1, true);
                UnityEngine.Debug.LogWarning("[CameraShake2D] Shake() blocked because lockShake=true\nCaller:\n" + st);
            }
            return;
        }

        // ====== 以下是你原本 Shake 的內容 ======
        // 新增：如果正在抖，先停掉，避免疊加把相機越抖越偏
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        // 新增：每次開始前，重新抓一次原點（避免你在 Editor 調過 localPos）
        originalLocalPos = transform.localPosition;

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, amplitude, frequency)); // 新增
    }

    System.Collections.IEnumerator ShakeRoutine(float duration, float amplitude, float frequency) // 新增
    {
        float t = 0f;

        // 新增：避免 frequency <= 0 造成沒動
        if (frequency <= 0f) frequency = 1f;

        while (t < duration)
        {
            // 用 PerlinNoise 做比較「平滑」的抖動（不會像 Random 那麼刺）
            float nx = Mathf.PerlinNoise(Time.time * frequency, 0.123f) * 2f - 1f;
            float ny = Mathf.PerlinNoise(0.456f, Time.time * frequency) * 2f - 1f;

            transform.localPosition = originalLocalPos + new Vector3(nx, ny, 0f) * amplitude;

            t += Time.deltaTime;
            yield return null;
        }

        // 結束要回原位
        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }

    public void StopAll()
    {
        StopAllCoroutines();

        // 新增：停掉後立刻回原位，避免停在偏移狀態
        transform.localPosition = originalLocalPos;

        shakeRoutine = null; // 新增
        // 你原本註解：你原本的 StopAll 內容是空的
    }
}
