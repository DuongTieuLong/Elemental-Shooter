using System;
using UnityEngine;

public class PlayerArmAimController : MonoBehaviour
{
    [Header("Arm Transforms")]
    [SerializeField] private Transform leftArm;  // Tay trước (Tay phụ bám ốp súng)
    [SerializeField] private Transform rightArm; // Tay sau (Tay chính cầm cò/vũ khí)
    [SerializeField] private TrailRenderer saberTrail;

    [Header("Aiming Settings")]
    [SerializeField] private float leftArmAngleOffset = 180f;  // Độ lệch góc tay phụ (mặc định 180 độ vì sprite tay trước vẽ ngược hướng)
    [SerializeField] private float rightArmAngleOffset = 180f; // Độ lệch góc tay chính (mặc định 180 độ vì sprite tay sau vẽ ngược hướng)
    [SerializeField] private float armLength = 1.0f;            // Độ dài sải tay từ vai đến tay
    [SerializeField] private float verticalOffset = 0.35f;      // Độ lệch dịch chuyển súng lên trên (tránh che chân/váy)
    [Tooltip("Tỷ lệ trượt tối thiểu của tay phụ về phía báng súng khi ngắm thẳng đứng (tránh duỗi tay quá dài)")]
    [SerializeField] [Range(0f, 1f)] private float minGripDistanceRatio = 0.3f;

    [Header("Visual Recoil Settings")]
    [SerializeField] private float armRecoilMultiplier = 0.5f;

    [Header("Manual Adjustment (Runtime Tweaks in Local Space)")]
    [Tooltip("Tinh chỉnh vị trí vai tay sau (Right Arm - Tay chính) trong local space")]
    [SerializeField] private Vector3 rightArmPositionOffset = Vector3.zero;
    [Tooltip("Tinh chỉnh vị trí vai tay trước (Left Arm - Tay phụ) trong local space")]
    [SerializeField] private Vector3 leftArmPositionOffset = Vector3.zero;

    [Header("Melee Settings")]
    [Tooltip("Khoảng cách dịch chuyển của vũ khí cận chiến dọc theo trục X để đưa chuôi kiếm về tay")]
    [SerializeField] private float meleeGripOffsetX = 0.5f;
    [Tooltip("Khoảng cách dịch chuyển của vũ khí cận chiến dọc theo trục Y")]
    [SerializeField] private float meleeGripOffsetY = 0f;

    [Header("Melee Swing Settings")]
    [SerializeField] private float swingDuration = 0.15f;
    [SerializeField] private float swingAngleStart = 80f;
    [SerializeField] private float swingAngleEnd = -80f;

    [Header("Dynamic Offset Settings ([-30, 30] degree window)")]
    [Tooltip("Góc giới hạn (độ) từ trục ngang để áp dụng tinh chỉnh động (mặc định 30 độ)")]
    [SerializeField] private float dynamicOffsetAngleLimit = 30f;
    [Tooltip("Lượng tinh chỉnh vị trí vai trái tối đa (X, Y) khi súng lệch góc (tự động nhân với tỷ lệ góc và triệt tiêu ngoài vùng giới hạn)")]
    [SerializeField] private Vector3 leftArmDynamicOffset = Vector3.zero;

    private Weapon _weapon;
    private WeaponAim _weaponAim;
    private Vector3 _originalGunScale = Vector3.one;

    private Transform _bodyTransform;
    private Vector3 _bodyOrigPos;

    // Lưu vị trí gốc của hai cánh tay để khôi phục khi cất vũ khí
    private Vector3 _leftArmOrigPos;
    private Vector3 _rightArmOrigPos;

    // Biến trạng thái hoạt ảnh chém cận chiến (Melee Swing)
    private bool _isSwinging;
    private float _swingTimer;
    private float _swingDuration;
    private float _swingAngleStart;
    private float _swingAngleEnd;

    private Quaternion _animatorBodyRotation = Quaternion.identity;

    public Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        _weapon = GetComponent<Weapon>();
        _weaponAim = GetComponent<WeaponAim>();

        _bodyTransform = transform.Find("Body");
        if (_bodyTransform != null) _bodyOrigPos = _bodyTransform.localPosition;

