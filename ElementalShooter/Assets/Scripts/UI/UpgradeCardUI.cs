using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI upgradeValue;
    public Image icon;
    public Button selectButton;

    private CardUpgrade _currentData; // "Linh hồn" của cái thẻ này

    private void Awake()
    {
        // Đăng ký 1 lần duy nhất khi khởi tạo Object
        selectButton.onClick.AddListener(OnClickSelect);
    }

    // Hàm này để Manager đổ dữ liệu vào
    public void Setup(CardUpgrade data)
    {
        _currentData = data;

        UpgradeManager manager = FindAnyObjectByType<UpgradeManager>();
        int currentLvl = manager != null ? manager.GetCardLevel(data) : 0;

        nameText.text = $"{data.upgradeName} (Cấp {currentLvl}/{data.maxLevel})";
        descText.text = data.description;
        
        // Cập nhật chi tiết chỉ số nâng cấp sang Tiếng Việt với so sánh trước/sau
        if (upgradeValue != null)
        {
            upgradeValue.text = GetUpgradeDetailsText(data);
        }

        // Cập nhật icon từ data
        if (icon != null)
        {
            if (data.icon != null)
            {
                icon.sprite = data.icon;
                icon.color = Color.white; // reset lại màu gốc nếu có sprite
            }
            else
            {
                icon.sprite = null;
                icon.color = Color.blueViolet; // default fallback
            }
        }
        
        Debug.Log("Setup cardUI: " + data.upgradeName);
    }

    // Hàm này gắn vào sự kiện OnClick của Button trên chính cái thẻ này
    public void OnClickSelect()
    {
        if (_currentData != null)
        {
            // Khi bấm nút, nó gửi chính cái "Linh hồn" của nó đi
            GameEvents.RaiseUpgradeSelected(_currentData);
            Debug.Log("On Select upgrade: " + _currentData.upgradeName);
        }
    }

    private string GetUpgradeDetailsText(CardUpgrade data)
    {
        StringBuilder sb = new StringBuilder();
        StatHandler playerStats = FindAnyObjectByType<StatHandler>();
        Weapon activeWeapon = FindAnyObjectByType<Weapon>();

        if (data.changes != null && data.changes.Count > 0)
        {
            foreach (var change in data.changes)
            {
                string statLabel = GetStatLabelVietnamese(change.type);
                string sign = change.value >= 0 ? "+" : "";
                
                string valueStr;
                if (change.modType == ModifierType.Percent)
                {
                    valueStr = $"{sign}{change.value * 100f}%";
                }
                else
                {
                    valueStr = $"{sign}{change.value}";
                }

                if (data.requiredWeapon != null && activeWeapon != null && activeWeapon.data == data.requiredWeapon)
                {
                    // Nâng cấp cho vũ khí cụ thể đang trang bị
                    if (change.type == StatType.Damage)
                    {
                        float cur = activeWeapon.GetFinalDamage();
                        float prev = activeWeapon.GetPreviewDamage(change.value, change.modType);
                        sb.AppendLine($"{statLabel}: {cur:F1} -> <color=#00FF00>{prev:F1}</color> ({valueStr})");
                    }
                    else if (change.type == StatType.AttackSpeed)
                    {
                        float cur = activeWeapon.GetFinalAttackSpeed();
                        float prev = activeWeapon.GetPreviewAttackSpeed(change.value, change.modType);
                        sb.AppendLine($"{statLabel}: {cur:F1} -> <color=#00FF00>{prev:F1}</color> ({valueStr})");
                    }
                    else if (change.type == StatType.ProjectileCount)
                    {
                        int cur = activeWeapon.GetFinalProjectileCount();
                        int prev = activeWeapon.GetPreviewProjectileCount(change.value);
                        sb.AppendLine($"{statLabel}: {cur} -> <color=#00FF00>{prev}</color> ({valueStr})");
                    }
                    else
                    {
                        sb.AppendLine($"{statLabel}: {valueStr}");
                    }
                }
                else if (playerStats != null)
                {
                    // Nâng cấp chỉ số chung của Player
                    float cur = playerStats.GetValue(change.type);
                    float prev = playerStats.GetPreviewValue(change.type, change.value, change.modType);
                    sb.AppendLine($"{statLabel}: {cur:F1} -> <color=#00FF00>{prev:F1}</color> ({valueStr})");
                }
                else
                {
                    sb.AppendLine($"{statLabel} {valueStr}");
                }
            }
        }

        if (data.newElement != ElementType.None)
        {
            sb.AppendLine($"<color=#FFFF00>Mở khóa Hệ: {data.newElement}</color>");
        }

        if (data.newWeapon != null)
        {
            sb.AppendLine($"<color=#00FFFF>Nhận vũ khí: {data.newWeapon.weaponName}</color>");
        }

        return sb.ToString().TrimEnd();
    }

    private string GetStatLabelVietnamese(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth:
                return "Máu tối đa";
            case StatType.Health:
                return "Máu hiện tại";
            case StatType.Damage:
                return "Sát thương";
            case StatType.MoveSpeed:
                return "Tốc độ chạy";
            case StatType.AttackSpeed:
                return "Tốc độ bắn";
            case StatType.Range:
                return "Tầm bắn";
            case StatType.ProjectileCount:
                return "Tia đạn VK";
            case StatType.Weapon:
                return "Vũ khí";
            default:
                return type.ToString();
        }
    }
}