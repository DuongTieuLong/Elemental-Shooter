using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;


    public void OnEnable()
    {
        GameEvents.OnBossHealthChanged += UpdateHealthUI;
    }


    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        Debug.Log("Update health boss ui");
        // Tránh lỗi chia cho 0
        if (maxHealth > 0)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    // Luôn nhớ Unsubscribe khi UI bị tắt hoặc hủy để tránh lỗi NullReferenceException
    private void OnDisable()
    {
        GameEvents.OnBossHealthChanged -= UpdateHealthUI;
    }
}