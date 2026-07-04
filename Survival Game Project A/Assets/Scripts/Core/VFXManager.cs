using UnityEngine;
using System.Collections;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Jackpot Visuals")]
    public GameObject lightPillarPrefab; // Gắn prefab cột sáng vào đây
    public AudioClip jackpotSound;       // Gắn sound hiệu ứng vào đây
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameEvents.OnJackpotTriggered += HandleJackpotTriggered;
        GameEvents.OnHitStopRequested += TriggerHitStop;
        GameEvents.OnCameraShakeRequested += TriggerCameraShake;
    }

    private void OnDisable()
    {
        GameEvents.OnJackpotTriggered -= HandleJackpotTriggered;
        GameEvents.OnHitStopRequested -= TriggerHitStop;
        GameEvents.OnCameraShakeRequested -= TriggerCameraShake;
    }

    private void HandleJackpotTriggered(Vector3 position, ScriptableObject data)
    {
        // 1. Cột sáng chói lóa
        if (lightPillarPrefab != null)
        {
            Instantiate(lightPillarPrefab, position, Quaternion.identity);
            // Lưu ý: Có thể dùng PoolManager nếu LightPillar rớt quá nhiều
        }

        // 2. Rung màn hình (Giả lập gọi sự kiện rung)
        GameEvents.OnCameraShakeRequested?.Invoke(5f, 0.5f);

        // 3. Âm thanh réo rắt
        if (jackpotSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jackpotSound);
        }

        // 4. Hit Stop (Dừng đọng thời gian 0.5 giây)
        GameEvents.OnHitStopRequested?.Invoke(0.5f);
    }

    private void TriggerHitStop(float durationRealtime)
    {
        StartCoroutine(HitStopCoroutine(durationRealtime));
    }

    private IEnumerator HitStopCoroutine(float durationRealtime)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(durationRealtime);
        Time.timeScale = 1f;
    }

    private void TriggerCameraShake(float intensity, float duration)
    {
        // Ở đây tích hợp với Cinemachine Impulse hoặc logic rung camera của dự án.
        // CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
        // impulseSource.GenerateImpulse(intensity);
        Debug.Log($"[VFXManager] Camera Shaking with intensity {intensity} for {duration}s");
    }
}
