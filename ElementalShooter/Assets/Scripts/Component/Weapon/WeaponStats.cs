using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weaponstats : MonoBehaviour
{
    private StatHandler playerStat;
    private Weapon weapon;

    private Dictionary<WeaponData, WeaponModifier> _weaponModifiers = new Dictionary<WeaponData, WeaponModifier>();

    private void Awake()
    {
        playerStat = GetComponent<StatHandler>();
        weapon = GetComponent<Weapon>();
    }

    public void ApplyWeaponUpgrade(WeaponData targetWeapon, List<StatChange> changes)
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

    #region --- CORE FORMULAS (CÔNG THỨC CHUNG - SỬA CÔNG THỨC TẠI ĐÂY) ---

    /// <summary>
    /// Công thức tính Stat dạng Float chung (Damage, AttackSpeed)
    /// </summary>
    private float CalculateStat(float baseValue, float flatMod, float percentMod, StatType statType, float minLimit, float maxLimit, float bonusFlat = 0f, ModifierType bonusType = ModifierType.Flat)
    {
        // Nếu có preview bonus truyền vào thì cộng trước
        if (bonusType == ModifierType.Flat) flatMod += bonusFlat;
        else percentMod += bonusFlat;

        // 1. Công thức tính chỉ số gốc sau khi nâng cấp vũ khí
        float modifiedBase = (baseValue + flatMod) * (1f + percentMod);

        // 2. Lấy hệ số nhân từ Player Stat
        var playerStatObj = playerStat != null ? playerStat.GetStat(statType) : null;
        float multiplier = (playerStatObj != null && playerStatObj.BaseValue > 0) 
            ? playerStatObj.Value / playerStatObj.BaseValue 
            : 1f;

        // 3. Công thức cuối cùng
        float finalValue = modifiedBase * multiplier;

        return Mathf.Clamp(finalValue, minLimit, maxLimit);
    }

    /// <summary>
    /// Công thức tính số lượng đạn (Int)
    /// </summary>
    private int CalculateProjectileCount(int bonusFlat = 0)
    {
        if (weapon.CurrentData == null) return 1;

        int weaponFlat = 0;
        if (_weaponModifiers.TryGetValue(weapon.CurrentData, out var mod))
        {
            weaponFlat = mod.projectileCountFlat;
        }

        int baseCount = (weapon.CurrentData.attackData is RangedStrategyData rangedData) ? rangedData.baseProjectileCount : 1;
        int finalCount = baseCount + weaponFlat + bonusFlat;

        return Mathf.Clamp(finalCount, 1, weapon.CurrentData.maxProjectileCount);
    }

    #endregion

    #region --- PUBLIC GETTERS & PREVIEWS ---

    // DAMAGE
    public float GetFinalDamage() 
        => CalculateDamageInternal(0f, ModifierType.Flat);

    public float GetPreviewDamage(float amount, ModifierType type) 
        => CalculateDamageInternal(amount, type);

    private float CalculateDamageInternal(float bonusAmount, ModifierType bonusType)
    {
        if (weapon.CurrentData == null) return 0f;

        _weaponModifiers.TryGetValue(weapon.CurrentData, out var mod);
        float flat = mod != null ? mod.damageFlat : 0f;
        float percent = mod != null ? mod.damagePercent : 0f;

        return CalculateStat(weapon.CurrentData.baseDamage, flat, percent, StatType.Damage, 0f, weapon.CurrentData.maxDamage, bonusAmount, bonusType);
    }


    // ATTACK SPEED
    public float GetFinalAttackSpeed() 
        => CalculateAttackSpeedInternal(0f, ModifierType.Flat);

    public float GetPreviewAttackSpeed(float amount, ModifierType type) 
        => CalculateAttackSpeedInternal(amount, type);

    private float CalculateAttackSpeedInternal(float bonusAmount, ModifierType bonusType)
    {
        if (weapon.CurrentData == null) return 1f;

        _weaponModifiers.TryGetValue(weapon.CurrentData, out var mod);
        float flat = mod != null ? mod.attackSpeedFlat : 0f;
        float percent = mod != null ? mod.attackSpeedPercent : 0f;

        return CalculateStat(weapon.CurrentData.baseAttackSpeed, flat, percent, StatType.AttackSpeed, 0.05f, weapon.CurrentData.maxAttackSpeed, bonusAmount, bonusType);
    }


    // PROJECTILE COUNT
    public int GetFinalProjectileCount() 
        => CalculateProjectileCount(0);

    public int GetPreviewProjectileCount(float amount) 
        => CalculateProjectileCount((int)amount);

    #endregion
}

public class WeaponModifier
{
    public float damageFlat = 0f;
    public float damagePercent = 0f;
    public float attackSpeedFlat = 0f;
    public float attackSpeedPercent = 0f;
    public int projectileCountFlat = 0;
}