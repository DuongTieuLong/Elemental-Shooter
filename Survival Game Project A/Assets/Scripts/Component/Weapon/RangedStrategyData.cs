using System;
using UnityEngine;

[Serializable]
public class RangedStrategyData : IAttackStrategyData
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public int baseProjectileCount = 1;
    public float bulletSpread = 5f;
    public BulletConfigData bulletConfig;

    [Header("Recoil Settings")]
    public float recoilForce = 0.2f;
    public float recoilRecovery = 15f;
    
    [Header("Audio")]
    public AudioClip shootSFX;
}
