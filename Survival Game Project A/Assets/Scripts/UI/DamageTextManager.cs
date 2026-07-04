using System.Collections.Generic;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    // Tạo một struct để hiển thị cấu hình trực quan trên Editor
    [System.Serializable]
    public struct ElementColorConfig
    {
        public ElementType elementType;
        public Color textColor;
    }

    [Header("Settings")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float randomOffsetRange = 0.3f;

    [Header("Element Colors Configuration")]
    [Tooltip("Thêm nguyên tố và màu sắc tương ứng trực tiếp tại đây trên Editor")]
    [SerializeField] private List<ElementColorConfig> elementColors;
    
    [SerializeField] private Color defaultColor = Color.white;

    // Dùng Dictionary nội bộ để tối ưu tốc độ tìm kiếm khi chạy game (O(1))
    private Dictionary<ElementType, Color> elementColorMap;

    private void Awake()
    {
        InitializeColorMap();
    }

    private void OnEnable()
    {
        GameEvents.OnDamageDealt += SpawnDamageText;
    }

    private void OnDisable()
    {
        GameEvents.OnDamageDealt -= SpawnDamageText;
    }

    private void InitializeColorMap()
    {
        elementColorMap = new Dictionary<ElementType, Color>();

        if (elementColors == null) return;

        foreach (var config in elementColors)
        {
            // Kiểm tra trùng lặp để tránh lỗi Crash/Log lỗi của Dictionary
            if (!elementColorMap.ContainsKey(config.elementType))
            {
                elementColorMap.Add(config.elementType, config.textColor);
            }
            else
            {
                Debug.LogWarning($"Trùng cấu hình màu cho nguyên tố: {config.elementType} trong DamageTextManager!");
            }
        }
    }

    private void SpawnDamageText(float damage, Vector3 targetPosition, ElementType element)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("DamageTextPrefab is not assigned in DamageTextManager!");
            return;
        }

        Vector3 randomPos = targetPosition + spawnOffset + new Vector3(
            Random.Range(-randomOffsetRange, randomOffsetRange),
            Random.Range(-randomOffsetRange, randomOffsetRange),
            0f
        );

        GameObject textObj = PoolManager.Instance.Get(damageTextPrefab, randomPos);
        if (textObj.TryGetComponent<DamageText>(out var damageText))
        {
            Color textColor = GetColorForElement(element);
            damageText.Setup(damage, textColor, damageTextPrefab);
        }
    }

    private Color GetColorForElement(ElementType element)
    {
        if (elementColorMap != null && elementColorMap.TryGetValue(element, out Color color))
        {
            return color;
        }
        return defaultColor; // Trả về màu mặc định nếu quên chưa cấu hình trên Editor
    }
}