        if (leftArm == null) leftArm = transform.Find("Body/Left Arm");
        if (rightArm == null) rightArm = transform.Find("Body/Right Arm");

        if (leftArm != null) _leftArmOrigPos = leftArm.localPosition;
        if (rightArm != null) _rightArmOrigPos = rightArm.localPosition;

        if (_weaponAim != null && _weaponAim.GunPos != null)
        {
            _originalGunScale = _weaponAim.GunPos.localScale;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHit += SetTriggerAnimationHit;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerHit -= SetTriggerAnimationHit;
        RestoreOriginalTransforms();
    }

    public void SetTriggerAnimationHit()
    {
        animator.SetTrigger("IsHit");
    }

    private void RestoreOriginalTransforms()
    {
        if (leftArm != null)
        {
            leftArm.localPosition = _leftArmOrigPos;
            leftArm.localScale = Vector3.one;
        }
        if (rightArm != null)
        {
            rightArm.localPosition = _rightArmOrigPos;
            rightArm.localScale = Vector3.one;
        }
    }

    public void TriggerSwing()
    {
        TriggerSwing(swingDuration, swingAngleStart, swingAngleEnd);
    }

    public void TriggerSwing(float duration, float startAngle, float endAngle)
    {
        _swingDuration = duration;
        _swingTimer = duration;
        _swingAngleStart = startAngle;
        _swingAngleEnd = endAngle;
        _isSwinging = true;
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0) return; // Block arm aiming updates when paused

        if (_bodyTransform != null)
        {
            _animatorBodyRotation = _bodyTransform.localRotation;
        }

        // Nếu không cầm vũ khí, khôi phục lại vị trí và tỷ lệ cánh tay
        if (_weapon == null || _weapon.data == null || _weaponAim == null)
        {
            RestoreOriginalTransforms();
            return;
        }

        if (leftArm == null || rightArm == null) return;

        WeaponData data = _weapon.data;
        Vector2 aimDir = _weaponAim.GetAimDirection();

        // Phòng tránh lỗi chia cho 0 nếu armLength bị đặt nhầm = 0 trong Inspector
        if (armLength <= 0.001f) armLength = 1.0f;

        // 1. Cập nhật bộ đếm thời gian chém cận chiến
        if (_isSwinging)
        {
            _swingTimer -= Time.deltaTime;
            if (_swingTimer <= 0)
            {
                _isSwinging = false;
            }
        }

        // Hướng lật của nhân vật (1 nếu quay phải, -1 nếu quay trái/flip)
        float flipSign = transform.localScale.x < 0 ? -1f : 1f;

        // 2. Tính toán vị trí súng nhắm mục tiêu trong LOCAL SPACE của Body
        // Sử dụng InverseTransformVector để tự động lật ngược trục X khi nhân vật bị flip
        Vector3 localAimDir = rightArm.parent.InverseTransformVector(aimDir).normalized;
        
        // Vị trí súng mục tiêu trong local space (dịch chuyển lên trên bằng verticalOffset)
        Vector3 baseTargetPosLocal = _rightArmOrigPos + localAimDir * armLength + Vector3.up * verticalOffset;

        // 3. Tính toán góc xoay tay chính (Right Arm - Tay sau) để hướng về phía súng
        Vector3 localShoulderPos = _rightArmOrigPos + rightArmPositionOffset;
        Vector3 localAimDirToGun = (baseTargetPosLocal - localShoulderPos).normalized;
        float localAimAngle = Mathf.Atan2(localAimDirToGun.y, localAimDirToGun.x) * Mathf.Rad2Deg;

