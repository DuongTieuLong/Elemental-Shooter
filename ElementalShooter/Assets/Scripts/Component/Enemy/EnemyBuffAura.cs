using UnityEngine;

public class EnemyBuffAura : MonoBehaviour, IEnemyComponent
{
    [Header("Aura Settings")]
    public float auraRadius = 5.5f;
    public bool buffSpeed = true;
    public bool buffDamage = true;
    public bool healingAura = true;
    public float healPerSecond = 8f;

    private int _frameOffset;
    private EnemyController _controller;
    private static readonly Collider2D[] _auraBuffer = new Collider2D[16];
    private static ContactFilter2D _filter = new ContactFilter2D();

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
    }

    public void OnSpawnComponent()
    {
        _frameOffset = Random.Range(0, 60); // Rải quét aura qua 60 frames (1 giây ở 60fps)
    }

    public void OnDespawnComponent()
    {
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if ((Time.frameCount + _frameOffset) % 60 == 0)
        {
            ApplyAuraEffects();
        }
    }

    private void ApplyAuraEffects()
    {
        if (_controller == null) return;

        int numColliders = Physics2D.OverlapCircle(transform.position, auraRadius, _filter, _auraBuffer);
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
                    // Hồi máu tương ứng với khoảng thời gian tích lũy giữa các chu kỳ quét (1 giây)
                    enemy.enemyHealth.Heal(healPerSecond * 1f);
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
