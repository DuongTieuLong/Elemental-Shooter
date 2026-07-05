using System;
using System.Collections;
using UnityEngine;

public class BurnEffect : IActiveEffect
{
    private float duration;
    private float maxDuration; // Thêm biến lưu thời gian tối đa
    private float damage;
    private float tickInterval;
    private float tickTimer;
    private IDamageable target;
    private GameObject targetObj;
    private GameObject vfxPrefab;
    private GameObject vfxInstance;

    private Action onExpireCallback;

    public bool IsExpired => duration <= 0;
    public float DurationRemaining => duration;
    public float MaxDuration => maxDuration;

    private ElementType element;

    // Nhận toàn bộ config từ Data
    public BurnEffect(GameObject targetObj, IDamageable target, float duration, float damage, float tickInterval, ElementType element, GameObject vfxPrefab, Action onExpireCallback = null)
    {
        this.targetObj = targetObj;
        this.target = target;
        this.duration = duration;
        this.maxDuration = duration;
        this.damage = damage;
        this.tickInterval = tickInterval;
        tickTimer = tickInterval;
        this.element = element;
        this.vfxPrefab = vfxPrefab;

        this.onExpireCallback = onExpireCallback;
    }

    public void Tick(float dt)
    {
        duration -= dt;
        tickTimer -= dt;

        if (tickTimer <= 0)
        {
            target.TakeDamage(new DamageInfo
            {
                Amount = damage,
                Element = element
            });

            tickTimer = tickInterval;
        }
    }

    public void Refresh(IActiveEffect incoming)
    {
        if (incoming is BurnEffect b)
        {
            duration = Mathf.Max(duration, b.duration);
            maxDuration = Mathf.Max(maxDuration, b.maxDuration);
            damage = Mathf.Max(damage, b.damage);
        }
    }

    public void OnExpire(GameObject target, bool isForced = false)
    {
        if (vfxInstance != null)
        {
            GameObject.Destroy(vfxInstance);
            vfxInstance = null;
        }

        if (!isForced)
        {
            onExpireCallback?.Invoke();
        }
    }

    public void OnApply()
    {
        if (vfxPrefab != null && targetObj != null && targetObj.activeInHierarchy)
        {
            vfxInstance = GameObject.Instantiate(vfxPrefab, targetObj.transform);
        }
    }
}