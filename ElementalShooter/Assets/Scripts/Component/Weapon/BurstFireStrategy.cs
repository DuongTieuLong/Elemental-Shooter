using System.Collections;
using UnityEngine;

public class BurstFireStrategy : IFireStrategy
{
    private RangedStrategyData _rangedData;

    public void Initialize(IAttackStrategyData data)
    {
        _rangedData = data as RangedStrategyData;
        if (_rangedData == null)
        {
            Debug.LogError("BurstFireStrategy requires RangedStrategyData, but received an invalid type.");
        }
    }

    public void ExecuteFire(Weapon weapon, Transform muzzle, StatHandler stats, WeaponAim weaponAim, LayerMask enemyLayer)
    {
        if (_rangedData == null) return;

        float weaponDame = weapon.GetFinalDamage();
        float playerDame = stats.GetValue(StatType.Damage);
        float finalDamage = weaponDame + playerDame;
        int bulletCount = weapon.GetFinalProjectileCount();

        BulletConfigData activeConfig = _rangedData.bulletConfig;

        weapon.StartCoroutine(FireBurstRoutine(weapon, muzzle, stats, activeConfig, weaponAim, enemyLayer, finalDamage, bulletCount));
    }

    private IEnumerator FireBurstRoutine(Weapon weapon, Transform muzzle, StatHandler stats, BulletConfigData bulletConfigData,
                                            WeaponAim weaponAim, LayerMask enemyLayer, float finalDamage, int bulletCount)
    {
        float delayBetweenShots = 0.08f;
        WaitForSeconds wait = new WaitForSeconds(delayBetweenShots);

        // Giảm lực rung cho mỗi viên đạn lẻ trong loạt Burst để tránh chóng mặt
        float baseShake = weapon.data.cameraShakeIntensity * 0.5f;

        for (int i = 0; i < bulletCount; i++)
        {
            // CHỐT AN TOÀN: Ngắt ngay lập tức nếu súng bị cất đi hoặc bị hủy
            if (weapon == null || !weapon.gameObject.activeInHierarchy) break;

            if (CameraShakeManager.Instance != null)
            {
                float finalShake = baseShake * weapon.CurrentRecoilMultiplier;
                CameraShakeManager.Instance.ShakeCamera(finalShake);
            }

            Vector2 direction = weaponAim.GetAimDirectionWithSpread(weapon.GetCurrentSpread());

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, muzzle.position, 0.65f, 0.15f);
            }

            WeaponFireHelper.SpawnElementBullet(muzzle, stats, bulletConfigData, direction, enemyLayer, finalDamage);
            GameEvents.OnBulletFired?.Invoke(direction);

            yield return wait;
        }
    }
}