        // 4. Xử lý các phong cách cầm vũ khí
        if (data.gripStyle == WeaponGripStyle.Melee)
        {
            HandleMeleeAim(localAimAngle, baseTargetPosLocal, localAimDirToGun, flipSign);
        }
        else if (data.gripStyle == WeaponGripStyle.TwoHanded)
        {
            if (saberTrail.enabled) saberTrail.emitting = false;
            HandleTwoHandedAim(localAimAngle, baseTargetPosLocal, localAimDirToGun, flipSign);
        }
        else // OneHanded (Pistol)
        {
            if (saberTrail.enabled) saberTrail.emitting = false;
            HandleOneHandedAim(localAimAngle, baseTargetPosLocal, localAimDirToGun, flipSign);
        }
    }

    private void SetGunTransform(Vector3 localPos, float localAngle)
    {
        SetGunTransform(localPos, Quaternion.Euler(0, 0, localAngle));
    }

    private void SetGunTransform(Vector3 localPos, Quaternion localRotation)
    {
        if (_weaponAim.GunPos == null) return;

        // Chuyển đổi vị trí từ local space của Body (rightArm.parent) sang World Space
        Vector3 worldPos = rightArm.parent.TransformPoint(localPos);
        
        // Chuyển vị trí World về local space của cha GunPos (để thừa hưởng đúng tỉ lệ scale âm khi flip)
        _weaponAim.GunPos.localPosition = _weaponAim.GunPos.parent.InverseTransformPoint(worldPos);

        // Áp dụng xoay súng theo Quaternion để hỗ trợ nghiêng 3D
        _weaponAim.GunPos.localRotation = localRotation;
        _weaponAim.GunPos.localScale = _originalGunScale;
    }

    private void HandleOneHandedAim(float localAimAngle, Vector3 baseTargetPosLocal, Vector3 localAimDirToGun, float flipSign)
    {
      
        // Tính toán vị trí súng có tính độ giật (recoil) trước
        float recoil = 0f;
        if (_weaponAim.GunPos != null)
        {
            recoil = _weaponAim.CurrentRecoilDistance * armRecoilMultiplier;
        }
        Vector3 gunPosLocal = baseTargetPosLocal - localAimDirToGun * recoil;

        // 1. Tay phải (tay chính) ngắm súng
        rightArm.localPosition = _rightArmOrigPos + rightArmPositionOffset;
        rightArm.localRotation = Quaternion.Euler(0, 0, localAimAngle + rightArmAngleOffset);
        
        // Co giãn nhẹ tay chính theo độ giật (sử dụng gunPosLocal để tay co lại tương ứng)
        float mainDist = Vector3.Distance(rightArm.localPosition, gunPosLocal);
        rightArm.localScale = new Vector3(mainDist / armLength, 1f, 1f);

        // Tay trái (tay phụ) tự do vung theo Animator
        leftArm.localPosition = _leftArmOrigPos;
        leftArm.localScale = Vector3.one;

        // 2. Định vị súng ở đầu cánh tay phải
        if (_weaponAim.GunPos != null)
        {
            SetGunTransform(gunPosLocal, localAimAngle);
        }
    }

    private void HandleTwoHandedAim(float localAimAngle, Vector3 baseTargetPosLocal, Vector3 localAimDirToGun, float flipSign)
    {
    
        // Tính toán vị trí súng có tính độ giật (recoil) trước
        float recoil = 0f;
        if (_weaponAim.GunPos != null)
        {
            recoil = _weaponAim.CurrentRecoilDistance * armRecoilMultiplier;
        }
        Vector3 gunPosLocal = baseTargetPosLocal - localAimDirToGun * recoil;

        // 1. Tay phải (tay chính) ngắm súng
        rightArm.localPosition = _rightArmOrigPos + rightArmPositionOffset;
        rightArm.localRotation = Quaternion.Euler(0, 0, localAimAngle + rightArmAngleOffset);
        
        // Co giãn nhẹ tay chính theo độ giật (sử dụng gunPosLocal để tay co lại tương ứng)
        float mainDist = Vector3.Distance(rightArm.localPosition, gunPosLocal);
        rightArm.localScale = new Vector3(mainDist / armLength, 1f, 1f);

        // 2. Định vị súng ở đầu cánh tay phải (bàn tay)
        if (_weaponAim.GunPos != null)
        {
            SetGunTransform(gunPosLocal, localAimAngle);
        }

        // 3. Tay trái (tay phụ phía trước) đỡ thân súng:
        if (_weaponAim.GunPos != null)
        {
            // Lấy thành phần hướng ngang của súng (từ 0 đến 1)
            float horizontalFactor = Mathf.Abs(localAimDirToGun.x);
            float t = Mathf.SmoothStep(0f, 1f, horizontalFactor);

            // Co vai trái (tay phụ) sát vào thân khi ngắm thẳng đứng (t gần 0)
            // Ngắm lên (12h): Cho phép vai dịch sâu vào trong (gần vai phải) để ôm súng tự nhiên
            // Ngắm xuống (6h): Hạn chế dịch vai để tránh cẳng tay bị đè lên tay phải
            float defaultShoulderX = _leftArmOrigPos.x + leftArmPositionOffset.x;
            float targetShoulderXMin = (localAimDirToGun.y > 0f) ? (rightArm.localPosition.x + 0.3f) : (rightArm.localPosition.x + 0.65f);
            float targetShoulderX = Mathf.Lerp(targetShoulderXMin, defaultShoulderX, t);
            
            // Tính toán lượng tinh chỉnh vai động (Dynamic Offset) trong khoảng [-limit, limit] độ quanh hướng ngang
            Vector3 activeDynamicOffset = Vector3.zero;
            if (dynamicOffsetAngleLimit > 0.01f)
            {
                float absAngle = Mathf.Abs(localAimAngle);
                if (absAngle < dynamicOffsetAngleLimit)
                {
                    // fadeFactor = 1 tại 0 độ (ngang), về 0 tại giới hạn góc
                    float fadeFactor = Mathf.Clamp01(1f - (absAngle / dynamicOffsetAngleLimit));
                    fadeFactor = Mathf.SmoothStep(0f, 1f, fadeFactor);
                    
                    // normalizedAngle nằm trong khoảng [-1, 1] đại diện cho hướng UP (+) hoặc DOWN (-)
                    float normalizedAngle = localAimAngle / dynamicOffsetAngleLimit;
                    
                    // Tính toán offset động dịch vai
                    activeDynamicOffset = leftArmDynamicOffset * normalizedAngle * fadeFactor;
                }
            }
            
            leftArm.localPosition = new Vector3(targetShoulderX, _leftArmOrigPos.y + leftArmPositionOffset.y, _leftArmOrigPos.z + leftArmPositionOffset.z) + activeDynamicOffset;

            // Tinh chỉnh góc xoay tay phụ động khi ngắm xuống hướng 6h (quá -30 độ)
            // Xoay góc offset từ mặc định về 30 độ để cẳng tay và bàn tay bám súng tự nhiên nhất
            float currentLeftArmAngleOffset = leftArmAngleOffset;
            if (localAimAngle < -30f)
            {
                // Lấy tỷ lệ từ -30 độ (t_angle = 0) đến -90 độ (t_angle = 1)
                float t_angle = Mathf.Clamp01((localAimAngle + 30f) / -60f);
                currentLeftArmAngleOffset = Mathf.Lerp(leftArmAngleOffset, 30f, t_angle);
            }

            // Tính toán vị trí bám tay phụ động: trượt dọc theo thân súng để tránh bị duỗi quá dài khi hướng súng chỉ thẳng đứng
            float maxGripX = _weapon.data.leftHandGripOffset.x;
            float minGripX = maxGripX * minGripDistanceRatio;
            float dynamicGripX = Mathf.Lerp(minGripX, maxGripX, t);

            Vector2 dynamicGripOffset = new Vector2(dynamicGripX, _weapon.data.leftHandGripOffset.y);

            // Lấy vị trí bám tay trái trên súng trong local space của Body
            Vector3 leftGripLocal = gunPosLocal + Quaternion.Euler(0, 0, localAimAngle) * (Vector3)dynamicGripOffset;
            
            // Xoay tay phụ hướng về phía Grip súng
            Vector3 localDirLeft = (leftGripLocal - leftArm.localPosition).normalized;
            float leftAngle = Mathf.Atan2(localDirLeft.y, localDirLeft.x) * Mathf.Rad2Deg;
            
            // Xoay tay phụ trong hệ tọa độ cục bộ (để tự động đảo hướng xoay theo cha khi flip)
            leftArm.localRotation = Quaternion.Euler(0, 0, leftAngle + currentLeftArmAngleOffset);

            // Co giãn nhẹ tay phụ để bàn tay chạm đúng vào điểm Grip
            float leftDist = Vector3.Distance(leftArm.localPosition, leftGripLocal);
            leftArm.localScale = new Vector3(leftDist / armLength, 1f, 1f);
        }
    }

    private void HandleMeleeAim(float localAimAngle, Vector3 baseTargetPosLocal, Vector3 localAimDirToGun, float flipSign)
    {
        float targetArmAngle = localAimAngle;

        if (_isSwinging)
        {
            float t = 1f - (_swingTimer / _swingDuration);

            // Nội suy góc chém từ Start đến End (Ví dụ Start = 60, End = -60)
            float swingOffset = Mathf.Lerp(_swingAngleStart, _swingAngleEnd, t);
            
            if (transform.localScale.x < 0)
            {
                swingOffset = -swingOffset;
            }
            
            targetArmAngle += swingOffset;

            // Áp dụng nhún/nghiêng người (Body Lean & Crouching via Z-rotation)
            if (_bodyTransform != null)
            {
                float tiltAngle = Mathf.Sin(t * Mathf.PI) * -12f; // Nghiêng 12 độ ở đòn thường
                if (_swingDuration > 0.16f) // Đòn 3 nặng
                {
                    tiltAngle = Mathf.Sin(t * Mathf.PI) * -22f; // Nghiêng 22 độ
                }
                _bodyTransform.localRotation = _animatorBodyRotation * Quaternion.Euler(0, 0, tiltAngle);
            }

            // 1. BẬT TRAIL: Khi đang trong quá trình vung kiếm
            if (saberTrail != null)
            {
                // Kiểm tra xem có phải là frame đầu tiên của nhát chém mới không
                if (!saberTrail.emitting)
                {
                    saberTrail.Clear();      // CẮT ĐỨT đuôi cũ, chống nhảy hình
                    saberTrail.emitting = true; // Bắt đầu xả trail mới
                }
            }
        }
        else
        {
            // 2. TẮT TRAIL: Khi đã chém xong (hoặc đang đứng im không chém)
            if (saberTrail != null && saberTrail.emitting)
            {
                saberTrail.emitting = false; // Ngừng xả điểm mới, để đuôi cũ tự mờ dần
            }
            if (_bodyTransform != null)
            {
                _bodyTransform.localRotation = _animatorBodyRotation;
            }
        }

        // --- Giữ nguyên logic xử lý cánh tay và thanh kiếm bên dưới của bạn ---
        // Tay phải (tay chính) cầm kiếm
        rightArm.localPosition = _rightArmOrigPos + rightArmPositionOffset;
        rightArm.localRotation = Quaternion.Euler(0, 0, targetArmAngle + rightArmAngleOffset);

        float mainDist = Vector3.Distance(rightArm.localPosition, baseTargetPosLocal);
        rightArm.localScale = new Vector3(mainDist / armLength, 1f, 1f);

        leftArm.localPosition = _leftArmOrigPos;
        leftArm.localScale = Vector3.one;

        if (_weaponAim.GunPos != null)
        {
            // Tính toán vị trí bàn tay phải xoay theo góc chém hiện tại
            Vector3 handPosLocal = rightArm.localPosition + Quaternion.Euler(0, 0, targetArmAngle) * Vector3.right * mainDist;

            // Dịch chuyển tâm kiếm về phía trước để chuôi kiếm nằm đúng bàn tay cầm
            Vector3 gunPosLocal = handPosLocal + Quaternion.Euler(0, 0, targetArmAngle) * new Vector3(meleeGripOffsetX, meleeGripOffsetY, 0);

            // Xoay kiếm theo trục Z đồng thời xoay nhẹ Y để tạo hiệu ứng nghiêng 3D khi vung
            float yRot = 0f;
            if (_isSwinging)
            {
                float t = 1f - (_swingTimer / _swingDuration);
                yRot = Mathf.Sin(t * Mathf.PI) * 40f; // Xoay kiếm nghiêng 3D tối đa 40 độ
            }
            Quaternion gunLocalRot = Quaternion.Euler(0, yRot, targetArmAngle);

            SetGunTransform(gunPosLocal, gunLocalRot);
        }
    }
}
