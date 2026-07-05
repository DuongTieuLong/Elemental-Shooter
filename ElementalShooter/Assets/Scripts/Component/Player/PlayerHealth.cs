using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private StatHandler stats;

    private float currentHealth;

    public float MaxHealth => stats.GetValue(StatType.MaxHealth);

    private void Start()
    {
        stats = GetComponent<StatHandler>();
        // Khởi tạo máu hiện tại bằng Max Health lúc đầu
        currentHealth = MaxHealth;

        // Đăng ký sự kiện: Nếu Max Health thay đổi (do nâng cấp), 
        // chúng ta có thể hồi thêm máu hoặc cập nhật UI
        stats.GetStat(StatType.MaxHealth).OnValueChanged += HandleMaxHealthChanged;

        NotifyUI();
    }


    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHealth -= damageInfo.Amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        GameEvents.RaisePlayerHit();
        NotifyUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void HandleMaxHealthChanged(float oldMaxHealth,float newMaxHealth)
    {
        // Ví dụ: Khi nâng cấp Max Health, hồi máu thêm đúng bằng lượng được tăng
        // Hoặc đơn giản là báo cho UI biết Max Health mới

        currentHealth += (newMaxHealth - oldMaxHealth); // Hồi máu thêm phần tăng
        Debug.Log($"Max Health updated to: {newMaxHealth}");
        NotifyUI();
    }

    private void NotifyUI()
    {
        Debug.Log($"Current Health: {currentHealth} / {MaxHealth}");
        GameEvents.RaisePlayerHealthChanged(currentHealth, MaxHealth);
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        GameEvents.RaisePlayerDeath();

        // Vô hiệu hóa điều khiển di chuyển và súng để người chơi không thể bắn hay di chuyển sau khi chết
        if (TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = false;
        if (TryGetComponent<Weapon>(out var weapon))
            weapon.enabled = false;
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            var maxHealthStat = stats.GetStat(StatType.MaxHealth);
            if (maxHealthStat != null)
            {
                maxHealthStat.OnValueChanged -= HandleMaxHealthChanged;
            }
        }
    }

}