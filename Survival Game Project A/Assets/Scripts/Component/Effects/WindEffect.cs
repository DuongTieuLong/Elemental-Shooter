using System;
using UnityEngine;

public class WindEffect : IActiveEffect
{
    private float duration;
    private float maxDuration;
    private float knockbackForce;
    private GameObject targetObj;
    private GameObject vfxPrefab;
    private GameObject vfxInstance;
    private Action onExpireCallback;

    public bool IsExpired => duration <= 0;
    public float DurationRemaining => duration;
    public float MaxDuration => maxDuration;

    public WindEffect(GameObject target, float duration, float force, GameObject vfxPrefab, Action onExpire = null)
    {
        this.targetObj = target;
        this.duration = duration;
        this.maxDuration = duration;
        this.knockbackForce = force;
        this.vfxPrefab = vfxPrefab;
        this.onExpireCallback = onExpire;
    }

    public void OnApply()
    {
        if (vfxPrefab != null && targetObj != null && targetObj.activeInHierarchy)
        {
            vfxInstance = GameObject.Instantiate(vfxPrefab, targetObj.transform);
        }
    }

    public void Tick(float dt)
    {
        duration -= dt;

        if (targetObj == null) return;

        // Tìm vị trí người chơi để đẩy quái lùi ra xa
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pushDir = (targetObj.transform.position - player.transform.position).normalized;
            pushDir.z = 0; // Đảm bảo chuyển động phẳng 2D

            // Lực đẩy lùi giảm dần theo thời gian (Decaying Force) tạo cảm giác quán tính mượt mà
            float normalizedTime = duration / maxDuration;
            float currentForce = knockbackForce * normalizedTime;

            targetObj.transform.position += pushDir * (currentForce * dt);
        }
    }

    public void Refresh(IActiveEffect incoming)
    {
        if (incoming is WindEffect w)
        {
            duration = Mathf.Max(duration, w.duration);
            maxDuration = Mathf.Max(maxDuration, w.maxDuration);
            knockbackForce = Mathf.Max(knockbackForce, w.knockbackForce);
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
}
