using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private float comboTimeout = 3f; // Thời gian giữ combo
    
    private int currentCombo = 0;
    private float comboTimer = 0f;

    // Biến cờ để throttle UI update
    private bool needsUIUpdate = false;

    private void OnEnable()
    {
        GameEvents.OnDamageDealt += HandleDamageDealt;
    }

    private void OnDisable()
    {
        GameEvents.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleDamageDealt(float damage, Vector3 pos, ElementType element)
    {
        currentCombo++;
        comboTimer = comboTimeout;
        needsUIUpdate = true;
    }

    private void Update()
    {
        // Giảm timer combo nếu đang có combo
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                currentCombo = 0;
                needsUIUpdate = true; // Cập nhật UI reset combo
            }
        }
    }

    private void LateUpdate()
    {
        // Throttle UI Updates: Chỉ gửi Event cập nhật UI đúng 1 lần vào cuối frame
        // Giúp tránh tình trạng Spam Event khi hàng loạt quái bị trúng đạn cùng lúc
        if (needsUIUpdate)
        {
            needsUIUpdate = false;
            GameEvents.RaiseComboUpdated(currentCombo);
        }
    }
}
