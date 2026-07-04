using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    [SerializeField] private bool autoAim = false;
    [SerializeField] private float viewRange = 10f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Rotation Settings")]
    [SerializeField] private Transform player;      // Gán Transform của Player vào đây
    [SerializeField] private float orbitDistance; // Khoảng cách từ súng đến tâm Player

    public Transform GunPos;

    // Biến phục vụ cơ chế giật lùi
    private float _currentRecoilDistance = 0f;
    private float _recoilRecoverySpeed = 10f;
    private PlayerArmAimController _armController;

    public float CurrentRecoilDistance => _currentRecoilDistance;

    void Awake()
    {
        _armController = GetComponent<PlayerArmAimController>();
    }

    void Update()
    {
        if (Time.timeScale == 0) return; // Block weapon rotation updates when paused
        HandleWeaponRotation();
        RestoreRecoil(); // Hồi phục độ giật theo thời gian
    }

    private void HandleWeaponRotation()
    {
        if (player == null) return;

        // Nếu có PlayerArmAimController đang hoạt động, để nó tự quản lý vị trí súng ở tay
        if (_armController != null && _armController.enabled)
        {
            return;
        }

        Vector2 aimDirection = GetAimDirection();

        // 1. Tính toán vị trí mới của vũ khí (Orbit quanh Player)
        // TÍNH TOÁN VỊ TRÍ GỐC (Orbit quanh Player)
        Vector2 targetPosition = (Vector2)player.position + (aimDirection * orbitDistance);

        // ÁP DỤNG ĐỘ GIẬT: Trừ đi một khoảng dọc theo hướng nhắm (đẩy súng về sau)
        GunPos.position = targetPosition - (aimDirection * _currentRecoilDistance);

        // 2. Xoay vũ khí để nó luôn hướng mặt về phía mục tiêu
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        GunPos.rotation = Quaternion.Euler(0, 0, angle);

        // 3. (Tùy chọn) Lật súng (Flip) nếu súng chổng ngược khi hướng sang trái
        if (aimDirection.x < 0)
            GunPos.localScale = new Vector3(0.5f,-0.5f,0.5f);
        else
            GunPos.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    // Hàm gọi từ class Weapon khi súng nổ
    public void TriggerRecoil(float force, float recoverySpeed)
    {
        _currentRecoilDistance = force;
        _recoilRecoverySpeed = recoverySpeed;
    }

    private void RestoreRecoil()
    {
        // Trả khoảng cách giật về 0 dần dần theo thời gian
        if (_currentRecoilDistance > 0)
        {
            _currentRecoilDistance = Mathf.Lerp(_currentRecoilDistance, 0f, Time.deltaTime * _recoilRecoverySpeed);
        }
    }

    public Vector2 GetAimDirection()
    {
        if (autoAim)
        {
            Collider2D target = Physics2D.OverlapCircle(player.position, viewRange, enemyLayer);
            if (target != null)
                return ((Vector2)target.transform.position - (Vector2)player.position).normalized;
        }

        // Lấy vị trí chuột theo Input System mới
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        return ((Vector2)mouseWorldPos - (Vector2)player.position).normalized;
    }


    // Hàm bổ sung: Lấy hướng bắn có tính độ lệch tâm (Spread) dùng cho Fire Strategy
    public Vector2 GetAimDirectionWithSpread(float maxSpreadAngle)
    {
        Vector2 baseDir = GetAimDirection();
        if (maxSpreadAngle <= 0) return baseDir;

        // Tạo một góc lệch ngẫu nhiên
        float randomAngle = Random.Range(-maxSpreadAngle, maxSpreadAngle);

        // Quay vector hướng gốc đi một góc ngẫu nhiên
        return Quaternion.Euler(0, 0, randomAngle) * baseDir;
    }
}