using UnityEngine;
using System.Collections.Generic;
public class StatHandler : MonoBehaviour
{
    public PlayerData baseData;
    [SerializeField] private StatBalanceConfig balanceConfig;
    private Weapon weapon;

    private const float ElementBaseMultiplier = 1.0f;
    private const float ElementStackMultiplier = 0.1f;

    // Kho chứa tất cả các chỉ số của Player
    private Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

    // Kho chứa sức mạnh nguyên tố (có thể dùng class Stat cho Element luôn nếu cần nâng cấp)
    public Dictionary<ElementType, float> ElementModifiers = new Dictionary<ElementType, float>();

    private void Awake()
    {
        InitializeStats();

        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("StatHandler requires a Weapon component on the same GameObject.");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnUpgradeSelected += ApplyUpgrade;
    }

    private void InitializeStats()
    {
        // Khởi tạo các chỉ số cơ bản từ PlayerData
        _stats[StatType.MaxHealth] = new Stat { BaseValue = baseData.maxHealth };
        _stats[StatType.Damage] = new Stat { BaseValue = baseData.attackDamage };
        _stats[StatType.MoveSpeed] = new Stat { BaseValue = baseData.moveSpeed };
        _stats[StatType.AttackSpeed] = new Stat { BaseValue = baseData.attackSpeed };
        _stats[StatType.Range] = new Stat { BaseValue = baseData.attackRange };
    }

    // Hàm tiện ích để lấy giá trị nhanh
    public float GetValue(StatType type)
    {
        if (!_stats.ContainsKey(type)) return 0;
        float rawValue = _stats[type].Value;
        if (balanceConfig != null)
        {
            return balanceConfig.ClampValue(type, rawValue);
        }
        return rawValue;
    }

    public float GetPreviewValue(StatType type, float amount, ModifierType modType)
    {
        if (!_stats.ContainsKey(type)) return 0;
        float rawPreview = _stats[type].GetPreviewValue(amount, modType);
        if (balanceConfig != null)
        {
            return balanceConfig.ClampValue(type, rawPreview);
        }
        return rawPreview;
    }
    public Stat GetStat(StatType type) => _stats.ContainsKey(type) ? _stats[type] : null;

    public void ApplyUpgrade(CardUpgrade upgrade)
    {
        if(upgrade == null)
        {
            Debug.LogWarning("ApplyUpgrade called with null upgrade.");
            return;
        }

        // Nếu thẻ yêu cầu một vũ khí cụ thể, chúng ta áp dụng nâng cấp riêng cho vũ khí đó
        if (upgrade.requiredWeapon != null)
        {
            if (weapon != null)
            {
                weapon.ApplyWeaponUpgrade(upgrade.requiredWeapon, upgrade.changes);
               // Debug.Log("weapon upgrade" + weapon.);
            }
        }
        else
        {
            // 1. Xử lý các chỉ số thông thường của Player
            foreach (var change in upgrade.changes)
            {
                if (_stats.TryGetValue(change.type, out Stat stat))
                {
                    // Giả sử CardUpgrade có định nghĩa loại modifier là Flat hay Percent
                    stat.AddModifier(change.value, change.modType);
                }
            }
        }

        // 2. Xử lý Nguyên tố
        if (upgrade.newElement != ElementType.None)
        {
            if (!ElementModifiers.ContainsKey(upgrade.newElement))
                ElementModifiers[upgrade.newElement] = ElementBaseMultiplier;
            else
                ElementModifiers[upgrade.newElement] += ElementStackMultiplier;
        }

        // 3. Xử lý vũ khí mới được trang bị
        if (upgrade.newWeapon != null)
        {
            if (weapon != null)
            {
                weapon.EquipWeapon(upgrade.newWeapon);
            }
        }

        // 4. Thông báo cho UI hoặc hệ thống khác
        GameEvents.RaiseStatUpdated();

 
    }

    // Hàm trả về danh sách các Nguyên tố mà người chơi đang có
    public List<ElementType> GetActiveElements()
    {
        // Trả về một List chứa các Key (Fire, Ice, v.v.) từ Dictionary
        return new List<ElementType>(ElementModifiers.Keys);
    }

    public float GetElementModifier(ElementType element)
    {
        return ElementModifiers.ContainsKey(element) ? ElementModifiers[element] : 0f;
    }

    public ElementType GetRandomEffect()
    {
        List<ElementType> availableElements = GetActiveElements();

        // Bước 1: Tung xúc xắc 50/50 trước
        // Nếu tỷ lệ rơi vào nhóm 50% ra None, HOẶC danh sách rỗng/null thì bắt buộc trả về None
        if (Random.value < 0.5f || availableElements == null || availableElements.Count == 0)
        {
            return ElementType.None;
        }

        // Bước 2: 50% còn lại, tiến hành random ngẫu nhiên trong danh sách đã có
        int randomIndex = Random.Range(0, availableElements.Count);
        return availableElements[randomIndex];
    }

    private void OnDisable()
    {
        GameEvents.OnUpgradeSelected -= ApplyUpgrade;
    }
}


