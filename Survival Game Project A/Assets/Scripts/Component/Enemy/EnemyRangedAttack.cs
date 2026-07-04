using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour, IEnemyComponent
{
    [Header("Ranged Attack Settings")]
    public float attackRange = 5.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.8f;

    [Header("Projectile Config")]
    public GameObject projectilePrefab;
    public Transform spawnPos;
    public float projectileSpeed = 8f;

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
        if (projectilePrefab == null) return;

        Transform playerTransform = EnemyManager.Instance.PlayerTransform;
        if (playerTransform == null) return;

        GameObject projObj = PoolManager.Instance.Get(projectilePrefab, spawnPos.position);
        Bullet bullet = projObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)spawnPos.position).normalized;
            DamageInfo info = new DamageInfo
            {
                Amount = _controller.attackDamage.Value,
                Element = ElementType.None,
                ElementMultiplier = 1f
            };

            // Mục tiêu là người chơi (Layer Default - bao gồm cả tường để cản đạn)
            LayerMask targetLayer = LayerMask.GetMask("Player");
            bullet.Initialized(dir, info, projectilePrefab, targetLayer, null, 20);

            // Chơi hiệu ứng âm thanh bắn
            if (AudioManager.Instance != null && AudioManager.Instance.shootClip != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, spawnPos.position, 0.6f);
            }
        }
    }
}
