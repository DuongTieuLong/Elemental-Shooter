using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }
    }

    public void ShakeCamera(float intensity)
    {
        if (impulseSource != null)
        {
            // Cinemachine 3.x GenerateImpulse method generates impulse using default settings with custom force multiplier
            impulseSource.GenerateImpulse(intensity);
        }
    }
}
