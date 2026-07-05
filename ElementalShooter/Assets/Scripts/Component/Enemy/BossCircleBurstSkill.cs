using UnityEngine;

public class BossCircleBurstSkill : MonoBehaviour, IEnemyComponent
{
    [Header("Skill Settings")]
    public float skillCooldown = 6f;
    public float skillDuration = 1.2f;
    public int projectileCount = 12;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    private float _cooldownTimer;
    private bool _isCasting;
    private float _castTimer;

    private EnemyController _controller;
    private SpriteRenderer _spriteRenderer;

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void OnSpawnComponent()
    {
        // 3 giây đệm trước khi thi triển lần đầu để người chơi kịp định hình
        _cooldownTimer = Mathf.Min(3f, skillCooldown);
        _isCasting = false;
        
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white;
        }
    }

    public void OnDespawnComponent()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white;
        }
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if (_controller == null) return;

        if (!_isCasting)
        {
            _cooldownTimer -= deltaTime;
            if (_cooldownTimer <= 0f)
            {
                TriggerSkillWarning();
            }
        }
        else
        {
            UpdateSkillCasting(deltaTime);
        }
    }

    private void TriggerSkillWarning()
    {
        _isCasting = true;
        _castTimer = skillDuration;
        _controller.isMovementLocked = true; // Khóa di chuyển của quái khi đang thi triển chiêu

        // Phát âm thanh cảnh báo nạp chiêu
        if (AudioManager.Instance != null && AudioManager.Instance.shootClip != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, transform.position, 0.5f, 0.2f);
        }
    }

    private void UpdateSkillCasting(float deltaTime)
    {
        _castTimer -= deltaTime;

        // Nhấp nháy đỏ báo hiệu đang sạc năng lượng
        if (_spriteRenderer != null)
        {
            float flash = Mathf.PingPong(Time.time * 12f, 1f);
            _spriteRenderer.color = Color.Lerp(Color.white, Color.red, flash);
        }

        if (_castTimer <= 0f)
        {
            ExecuteCircleBurst();
            
            // Dọn dẹp trạng thái
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
            }
            _controller.isMovementLocked = false;
            _isCasting = false;
            _cooldownTimer = skillCooldown;
        }
    }

    private void ExecuteCircleBurst()
    {
        if (projectilePrefab == null) return;

        float angleStep = 360f / projectileCount;
        Vector3 spawnPos = transform.position;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject projObj = PoolManager.Instance.Get(projectilePrefab, spawnPos);
            Bullet bullet = projObj.GetComponent<Bullet>();

            if (bullet != null)
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = _controller.attackDamage.Value * 1.5f, // Đòn kỹ năng gây sát thương mạnh hơn
                    Element = ElementType.None,
                    ElementMultiplier = 1f
                };

                // Bắn trúng Layer Default (Player và Tường)
                LayerMask targetLayer = LayerMask.GetMask("Player");
                bullet.Initialized(dir, info, projectilePrefab, targetLayer, null);
            }
        }

        // Âm thanh nổ tung
        if (AudioManager.Instance != null && AudioManager.Instance.explosionClip != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionClip, spawnPos, 0.8f);
        }
    }
}
