using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Sprites")]
    [SerializeField] private Sprite cursorSprite;
    [SerializeField] private Sprite crosshairSprite;
    [SerializeField] private Sprite crosshairFireSprite;

    [Header("Settings")]
    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float spreadScaleFactor = 0.5f; // Tỉ lệ nở ra của crosshair dựa trên độ spread
    [SerializeField] private float recoilZoomFactor = 1.5f;  // Độ zoom in/out bồi thêm khi súng nảy (kick)
    [SerializeField] private float recoilJumpFactor = 15f;   // Độ dịch chuyển (giật) của crosshair trên màn hình

    private Image _cursorImage;
    private RectTransform _rectTransform;
    private Weapon _currentWeapon;

    private Vector2 _currentRecoilOffset;
    private Vector2 _targetRecoilOffset;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.parent.gameObject);
            
            // Tìm hoặc thêm Image UI component
            _cursorImage = GetComponent<Image>();
            if (_cursorImage == null)
            {
                _cursorImage = gameObject.AddComponent<Image>();
            }
            
            _rectTransform = GetComponent<RectTransform>();
            _cursorImage.raycastTarget = false; // Đảm bảo cursor không cản trở thao tác UI
            
            Cursor.visible = false; // Ẩn con trỏ chuột mặc định của OS
            Cursor.lockState = CursorLockMode.Confined; // Giữ chuột trong cửa sổ game
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        GameEvents.OnBulletFired += HandleBulletFired;
    }

    private void OnDisable()
    {
        GameEvents.OnBulletFired -= HandleBulletFired;
    }

    private void HandleBulletFired(Vector2 bulletWorldDirection)
    {
        if (_currentWeapon == null || _currentWeapon.weaponAim == null || Camera.main == null) return;
        
        // Vị trí player trên màn hình
        Vector3 playerWorldPos = _currentWeapon.transform.position;
        Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(playerWorldPos);

        // Vị trí chuột thật trên màn hình
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Vector hướng ngắm gốc trên màn hình (Từ player đến chuột)
        Vector2 screenAimVector = mousePos - (Vector2)playerScreenPos;

        // Tính góc chênh lệch (độ tản mát đạn) ở world space
        Vector2 pureWorldAim = _currentWeapon.weaponAim.GetAimDirection(); 
        float deltaAngle = Vector2.SignedAngle(pureWorldAim, bulletWorldDirection);

        // Xoay vector màn hình theo đúng góc lệch của đạn
        float rad = deltaAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        
        Vector2 rotatedScreenAim = new Vector2(
            screenAimVector.x * cos - screenAimVector.y * sin,
            screenAimVector.x * sin + screenAimVector.y * cos
        );

        // Lấy khoảng lệch thực tế
        Vector2 exactOffset = rotatedScreenAim - screenAimVector;
        
        // Cộng dồn offset (cho các súng shotgun / burst) và kết hợp với recoil nảy ra sau (kick lùi)
        float currentRecoilDist = _currentWeapon.weaponAim.CurrentRecoilDistance;
        Vector2 kickBackOffset = -rotatedScreenAim.normalized * currentRecoilDist * recoilJumpFactor;

        _targetRecoilOffset = exactOffset + kickBackOffset;
        // Gán ngay lập tức để tạo lực giật nảy thay vì chờ Lerp
        _currentRecoilOffset = _targetRecoilOffset;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // 1. Cập nhật vị trí chuột thực tế
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // Cố gắng tìm vũ khí hiện tại nếu chưa có
        if (_currentWeapon == null)
        {
            _currentWeapon = FindFirstObjectByType<Weapon>();
        }

        // Kiểm tra xem có đang ở trong Gameplay hay không
        bool isGameplay = GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Gameplay;

        if (isGameplay && _currentWeapon != null)
        {
            HandleCrosshairState(mousePos);
        }
        else
        {
            HandleCursorState(mousePos);
        }
    }

    private void HandleCursorState(Vector2 mousePos)
    {
        _cursorImage.sprite = cursorSprite;
        _rectTransform.position = mousePos;
        _rectTransform.localScale = Vector3.one * baseScale;
        _rectTransform.localRotation = Quaternion.identity;
        _targetRecoilOffset = Vector2.zero;
        _currentRecoilOffset = Vector2.zero;
    }

    private void HandleCrosshairState(Vector2 mousePos)
    {
        if (_currentWeapon.weaponAim == null) return;

        float currentRecoilDist = _currentWeapon.weaponAim.CurrentRecoilDistance;

        // 1. Trạng thái hình ảnh (Chỉ hiển thị Fire Sprite khi súng thực sự giật/vừa bắn xong)
        if (currentRecoilDist > 0.05f)
        {
            _cursorImage.sprite = crosshairFireSprite;
        }
        else
        {
            _cursorImage.sprite = crosshairSprite;
            _targetRecoilOffset = Vector2.zero;
        }

        // Mượt mà hóa độ giật nảy vị trí về 0 (khi hết giật)
        _currentRecoilOffset = Vector2.Lerp(_currentRecoilOffset, _targetRecoilOffset, Time.deltaTime * 20f);

        // Áp dụng vị trí: Tâm ngắm dịch chuyển chính xác theo hướng đạn lệch
        _rectTransform.position = mousePos + _currentRecoilOffset;

        // Xoay nhẹ theo recoil offset để tạo cảm giác giật
        float targetRotation = -_currentRecoilOffset.x * 0.5f; 
        _rectTransform.localRotation = Quaternion.Lerp(_rectTransform.localRotation, Quaternion.Euler(0, 0, targetRotation), Time.deltaTime * 15f);

        // 3. Áp dụng độ giãn (Zoom in/out) kết hợp giữa Spread (độ tản mát cơ bản) và Recoil Kick (độ giật nổ)
        float currentSpread = _currentWeapon.GetCurrentSpread();
        float targetScale = baseScale + (currentSpread * spreadScaleFactor) + (currentRecoilDist * recoilZoomFactor);
        
        // Lerp scale để crosshair co giãn mượt mà
        _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, Vector3.one * targetScale, Time.deltaTime * 15f);
    }
}
