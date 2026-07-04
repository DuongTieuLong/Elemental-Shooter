using UnityEngine;

public class EnemyBuffAura : MonoBehaviour, IEnemyComponent
{
    [Header("Aura Settings")]
    public float auraRadius = 5.5f;
    public bool buffSpeed = true;
    public bool buffDamage = true;
    public bool healingAura = true;
    public float healPerSecond = 8f;

    private float _scanTimer;
    private const float ScanInterval = 1f; // Quét định kỳ để tối ưu hóa hiệu năng
    private EnemyController _controller;
    private static readonly Collider2D[] _auraBuffer = new Collider2D[16];

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
    }

    public void OnSpawnComponent()
    {
        _scanTimer = 0f;
    }

    public void OnDespawnComponent()
    {
    }

    public void OnUpdateComponent(float deltaTime)
    {
        _scanTimer -= deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = ScanInterval;
            ApplyAuraEffects();
        }
    }

    private void ApplyAuraEffects()
    {
        if (_controller == null) return;

        ContactFilter2D contactFilter = new ContactFilter2D();
        int numColliders = Physics2D.OverlapCircle(transform.position, auraRadius, contactFilter, _auraBuffer);
        for (int i = 0; i < numColliders; i++)
        {
            Collider2D other = _auraBuffer[i];
            if (other == null || other.gameObject == gameObject) continue;

            if (other.CompareTag("Enemy") && other.TryGetComponent<EnemyController>(out var enemy))
            {
                if (buffSpeed)
                {
                    enemy.IsBuffedBySpeed = true;
                }
                
                if (buffDamage)
                {
                    enemy.IsBuffedByDamage = true;
                }

                if (healingAura && enemy.enemyHealth != null)
                {
                    // Hồi máu tương ứng với khoảng thời gian tích lũy giữa các chu kỳ quét
                    enemy.enemyHealth.Heal(healPerSecond * ScanInterval);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
