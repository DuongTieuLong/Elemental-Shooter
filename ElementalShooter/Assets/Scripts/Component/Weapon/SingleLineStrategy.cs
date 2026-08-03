using UnityEngine;

public class SingleLineStrategy : IFireStrategy
{
    private RangedStrategyData _rangedData;

    public void Initialize(IAttackStrategyData data)
    {
        _rangedData = data as RangedStrategyData;
        if (_rangedData == null)
        {
            Debug.LogError("SingleLineStrategy requires RangedStrategyData, but received an invalid type.");
        }
    }

    public void ExecuteFire(Weapon weapon, Transform muzzle, StatHandler stats, WeaponAim weaponAim, LayerMask enemyLayer)
    {
        if (_rangedData == null) return;
        Debug.Log("SingleLineStrategy: Executing fire.");

        //Recoil
        weapon.recoilController.ApplyRecoil(weapon.spreadCalculator.GetCurrentRecoilMultiplier());

        // 1. Rung màn hình
        if (CameraShakeManager.Instance != null)
        {
            float finalShake = weapon.CurrentData.cameraShakeIntensity * weapon.spreadCalculator.GetCurrentRecoilMultiplier();
            CameraShakeManager.Instance.ShakeCamera(finalShake);
        }

        // 2. Phát âm thanh
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, muzzle.position, 0.8f);
        }

        // 3. Tính toán và sinh đạn
        float finalDamage = weapon.weaponStats.GetFinalDamage() + stats.GetValue(StatType.Damage);
        Vector2 bulletDirection = weaponAim.GetAimDirectionWithSpread(weapon.spreadCalculator.GetCurrentSpread());
        BulletConfigData activeConfig = _rangedData.bulletConfig;

        WeaponFireHelper.SpawnElementBullet(muzzle, stats, activeConfig, bulletDirection, enemyLayer, finalDamage);
        GameEvents.OnBulletFired?.Invoke(bulletDirection);
    }
}
