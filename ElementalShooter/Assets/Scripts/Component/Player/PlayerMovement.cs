using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private InputSystem_Actions _controls;
    private Vector2 _moveInput;
    private StatHandler _stats;
    private Animator _animator;
    private Weapon _weapon;
    private WeaponAim _weaponAim;
    
    private bool _isFacingRight = true; // Ban đầu nhân vật hướng sang phải
    private bool _isLunging = false; // Trạng thái đang lao chém

    public bool IsMoving => _moveInput != Vector2.zero; // Kiểm tra xem nhân vật có đang di chuyển hay không
    public bool IsSprinting => _controls.Player.Sprint.IsPressed(); // Kiểm tra xem nhân vật có đang chạy hay không

    void Awake()
    {
        _stats = GetComponent<StatHandler>();
        _animator = GetComponent<Animator>();
        _weapon = GetComponent<Weapon>();
        _weaponAim = GetComponent<WeaponAim>();
        
        _controls = new InputSystem_Actions();
        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return; // Block all updates when paused

        float speed = _stats != null ? _stats.GetValue(StatType.MoveSpeed) : 5f;

        if (_controls.Player.Sprint.IsPressed())
        {
            speed *= 1.5f; // Tăng tốc độ khi chạy
        }

        // Sử dụng _moveInput để di chuyển nhân vật (chặn di chuyển khi đang lao chém)
        Vector2 direction = new Vector2(_moveInput.x, _moveInput.y);
        if (!_isLunging)
        {
            transform.Translate(direction.normalized * Time.deltaTime * speed);
        }

        // Cập nhật hoạt ảnh dựa trên tốc độ di chuyển thực tế (chuyển về Idle khi đang chém để tấn vững chắc)
        if (_animator != null)
        {
            float currentSpeed = direction.magnitude * speed;
            _animator.SetFloat("Speed", _isLunging ? 0f : currentSpeed);

            // Kiểm tra trạng thái đi lùi: nếu đang di chuyển ngược hướng nhìn của nhân vật
            bool isMoving = _moveInput != Vector2.zero;
            bool isBackward = false;
            if (isMoving && !_isLunging)
            {
                isBackward = (_isFacingRight && _moveInput.x < -0.1f) || (!_isFacingRight && _moveInput.x > 0.1f);
            }
            _animator.SetBool("IsBackward", isBackward);
        }

        bool hasWeapon = _weapon != null && _weapon.data != null;

        if (hasWeapon)
        {
            if (_weaponAim != null)
            {
                Vector2 aimDir = _weaponAim.GetAimDirection();
                float flipDeadZone = 0.15f; // Ngưỡng trễ tránh giật hình khi ngắm góc 12h/6h

                // Lấy khoảng cách chuột để tránh flip liên tục khi hover gần tâm nhân vật
                float distanceToMouse = 999f;
                if (Camera.main != null && UnityEngine.InputSystem.Mouse.current != null)
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
                    distanceToMouse = Vector2.Distance(transform.position, mouseWorldPos);
                }

                if (distanceToMouse > 0.8f) // Bán kính 0.8m tránh lật liên tục
                {
                    if (aimDir.x > flipDeadZone && !_isFacingRight)
                    {
                        Flip();
                    }
                    else if (aimDir.x < -flipDeadZone && _isFacingRight)
                    {
                        Flip();
                    }
                }
            }
        }
        else
        {
            if (_moveInput.x > 0 && !_isFacingRight)
            {
                Flip();
            }
            else if (_moveInput.x < 0 && _isFacingRight)
            {
                Flip();
            }
        }
    }

    public void ApplyMeleeLunge(Vector2 direction, float distance, float duration)
    {
        StartCoroutine(LungeRoutine(direction.normalized, distance, duration));
    }

    private System.Collections.IEnumerator LungeRoutine(Vector2 direction, float distance, float duration)
    {
        _isLunging = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float speedMultiplier = Mathf.Sin((1f - t) * Mathf.PI * 0.5f);
            float step = (distance / duration) * speedMultiplier * Time.deltaTime;
            
            transform.Translate(direction * step, Space.World);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        _isLunging = false;
    }

    void Flip()
    {
        // Đổi trạng thái hướng
        _isFacingRight = !_isFacingRight;

        // Nhân trục X của localScale với -1
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

    private void OnEnable() => _controls.Player.Enable();
    private void OnDisable() => _controls.Player.Disable();
}
