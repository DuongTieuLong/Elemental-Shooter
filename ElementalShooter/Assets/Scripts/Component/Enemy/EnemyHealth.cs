using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class EnemyHealth : MonoBehaviour, IDamageable, IPoolable
{
    private float currentHealth;
    private float maxHealth;
    public bool isDead = false;
    
    private EnemyHealthBar healthBar; // Sẽ được cấp phát động từ UIManager

    public event Action<float, float> OnHealthChanged;

    public Action onDeath; // Sự kiện để thông báo khi Enemy chết
    public Action onHit; // Sự kiện khi nhận sát thương

    private EnemyController owner; // Tham chiếu ngược lại để lấy Data

    private void Awake()
    {
        owner = GetComponent<EnemyController>();
    }

    // Tạo một hàm để thiết lập máu ban đầu
    public void Initialize(float healthValue)
    {
        isDead = false;
        maxHealth = healthValue;
        currentHealth = maxHealth;


        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth, false);
        }
    }

    // Hàm để nhận sát thương
    public void TakeDamage(DamageInfo damage)
    {
        TakeDamage(damage.Amount, damage.Element);

        Debug.Log("HIt2");
    }

    public void TakeDamage(float damage, ElementType element)
    {
        if (isDead) return;

        onHit?.Invoke();

        currentHealth -= damage;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        // Phát sự kiện để hiển thị số sát thương nhảy múa
        GameEvents.RaiseDamageDealt(damage, transform.position, element);

        if (currentHealth <= 0)
        {
            isDead = true;
            HandleDeath();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth, false);
        }
    }

    private void HandleDeath()
    {
        int expReward = 10;
        if (owner != null && owner.enemyData != null)
        {
            expReward = owner.enemyData.expReward;
        }
        GameEvents.RaiseEnemyKilled(transform.position, expReward);
        
        // Trả lại thanh máu về pool trước khi Enemy bị de-active
        if (healthBar != null && UIManager.Instance != null)
        {

            UIManager.Instance.DespawnHealthBar(healthBar);
            healthBar = null;
        }

        onDeath?.Invoke(); // Thông báo cho các script nội bộ khác biết rằng Enemy đã chết
    }

    public void UpdateMaxHealth(float newMaxHealth)
    {
        if (isDead) return;
        maxHealth = newMaxHealth;
    }

    public void OnSpawn()
    {
        // Yêu cầu UIManager cấp phát một thanh máu Screen Space mới bám theo mình
        if (UIManager.Instance != null)
        {
            healthBar = UIManager.Instance.SpawnHealthBar(transform);
        }
    }

    public void OnDespawn()
    {
        isDead = false;
        
        // Thu hồi thanh máu nếu vẫn còn sở hữu (trong trường hợp quái bị despawn cưỡng bức không qua chết)
        if (healthBar != null && UIManager.Instance != null)
        {
            UIManager.Instance.DespawnHealthBar(healthBar);
            healthBar = null;
        }
    }
}
