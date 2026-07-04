using UnityEngine;

public class MeleeSlashStrategy : IFireStrategy
{
    private MeleeStrategyData _meleeData;

    public void Initialize(IAttackStrategyData data)
    {
        _meleeData = data as MeleeStrategyData;
        if (_meleeData == null)
        {
            Debug.LogError("MeleeSlashStrategy requires MeleeStrategyData, but received an invalid type.");
        }
    }
    
    // Lưu trữ trạng thái Combo của chiến thuật chém cận chiến
    private int _comboIndex = 0;
    private float _lastAttackTime = 0f;
    private const float ComboResetTime = 0.8f; // Reset combo sau 0.8 giây không chém liên tục

    public void ExecuteFire(Weapon weapon, Transform muzzle, StatHandler stats, WeaponAim weaponAim, LayerMask enemyLayer)
    {
        if (_meleeData == null) return;
        float currentTime = Time.time;
        // Nếu khoảng cách giữa 2 phát chém quá lâu, reset combo về đòn 1
        if (currentTime - _lastAttackTime > ComboResetTime)
        {
            _comboIndex = 0;
        }

        float weaponDame = weapon != null ? weapon.GetFinalDamage() : 0;
        float playerDame = stats.GetValue(StatType.Damage);

        Vector2 aimDir = weaponAim.GetAimDirection();
        Vector2 checkCenter = (Vector2)stats.transform.position + aimDir * (_meleeData.meleeAttackRange * 0.5f);

        // Lấy nguyên tố ngẫu nhiên từ nhân vật
        var element = stats.GetRandomEffect();

        // rung cam
        if (CameraShakeManager.Instance != null)
        {
            float finalShake = weapon.data.cameraShakeIntensity * weapon.CurrentRecoilMultiplier;
            CameraShakeManager.Instance.ShakeCamera(finalShake);
        }
        //am thanh
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, muzzle.position, 0.8f);
        }


        // 1. Xác định thông số chém, sát thương và quãng đường lao (Lunge) dựa vào Combo Index
        float duration = 0.15f;
        float startAngle = 80f;
        float endAngle = -80f;
        float damageMultiplier = 1.0f;
        float lungeDistance = 1.2f;   // Đòn thường lao 0.8m
        float lungeDuration = 0.12f;

        if (_comboIndex == 0)
        {
            // Đòn 1: Chém xéo từ trên xuống dưới
            duration = 0.15f;
            startAngle = 80f;
            endAngle = -80f;
            damageMultiplier = 1.0f;
            lungeDistance = 1.2f;
            lungeDuration = 0.12f;
        }
        else if (_comboIndex == 1)
        {
            // Đòn 2: Chém ngược từ dưới lên trên
            duration = 0.15f;
            startAngle = -80f;
            endAngle = 80f;
            damageMultiplier = 1.0f;
            lungeDistance = 1.2f;
            lungeDuration = 0.12f;
        }
        else // _comboIndex == 2
        {
            // Đòn 3: Chém bổ củi cực mạnh!
            duration = 0.22f;
            startAngle = 130f;
            endAngle = -130f;
            damageMultiplier = 1.5f; // Sát thương đòn cuối x1.5
            lungeDistance = 2.4f;   // Lao xa gấp đôi đòn thường
            lungeDuration = 0.18f;
        }

        // 2. Kích hoạt hoạt ảnh vung kiếm trên cánh tay
        var armController = stats.GetComponent<PlayerArmAimController>();
        if (armController != null)
        {
            armController.TriggerSwing(duration, startAngle, endAngle);
        }

        // 3. Thực hiện lao người tấn công (Melee Lunge)
        var movement = stats.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ApplyMeleeLunge(aimDir, lungeDistance, lungeDuration);
        }

        // 4. Lấy hiệu ứng vệt chém và âm thanh chém trúng từ MeleeConfigData
        GameObject slashPrefab = null;
        GameObject hitVFXPrefab = null;
        AudioClip hitSFX = null;

        if (_meleeData != null && _meleeData.meleeConfig != null)
        {
            var meleeVisual = _meleeData.meleeConfig.GetVisualForElement(element);
            slashPrefab = meleeVisual.slashVFXPrefab != null ? meleeVisual.slashVFXPrefab : _meleeData.meleeConfig.defaultSlashVFXPrefab;
            hitVFXPrefab = meleeVisual.hitVFXPrefab;
            hitSFX = meleeVisual.customHitSFX != null ? meleeVisual.customHitSFX : _meleeData.meleeConfig.defaultHitSFX;
        }

        // 5. Tạo hiệu ứng vệt chém (Slash Trail)
        if (slashPrefab != null && PoolManager.Instance != null)
        {
            GameObject slashVisual = PoolManager.Instance.Get(slashPrefab, muzzle.position);
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

            // Xoay hiệu ứng và lật tỉ lệ khớp với hướng chém
            Vector3 visualScale = slashVisual.transform.localScale;
            float absX = Mathf.Abs(visualScale.x);
            float absY = Mathf.Abs(visualScale.y);

            // Chém từ dưới lên (đòn 2) -> Lật ngược trục Y để vệt chém bay lên tương ứng
            float targetY = (_comboIndex == 1) ? -absY : absY;
            // Đòn 3 -> Phóng to vệt chém lên 1.3 lần cho uy lực
            float scaleMultiplier = (_comboIndex == 2) ? 1.3f : 1.0f;

            // Nghiêng vệt chém 25 độ theo trục X để tạo chiều sâu 3D chéo màn hình
            float tiltX = 25f;
            float tiltY = 0f;

            if (weapon.transform.localScale.x < 0)
            {
                slashVisual.transform.rotation = Quaternion.Euler(tiltX, tiltY, angle + 180f);
                slashVisual.transform.localScale = new Vector3(-absX * scaleMultiplier, targetY * scaleMultiplier, visualScale.z);
            }
            else
            {
                slashVisual.transform.rotation = Quaternion.Euler(tiltX, tiltY, angle);
                slashVisual.transform.localScale = new Vector3(absX * scaleMultiplier, targetY * scaleMultiplier, visualScale.z);
            }

            var deactivator = slashVisual.GetComponent<SimpleDeactivator>();
            if (deactivator == null)
            {
                deactivator = slashVisual.AddComponent<SimpleDeactivator>();
            }
            deactivator.delay = duration + 0.1f;
        }

        // 6. Phát hiện và gây sát thương các mục tiêu trong hình quạt
        float finalDamage = (weaponDame + playerDame) * damageMultiplier;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(checkCenter, _meleeData.meleeAttackRange * 0.7f, enemyLayer);
        float finalMultiplier = stats.GetElementModifier(element);

        foreach (var collider in hitColliders)
        {
            Vector2 dirToTarget = ((Vector2)collider.transform.position - (Vector2)stats.transform.position).normalized;
            float dot = Vector2.Dot(aimDir, dirToTarget);

            if (dot > 0.4f) // Nằm trong góc ~130 độ phía trước
            {
                // Gây hiệu ứng nguyên tố
                if (collider.TryGetComponent<IStatusReceiver>(out var status))
                {
                    status.ReceiveElement(new ElementData
                    {
                        Type = element,
                        Multiplier = finalMultiplier,
                        SourceDamage = finalDamage
                    });
                }

                Debug.Log("Enemy collider is" + collider.name);
                // Gây sát thương
                if (collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageInfo
                    {
                        Amount = finalDamage,
                        Element = element
                    });
                }

                // Tạo hiệu ứng nổ (Hit VFX) tại vị trí quái khi bị chém trúng
                if (hitVFXPrefab != null)
                {
                    Vector2 shallowDir = new Vector2(aimDir.x, aimDir.y * 0.3f).normalized;
                    Quaternion hitRotation = Quaternion.LookRotation(Vector3.forward, shallowDir);
                    GameObject vfxObj = PoolManager.Instance.Get(hitVFXPrefab, collider.transform.position);

                    vfxObj.transform.rotation = hitRotation * Quaternion.Euler(0, 0, 90);
                    vfxObj.GetComponent<ParticlePoolReturn>()?.SetBasePrefabs(hitVFXPrefab);
                }

                // Phát âm thanh khi chém trúng kẻ địch
                if (hitSFX != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(hitSFX, collider.transform.position, 0.8f);
                }
            }
        }

        // 7. Thiết lập thời gian hồi chiêu tùy biến cho nhịp Combo để tránh spam
        float attackSpeed = weapon.GetFinalAttackSpeed();
        float comboCooldown = 0.25f / (attackSpeed > 0 ? attackSpeed : 1f);
        float recoveryCooldown = 0.65f / (attackSpeed > 0 ? attackSpeed : 1f);

        float nextCooldown = (_comboIndex == 2) ? recoveryCooldown : comboCooldown;
        weapon.SetFireTimer(nextCooldown);

        // Tăng combo step và lưu thời gian chém
        _comboIndex = (_comboIndex + 1) % 3;
        _lastAttackTime = currentTime;


    }
}

// Lớp phụ trợ để tự động trả hiệu ứng visual chém về Pool
public class SimpleDeactivator : MonoBehaviour
{
    public float delay = 0.3f;
    private float timer;

    void OnEnable()
    {
        timer = delay;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
