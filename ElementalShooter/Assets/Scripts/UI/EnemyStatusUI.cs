using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusUI : MonoBehaviour
{
    [System.Serializable]
    public struct StatusUIConfig
    {
        [Tooltip("Tên chính xác của Class hiệu ứng (Ví dụ: BurnEffect, StunEffect)")]
        public string effectClassName;

        [Header("UI References")]
        public GameObject iconContainer;
        public Image radialFillImage;

        [Header("Visual Tint")]
        public Color tintColor;
    }

    [Header("Status Configurations")]
    [SerializeField] private List<StatusUIConfig> statusConfigurations = new List<StatusUIConfig>();
    [SerializeField] private Color normalColor = Color.white;

    private StatusEffectController controller;
    private SpriteRenderer enemySpriteRenderer;
    private Color lastAppliedColor = Color.white;
    private Type[] _cachedEffectTypes;

    private void Awake()
    {
        _cachedEffectTypes = new Type[statusConfigurations.Count];
        // Tự động ẩn toàn bộ các icon khi khởi chạy và cache Type
        for (int i = 0; i < statusConfigurations.Count; i++)
        {
            var config = statusConfigurations[i];
            if (!string.IsNullOrEmpty(config.effectClassName))
            {
                _cachedEffectTypes[i] = Type.GetType(config.effectClassName);
            }
            if (config.iconContainer != null) config.iconContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Gán thực thể kẻ địch mục tiêu để lắng nghe hiệu ứng và lấy SpriteRenderer.
    /// </summary>
    public void SetupTarget(Transform target)
    {
        if (target == null)
        {
            ResetStatusUI();
            return;
        }

        controller = target.GetComponent<StatusEffectController>();
        enemySpriteRenderer = target.GetComponentInChildren<SpriteRenderer>();

        // Ẩn các icon ban đầu khi gán target mới
        ToggleAllContainers(false);
    }

    private void Update()
    {
        if (controller == null || !controller.gameObject.activeInHierarchy)
        {
            ToggleAllContainers(false);
            return;
        }

        Color targetColor = normalColor;
        int activeEffectsCount = 0;
        Color blendedColor = Color.clear;

        // Vòng lặp duyệt qua mọi cấu hình đã cài đặt ngoài Editor
        for (int i = 0; i < statusConfigurations.Count; i++)
        {
            var config = statusConfigurations[i];
            var cachedType = _cachedEffectTypes[i];
            if (cachedType == null) continue;

            // Kiểm tra xem controller của quái có đang dính hiệu ứng này không thông qua Reflection tên class
            if (controller.TryGetEffectDuration(cachedType, out float remain, out float max))
            {
                // 1. Hiển thị và cập nhật thanh Radial Fill UI
                if (config.iconContainer != null && !config.iconContainer.activeSelf)
                    config.iconContainer.SetActive(true);

                if (config.radialFillImage != null && max > 0f)
                {
                    config.radialFillImage.fillAmount = remain / max;
                }

                // 2. Tính toán trộn màu sắc Tint
                if (activeEffectsCount == 0)
                {
                    blendedColor = config.tintColor;
                }
                else
                {
                    blendedColor = Color.Lerp(blendedColor, config.tintColor, 0.5f); // Trộn đều các màu nếu dính nhiều hiệu ứng
                }
                activeEffectsCount++;
            }
            else
            {
                // Ẩn icon nếu hiệu ứng đã hết tác dụng
                if (config.iconContainer != null && config.iconContainer.activeSelf)
                    config.iconContainer.SetActive(false);
            }
        }

        // 3. Cập nhật màu sắc cho SpriteRenderer (Chỉ cập nhật nếu có sự thay đổi màu)
        if (activeEffectsCount > 0)
        {
            targetColor = blendedColor;
        }

        if (enemySpriteRenderer != null && enemySpriteRenderer.gameObject.activeInHierarchy)
        {
            if (lastAppliedColor != targetColor)
            {
                enemySpriteRenderer.color = targetColor;
                lastAppliedColor = targetColor; // Lưu lại trạng thái màu để frame sau check tiếp
            }
        }
    }

    /// <summary>
    /// Khôi phục màu sắc gốc cho quái vật và dọn dẹp các tham chiếu.
    /// </summary>
    public void ResetStatusUI()
    {
        if (enemySpriteRenderer != null)
        {
            enemySpriteRenderer.color = normalColor;
        }

        controller = null;
        enemySpriteRenderer = null;
        lastAppliedColor = normalColor;

        ToggleAllContainers(false);
    }

    private void ToggleAllContainers(bool isActive)
    {
        foreach (var config in statusConfigurations)
        {
            if (config.iconContainer != null && config.iconContainer.activeSelf != isActive)
            {
                config.iconContainer.SetActive(isActive);
            }
        }
    }

    private void OnDisable()
    {
        ResetStatusUI();
    }
}