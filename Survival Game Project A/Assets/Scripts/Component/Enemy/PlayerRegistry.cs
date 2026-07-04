using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    // static property giúp mọi class truy cập được ngay lập tức ở bất cứ đâu
    public static Transform PlayerTransform { get; private set; }
    public static GameObject PlayerGameObject { get; private set; }

    private void OnEnable()
    {
        // Đăng ký sự kiện (Tùy thuộc vào cách bạn viết GameEvents, ví dụ dưới đây là dùng Action)
        GameEvents.OnPlayerSpawned += HandlePlayerSpawned;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh memory leak
        GameEvents.OnPlayerSpawned -= HandlePlayerSpawned;
    }

    private void HandlePlayerSpawned(Transform playerTransform)
    {
        PlayerTransform = playerTransform;
        PlayerGameObject = playerTransform.gameObject;
    }

    // Reset khi Scene bị hủy hoặc Player chết (nếu cần)
    private void OnDestroy()
    {
        PlayerTransform = null;
        PlayerGameObject = null;
    }
}