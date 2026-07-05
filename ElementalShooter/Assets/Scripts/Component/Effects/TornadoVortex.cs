using System.Collections.Generic;
using UnityEngine;

public class TornadoVortex : MonoBehaviour
{
    [Header("Settings")]
    public float pullRadius = 4.5f;
    public float pullSpeed = 3.5f;
    public float duration = 3.0f;
    public float damagePerTick = 10f;
    public float tickInterval = 0.4f;
    public LayerMask enemyLayer;
    public ElementType elementType;


    private float lifetimeTimer = 0f;
    private float tickTimer = 0f;

    private void Start()
    {
        lifetimeTimer = 0f;
        tickTimer = 0f;

        // Phát âm thanh tiếng gió lốc nổ nếu có
        if (AudioManager.Instance != null && AudioManager.Instance.explosionClip != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionClip, transform.position, 0.8f, 0.3f);
        }
    }

    private void Update()
    {
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= duration)
        {
            Destroy(gameObject); // Tự hủy sau khi hết thời gian duy trì lốc xoáy
            return;
        }

        tickTimer += Time.deltaTime;
        bool shouldDamage = false;
        if (tickTimer >= tickInterval)
        {
            shouldDamage = true;
            tickTimer = 0f;
        }

        // Hút các quái vật xung quanh về tâm lốc xoáy
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pullRadius, enemyLayer);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player")) continue; // Không hút người chơi

            // 1. Tác dụng lực hút vật lý kéo quái về tâm
            Vector3 pullDir = (transform.position - col.transform.position).normalized;
            float distance = Vector3.Distance(transform.position, col.transform.position);

            // Càng gần tâm lực hút càng mạnh (hoặc giảm dần tùy ý)
            float force = pullSpeed * (1f - (distance / pullRadius));
            col.transform.position += pullDir * (Mathf.Max(0.5f, force) * Time.deltaTime);

            // 2. Gây sát thương theo nhịp (Tick Damage)
            if (shouldDamage)
            {
                if (col.TryGetComponent<EnemyHealth>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(damagePerTick, ElementType.Fire); // Lốc xoáy mang tính chất đốt cháy (Hỏa + Phong)
                }
                else if (col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageInfo(damagePerTick, elementType));
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn phạm vi hút trong Editor để dễ căn chỉnh
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
