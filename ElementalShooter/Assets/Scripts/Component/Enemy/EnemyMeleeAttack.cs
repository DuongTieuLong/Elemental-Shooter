using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour, IEnemyComponent
{
    [Header("Melee Attack Settings")]
    public float attackRange = 1.3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    private float _attackTimer;
    private EnemyController _controller;

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
        
        // Thiết lập giá trị cơ sở cho các Stat của Controller để buff/debuff hoạt động đồng bộ
        _controller.attackDamage.BaseValue = attackDamage;
        _controller.attackCooldown.BaseValue = attackCooldown;
    }

    public void OnSpawnComponent()
    {
        _attackTimer = 0f;
        if (_controller != null)
        {
            _controller.OnAttackExecuteEvent += HandleAttackExecute;
        }
    }

    public void OnDespawnComponent()
    {
        if (_controller != null)
        {
            _controller.OnAttackExecuteEvent -= HandleAttackExecute;
        }
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if (_controller == null) return;

        if (_attackTimer > 0f)
        {
            _attackTimer -= deltaTime;
        }

        Transform playerTransform = EnemyManager.Instance.PlayerTransform;
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange && _attackTimer <= 0f)
        {
            _controller.OnAttackEvent?.Invoke();
            _attackTimer = _controller.attackCooldown.Value;
        }
    }

    private void HandleAttackExecute()
    {
        Transform playerTransform = EnemyManager.Instance.PlayerTransform;
        if (playerTransform == null) return;

        // Kiểm tra xem người chơi còn ở trong tầm đánh không (thêm 0.5f buffer do người chơi di chuyển trong lúc vung đòn)
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange + 0.5f)
        {
            Debug.Log("Attacking"); 

            if (playerTransform.TryGetComponent<IDamageable>(out var damageable))
            {

                Debug.Log("Get player heath" + damageable);
                damageable.TakeDamage(new DamageInfo
                {
                    Amount = _controller.attackDamage.Value,
                    Element = ElementType.None
                });

                // Phát âm thanh va chạm trúng đòn
                if (AudioManager.Instance != null && AudioManager.Instance.hitClip != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.hitClip, playerTransform.position, 0.7f);
                }
            }
        }
    }
}
