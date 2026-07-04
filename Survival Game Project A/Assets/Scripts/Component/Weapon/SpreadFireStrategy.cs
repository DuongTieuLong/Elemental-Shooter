using UnityEngine;

public class SpreadFireStrategy : IFireStrategy
{
    private RangedStrategyData _rangedData;

    public void Initialize(IAttackStrategyData data)
    {
        _rangedData = data as RangedStrategyData;
        if (_rangedData == null)
        {
            Debug.LogError("SpreadFireStrategy requires RangedStrategyData, but received an invalid type.");
        }
    }

    public void ExecuteFire(Weapon weapon, Transform muzzle, StatHandler stats, WeaponAim weaponAim, LayerMask enemyLayer)
    {
        if (_rangedData == null) return;

        // 1. Rung màn hình DUY NHẤT 1 LẦN cho cả chùm đạn
        if (CameraShakeManager.Instance != null)
        {
            float finalShake = weapon.data.cameraShakeIntensity * weapon.CurrentRecoilMultiplier;
            CameraShakeManager.Instance.ShakeCamera(finalShake);
        }

        // 2. Phát âm thanh DUY NHẤT 1 LẦN
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, muzzle.position, 0.8f);
        }

        // 3. Logic sinh đạn tỏa
        BulletConfigData activeConfig = _rangedData.bulletConfig;
        int bulletCount = weapon.GetFinalProjectileCount();
        float angleStep = 10f;

        Vector2 baseDirection = weaponAim.GetAimDirectionWithSpread(weapon.GetCurrentSpread());
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float finalDamage = weapon.GetFinalDamage() + stats.GetValue(StatType.Damage);

        GameEvents.OnBulletFired?.Invoke(baseDirection);

        for (int i = 0; i < bulletCount; i++)
        {
            float offsetAngle = (i - (bulletCount - 1) / 2f) * angleStep;
            float currentAngle = baseAngle + offsetAngle;

            Quaternion bulletRotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 spreadDirection = bulletRotation * Vector2.right;

            WeaponFireHelper.SpawnElementBullet(muzzle, stats, activeConfig, spreadDirection, enemyLayer, finalDamage);
     
        }
    }
}

