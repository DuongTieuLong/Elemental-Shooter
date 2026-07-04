using UnityEngine;

public class ParticlePoolReturn : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private GameObject basePrefabs;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void SetBasePrefabs(GameObject basePrefabs)
    {
        this.basePrefabs = basePrefabs;
    }

    // Unity sẽ tự động gọi hàm này khi Particle System chạy hết Duration và dừng lại
    private void OnParticleSystemStopped()
    {
        // Trả hiệu ứng này về PoolManager của bạn
        if (PoolManager.Instance != null)
        {
            // Truyền chính GameObject này về lại Pool
            PoolManager.Instance.ReturnToPool(basePrefabs,gameObject);
        }
        else
        {
            // Phương án phòng hờ nếu không dùng Pool cho VFX
            Destroy(gameObject);
        }
    }
}