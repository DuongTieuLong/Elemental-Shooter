using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour, IPoolable, IStatProvider
{
    public EnemyData enemyData;
    public EnemyHealth enemyHealth;

    public Rigidbody2D Rb { get; private set; }
    public Collider2D col;
    private GameObject myPrefab;

    public Stat moveSpeed;
    public Stat stopDistance;
    public Stat attackDamage;
    public Stat attackCooldown;

    // Các cờ và modifier quản lý Aura Buff từ Tinh Anh
    public bool IsBuffedBySpeed { get; set; }
    public bool IsBuffedByDamage { get; set; }
    private int _speedBuffModId = -1;
    private int _damageBuffModId = -1;

    // Cờ khóa di chuyển cho các kỹ năng sạc chiêu
    public bool isMovementLocked = false;

    public Action<GameObject, GameObject> OnDeathRequestPool;
    public Action OnAttackEvent;
    public Action OnAttackExecuteEvent;
    public Action OnSkillCastEvent;

    // Cache tất cả các component hành vi độc lập
    private IEnemyComponent[] _components;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if( col == null ) col = GetComponentInChildren<Collider2D>();

        // Thiết lập tối ưu hóa Rigidbody2D vật lý
        Rb.bodyType = RigidbodyType2D.Dynamic;
        Rb.gravityScale = 0f;
        Rb.angularDamping = 0f;
        Rb.linearDamping = 0f;
        Rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        moveSpeed = new Stat { BaseValue = enemyData != null ? enemyData.speed : 2f };
        stopDistance = new Stat { BaseValue = enemyData != null ? enemyData.stopDistance : 0f };
        attackDamage = new Stat { BaseValue = 10f }; // Sẽ được nạp lại bởi component cụ thể nếu có
        attackCooldown = new Stat { BaseValue = 1.5f };

        // Thu thập toàn bộ các component hành vi
        _components = GetComponents<IEnemyComponent>();
        //Debug.Log("Component" + _components.Length);


        Initialize();
    }

    private void OnEnable()
    {
        enemyHealth.onDeath += HandleDeath;
    }

    private void OnDisable()
    {
        enemyHealth.onDeath -= HandleDeath;
    }

    public void Initialize()
    {
        myPrefab = enemyData.prefab;
        moveSpeed.BaseValue = enemyData.speed;
        stopDistance.BaseValue = enemyData.stopDistance;

        // Gọi khởi tạo cho tất cả component hành vi
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i].Initialize(this);
        }
    }

    public void OnSpawn()
    {
        if (Rb != null)
        {
            Rb.simulated = true;
        }

        if (col != null)
        {
            col.enabled = true;
        }

        // 1. Đăng ký với EnemyManager
        EnemyManager.Instance.RegisterEnemy(this);

        // 2. Lấy hệ số độ khó hiện tại
        float multiplier = EnemyManager.Instance.CurrentDifficultyMultiplier;

        // 3. Tính toán máu thực tế = Máu gốc * Độ khó
        float finalMaxHealth = enemyData.health * multiplier;
        enemyHealth.OnSpawn();
        enemyHealth.Initialize(finalMaxHealth);

        // 4. Kích hoạt các logic phụ trợ
        GetComponent<StatusEffectController>().OnSpawn();
        GetComponent<StatusReceiver>().OnSpawn();


        IsBuffedBySpeed = false;
        IsBuffedByDamage = false;
        _speedBuffModId = -1;
        _damageBuffModId = -1;
        isMovementLocked = false;

        // 3. Kích hoạt toàn bộ component hành vi
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i].OnSpawnComponent();
        }
    }

    public void OnDespawn()
    {
        // 1. Hủy đăng ký với EnemyManager
        EnemyManager.Instance.UnregisterEnemy(this);

        // 2. Thu dọn logic phụ trợ
        GetComponent<StatusEffectController>().OnDespawn();
        GetComponent<StatusReceiver>().OnDespawn();
        enemyHealth.OnDespawn();

        // Dọn sạch các modifier chỉ số
        moveSpeed.ClearAllModifiers();
        stopDistance.ClearAllModifiers();
        attackDamage.ClearAllModifiers();
        attackCooldown.ClearAllModifiers();

        Rb.linearVelocity = Vector2.zero;

        // 3. Thu dọn toàn bộ component hành vi
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i].OnDespawnComponent();
        }
    }

    // Hàm Update tập trung duy nhất được quản lý bởi EnemyManager
    public void DoUpdate(Vector3 playerPos, float deltaTime)
    {
        if (enemyHealth != null && enemyHealth.isDead) return;

        // Cập nhật trạng thái Aura Buff
        UpdateBuffModifiers();

        // Chạy Update cho toàn bộ component hành vi (di chuyển, tấn công, kỹ năng, hào quang...)
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i].OnUpdateComponent(deltaTime);
        }
    }

    private void UpdateBuffModifiers()
    {
        // Tốc độ
        if (IsBuffedBySpeed && _speedBuffModId == -1)
        {
            // Buff 35% tốc chạy
            _speedBuffModId = moveSpeed.AddModifier(0.35f, ModifierType.Percent);
        }
        else if (!IsBuffedBySpeed && _speedBuffModId != -1)
        {
            moveSpeed.RemoveModifier(_speedBuffModId);
            _speedBuffModId = -1;
        }

        // Sát thương
        if (IsBuffedByDamage && _damageBuffModId == -1)
        {
            // Buff 25% sát thương
            _damageBuffModId = attackDamage.AddModifier(0.25f, ModifierType.Percent);
        }
        else if (!IsBuffedByDamage && _damageBuffModId != -1)
        {
            attackDamage.RemoveModifier(_damageBuffModId);
            _damageBuffModId = -1;
        }

        // Reset cờ buff cho frame tiếp theo
        IsBuffedBySpeed = false;
        IsBuffedByDamage = false;
    }

    private void HandleDeath()
    {
        if (Rb != null)
        {
            Rb.linearVelocity = Vector2.zero;
            Rb.simulated = false;
        }
        if (col != null)
        {
            col.enabled = false;
        }
    }

    public void CompleteDeath()
    {
        OnDeathRequestPool?.Invoke(enemyData.prefab, gameObject);
    }

    public Stat GetStat(StatType type)
    {
        return type switch
        {
            StatType.MoveSpeed => moveSpeed,
            StatType.Damage => attackDamage,
            StatType.AttackSpeed => attackCooldown,
            _ => null
        };
    }
}