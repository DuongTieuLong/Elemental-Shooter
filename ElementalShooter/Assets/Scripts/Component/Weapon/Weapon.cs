using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform muzzle;
    public StatHandler playerStat;
    public WeaponAim weaponAim;

    public SpriteRenderer leftHandRenderer;

    public PlayerMovement playerMovement; // Tham chiếu đến PlayerMovement để kiểm tra trạng thái di chuyển

    public WeaponData data;
    private IFireStrategy _fireStrategy;

    public LayerMask enemyLayer; // Lớp đối tượng địch để kiểm tra va chạm khi bắn

    private float _fireTimer; // Bộ đếm thời gian hồi chiêu

    // Lưu trữ nâng cấp tích lũy riêng của từng loại vũ khí tại runtime
    private System.Collections.Generic.Dictionary<WeaponData, WeaponModifier> _weaponModifiers = new System.Collections.Generic.Dictionary<WeaponData, WeaponModifier>();

    private void Awake()
    {
        if (data != null) EquipWeapon(data);
        playerStat = GetComponent<StatHandler>();
        weaponAim = GetComponent<WeaponAim>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        HandleAutomaticFiring();
    }

    public bool IsPullingTrigger { get; set; }

    private void HandleAutomaticFiring()
    {
        if (data == null || _fireStrategy == null) return;

        // 1. Tính toán Cooldown dựa trên Stat của vũ khí hiện tại
        float attackSpeed = GetFinalAttackSpeed();

        // Tránh chia cho 0
        float fireInterval = attackSpeed > 0 ? 1f / attackSpeed : 1f;

        // 2. Bộ đếm hồi chiêu luôn đếm ngược (để súng luôn sẵn sàng bắn phát đầu tiên ngay lập tức)
        if (_fireTimer > 0)
        {
            _fireTimer -= Time.deltaTime;
        }

        // 3. CHỈ KHI người chơi đang nhấn giữ nút bắn VÀ đã hết Cooldown thì mới bắn
        if (IsPullingTrigger && _fireTimer <= 0)
        {
            PullTrigger();
            if (_fireTimer <= 0)
            {
                _fireTimer = fireInterval; // Reset lại bộ đếm
            }
        }
    }

    public void ApplyWeaponUpgrade(WeaponData targetWeapon, System.Collections.Generic.List<StatChange> changes)
    {
        if (targetWeapon == null || changes == null) return;

        if (!_weaponModifiers.TryGetValue(targetWeapon, out var mod))
        {
            mod = new WeaponModifier();
            _weaponModifiers[targetWeapon] = mod;
        }

        foreach (var change in changes)
        {
            switch (change.type)
            {
                case StatType.Damage:
                    if (change.modType == ModifierType.Flat) mod.damageFlat += change.value;
                    else mod.damagePercent += change.value;
                    break;
                case StatType.AttackSpeed:
                    if (change.modType == ModifierType.Flat) mod.attackSpeedFlat += change.value;
                    else mod.attackSpeedPercent += change.value;
                    break;
                case StatType.ProjectileCount:
                    mod.projectileCountFlat += (int)change.value;
                    break;
            }
        }
        Debug.Log($"Applied weapon upgrade to {targetWeapon.weaponName}. Current modifiers - DamageFlat: {mod.damageFlat}, DamagePercent: {mod.damagePercent}, AttackSpeedFlat: {mod.attackSpeedFlat}, AttackSpeedPercent: {mod.attackSpeedPercent}, ProjectileCountFlat: {mod.projectileCountFlat}");
    }

    public float GetFinalDamage()
    {
        if (data == null) return 0f;

        float baseDamage = data.baseDamage;
        float weaponFlat = 0f;
        float weaponPercent = 0f;

        if (_weaponModifiers.TryGetValue(data, out var mod))
        {
            weaponFlat = mod.damageFlat;
            weaponPercent = mod.damagePercent;
        }

        // Sát thương gốc của vũ khí sau khi áp dụng nâng cấp riêng
        float modifiedBase = (baseDamage + weaponFlat) * (1f + weaponPercent);

        var damageStat = playerStat.GetStat(StatType.Damage);
        float multiplier = damageStat != null && damageStat.BaseValue > 0 ? damageStat.Value / damageStat.BaseValue : 1f;
        float finalDamage = modifiedBase * multiplier;

        return Mathf.Clamp(finalDamage, 0f, data.maxDamage);
    }

    public float GetPreviewDamage(float amount, ModifierType type)
    {
        if (data == null) return 0f;

        float baseDamage = data.baseDamage;
        float weaponFlat = 0f;
        float weaponPercent = 0f;

        if (_weaponModifiers.TryGetValue(data, out var mod))
        {
            weaponFlat = mod.damageFlat;
            weaponPercent = mod.damagePercent;
        }

        if (type == ModifierType.Flat) weaponFlat += amount;
        else weaponPercent += amount;

        float modifiedBase = (baseDamage + weaponFlat) * (1f + weaponPercent);

        var damageStat = playerStat.GetStat(StatType.Damage);
        float multiplier = damageStat != null && damageStat.BaseValue > 0 ? damageStat.Value / damageStat.BaseValue : 1f;
        float finalDamage = modifiedBase * multiplier;

        return Mathf.Clamp(finalDamage, 0f, data.maxDamage);
    }

    public float GetFinalAttackSpeed()
    {
        if (data == null) return 1f;

        float baseAttackSpeed = data.baseAttackSpeed;
        float weaponFlat = 0f;
        float weaponPercent = 0f;

        if (_weaponModifiers.TryGetValue(data, out var mod))
        {
            weaponFlat = mod.attackSpeedFlat;
            weaponPercent = mod.attackSpeedPercent;
        }

        // Tốc bắn gốc sau khi áp dụng nâng cấp riêng
        float modifiedBase = (baseAttackSpeed + weaponFlat) * (1f + weaponPercent);

        var speedStat = playerStat.GetStat(StatType.AttackSpeed);
        float multiplier = speedStat != null && speedStat.BaseValue > 0 ? speedStat.Value / speedStat.BaseValue : 1f;
        float finalAttackSpeed = modifiedBase * multiplier;

        return Mathf.Clamp(finalAttackSpeed, 0.05f, data.maxAttackSpeed);
    }

    public float GetPreviewAttackSpeed(float amount, ModifierType type)
    {
        if (data == null) return 1f;

        float baseAttackSpeed = data.baseAttackSpeed;
        float weaponFlat = 0f;
        float weaponPercent = 0f;

        if (_weaponModifiers.TryGetValue(data, out var mod))
        {
            weaponFlat = mod.attackSpeedFlat;
            weaponPercent = mod.attackSpeedPercent;
        }

        if (type == ModifierType.Flat) weaponFlat += amount;
        else weaponPercent += amount;

        float modifiedBase = (baseAttackSpeed + weaponFlat) * (1f + weaponPercent);

        var speedStat = playerStat.GetStat(StatType.AttackSpeed);
        float multiplier = speedStat != null && speedStat.BaseValue > 0 ? speedStat.Value / speedStat.BaseValue : 1f;
        float finalAttackSpeed = modifiedBase * multiplier;

        return Mathf.Clamp(finalAttackSpeed, 0.05f, data.maxAttackSpeed);
    }

    public int GetFinalProjectileCount()
    {
        if (data == null) return 1;

        int weaponFlat = 0;
        if (_weaponModifiers.TryGetValue(data, out var mod))
        {
            weaponFlat = mod.projectileCountFlat;
        }

        int baseCount = 1;
        if (data.attackData is RangedStrategyData rangedData)
        {
            baseCount = rangedData.baseProjectileCount;
        }

        int finalCount = baseCount + weaponFlat; // Bỏ qua cộng đạn từ player
        return Mathf.Clamp(finalCount, 1, data.maxProjectileCount);
    }

    public int GetPreviewProjectileCount(float amount)
    {
        if (data == null) return 1;

        int weaponFlat = 0;
        if (_weaponModifiers.TryGetValue(data, out var mod))
        {
            weaponFlat = mod.projectileCountFlat;
        }

        weaponFlat += (int)amount;

        int baseCount = 1;
        if (data.attackData is RangedStrategyData rangedData)
        {
            baseCount = rangedData.baseProjectileCount;
        }

        int finalCount = baseCount + weaponFlat; // Bỏ qua cộng đạn từ player
        return Mathf.Clamp(finalCount, 1, data.maxProjectileCount);
    }

    public void EquipWeapon(WeaponData newData)
    {
        data = newData;
        _fireStrategy = FireStrategyFactory.GetStrategy(data.strategyType);
        
        // Dependency Injection
        _fireStrategy?.Initialize(data.attackData);

        muzzle.GetComponentInParent<SpriteRenderer>().sprite = data.weaponIcon;
        // Reset timer khi đổi vũ khí để tránh việc vũ khí mới phải chờ cooldown của vũ khí cũ
        _fireTimer = 0;
        Debug.Log($"Equipped new weapon: {data.weaponName} with strategy {data.strategyType}");
        leftHandRenderer.sortingOrder = data.gripStyle == WeaponGripStyle.TwoHanded ? 4 : 0; // Đặt sorting order dựa trên kiểu cầm
    }

    public float CurrentRecoilMultiplier { get; private set; } = 1f;

    public void PullTrigger()
    {
        if (data == null || _fireStrategy == null) return;

        // 1. Tính toán Recoil Multiplier
        CurrentRecoilMultiplier = 1.0f;

        if (playerMovement != null)
        {
            if (playerMovement.IsSprinting) CurrentRecoilMultiplier = 1.5f;
            else if (!playerMovement.IsMoving) CurrentRecoilMultiplier = 0.5f;
        }

        if (Camera.main != null && UnityEngine.InputSystem.Mouse.current != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            float distanceToMouse = Vector2.Distance(transform.position, mouseWorldPos);
            float range = playerStat != null ? playerStat.GetValue(StatType.Range) : 10f;
            if (distanceToMouse > range)
            {
                CurrentRecoilMultiplier *= 1.3f;
            }
        }

        // Áp dụng giật hình ảnh súng
        if (data.attackData is RangedStrategyData rangedData && weaponAim != null)
        {
            weaponAim.TriggerRecoil(rangedData.recoilForce * CurrentRecoilMultiplier, rangedData.recoilRecovery * CurrentRecoilMultiplier);
        }

        // 2. Thực hiện bắn
        _fireStrategy?.ExecuteFire(this, muzzle, playerStat, weaponAim, enemyLayer);


    }

    public float GetCurrentSpread()
    {
        float spreadMultiplier = 1.0f;
        if (playerMovement != null)
        {
            if (playerMovement.IsSprinting)
            {
                spreadMultiplier = 1.5f;
            }
            else if (!playerMovement.IsMoving)
            {
                spreadMultiplier = 0.5f; // Giảm 50% khi đứng yên
            }
        }

        if (Camera.main != null && UnityEngine.InputSystem.Mouse.current != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            float distanceToMouse = Vector2.Distance(transform.position, mouseWorldPos);
            float range = playerStat != null ? playerStat.GetValue(StatType.Range) : 10f;
            if (distanceToMouse > range)
            {
                spreadMultiplier *= 1.3f; // Tăng thêm 30% khi ngắm quá xa
            }
        }

        float baseSpread = 0f;
        if (data.attackData is RangedStrategyData rangedData)
        {
            baseSpread = rangedData.bulletSpread;
        }

        return baseSpread * spreadMultiplier;
    }

    public void SetFireTimer(float time)
    {
        _fireTimer = time;
    }
}

public class WeaponModifier
{
    public float damageFlat = 0f;
    public float damagePercent = 0f;
    public float attackSpeedFlat = 0f;
    public float attackSpeedPercent = 0f;
    public int projectileCountFlat = 0;
}