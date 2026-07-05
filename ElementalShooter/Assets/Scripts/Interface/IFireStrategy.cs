using UnityEngine;

public interface IFireStrategy
{
    void Initialize(IAttackStrategyData data);
    void ExecuteFire(Weapon weapon, Transform muzzle, StatHandler stats, WeaponAim weaponAim, LayerMask enemyLayer);
}