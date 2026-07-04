using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealthUI : MonoBehaviour
{
    public Slider healthSlider;
    public Gradient healthGradient; // Biến này cho phép bạn chọn màu trên thanh màu
    public Image fillImage;
    public TextMeshProUGUI healthText;

    public void Start()
    {
        healthSlider.maxValue = 1;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChanged += UpdateHealthBar;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthText.text = $"{currentHealth} / {maxHealth}";

        // 1. Tính toán giá trị 0-1
        float healthNormalized = currentHealth / maxHealth;

        // 2. Cập nhật giá trị Slider
        healthSlider.value = healthNormalized;

        // 3. Thay đổi màu sắc dựa trên giá trị 0-1
        // Evaluate sẽ lấy ra màu tương ứng tại vị trí đó trên thanh Gradient
        if(healthNormalized > 0 || healthNormalized <1)
            fillImage.color = healthGradient.Evaluate(healthNormalized);
        else 
            Debug.LogWarning("Health value is out of range (0-1): " + healthNormalized);
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= UpdateHealthBar;
    }

}
