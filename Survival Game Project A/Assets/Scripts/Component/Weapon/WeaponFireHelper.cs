using UnityEngine;

public static class WeaponFireHelper
{
    public static void SpawnElementBullet(Transform muzzle, StatHandler stats, BulletConfigData bulletConfigData,
                                          Vector2 direction, LayerMask enemyLayer, float finalDamage)
    {
        var element = stats.GetRandomEffect();
        var visualConfig = bulletConfigData.GetVisualForElement(element);
        GameObject activeBulletPrefab = visualConfig.customBulletPrefab != null ? visualConfig.customBulletPrefab : bulletConfigData.defaultBulletPrefab;
       
        Debug.Log(activeBulletPrefab);

        if (activeBulletPrefab == null) return;

        GameObject bulletObj = PoolManager.Instance.Get(activeBulletPrefab, muzzle.position);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (visualConfig.muzzleVFXPrefab != null)
        {
            PoolManager.Instance.Get(visualConfig.muzzleVFXPrefab, muzzle.position);
        }

        if (bullet != null)
        {
            DamageInfo info = new DamageInfo
            {
                Amount = finalDamage,
                Element = element,
                ElementMultiplier = stats.GetElementModifier(element)
            };
            bullet.Initialized(direction, info, activeBulletPrefab, enemyLayer, visualConfig.hitVFXPrefab);
        }
    }
}