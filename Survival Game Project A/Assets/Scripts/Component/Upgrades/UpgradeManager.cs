using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public class UpgradeManager : MonoBehaviour
{
    public List<CardUpgrade> allUpgrades;

    private void OnEnable()
    {
        GameEvents.OnLevelUp += ShowUpgradeOptions;
        GameEvents.OnStatUpdated += FinalizeUpgrade;
        GameEvents.OnUpgradeSelected += HandleUpgradeSelected;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUp -= ShowUpgradeOptions;
        GameEvents.OnStatUpdated -= FinalizeUpgrade;
        GameEvents.OnUpgradeSelected -= HandleUpgradeSelected;
    }

    private int _rerollCount = 0;
    private const int MaxRerolls = 3;
    private Dictionary<CardUpgrade, int> _cardLevels = new Dictionary<CardUpgrade, int>();

    public int RerollCount => _rerollCount;

    public int GetCardLevel(CardUpgrade card)
    {
        if (card == null) return 0;
        return _cardLevels.TryGetValue(card, out var lvl) ? lvl : 0;
    }

    public Dictionary<CardUpgrade, int> GetActiveUpgrades()
    {
        return _cardLevels;
    }

    private void HandleUpgradeSelected(CardUpgrade upgrade)
    {
        if (upgrade == null) return;
        if (!_cardLevels.ContainsKey(upgrade))
        {
            _cardLevels[upgrade] = 1;
        }
        else
        {
            _cardLevels[upgrade]++;
        }
        Debug.Log($"Card {upgrade.upgradeName} upgraded to level {_cardLevels[upgrade]}/{upgrade.maxLevel}");
    }

    private void ShowUpgradeOptions()
    {
        Time.timeScale = 0;
        Debug.Log("Show Upgrade Options by Manager");

        // +1 Reroll count when level up, capped at 3
        _rerollCount = Mathf.Min(_rerollCount + 1, MaxRerolls);

        var chosen = GetRandomCards(3);
        if (chosen.Count < 3)
        {
            Debug.Log("Khong du 3 card upgrade ");
        }
        // Thay vì tự làm UI, nó "ra lệnh" cho hệ thống UI
        GameEvents.RaiseRequestUpgradeUI(chosen);
    }

    public void RerollUpgrades()
    {
        if (_rerollCount > 0)
        {
            _rerollCount--;
            var chosen = GetRandomCards(3);
            if (chosen.Count < 3)
            {
                Debug.Log("Khong du 3 card upgrade ");
            }
            GameEvents.RaiseRequestUpgradeUI(chosen);
        }
    }

    // Hàm này được gọi khi người chơi chọn xong 1 thẻ nâng cấp
    private void FinalizeUpgrade()
    {
        // Khi nghe tin, Manager làm đúng 1 việc: Mở lại thời gian
        Time.timeScale = 1;
        Debug.Log("Game Resumed by Manager");
    }

    Weapon activeWeapon;
    public List<CardUpgrade> GetRandomCards(int count)
    {
        // 1. Tìm vũ khí hiện tại của người chơi
        if (activeWeapon == null)
        {
            activeWeapon = FindAnyObjectByType<Weapon>();
        }

        WeaponData equippedWeaponData = activeWeapon != null ? activeWeapon.data : null;

        // 2. Lọc ra danh sách thẻ nâng cấp hợp lệ
        List<CardUpgrade> validUpgrades = new List<CardUpgrade>();

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;

            // THOẢ MÃN CÁC ĐIỀU KIỆN SAU THÌ MỚI GIỮ LẠI:
            // ĐK 1: Thẻ nâng cấp chung (Global), không yêu cầu vũ khí nào cả.
            bool isGlobalUpgrade = (upgrade.requiredWeapon == null && upgrade.newWeapon == null);

            // ĐK 2: Thẻ nâng cấp cho vũ khí HIỆN TẠI đang có (Yêu cầu đúng vũ khí đang trang bị)
            bool isUpgradeForEquipped = (upgrade.requiredWeapon != null && upgrade.requiredWeapon == equippedWeaponData);

            // ĐK 3: Thẻ mở khóa vũ khí MỚI HOÀN TOÀN (Vũ khí đó chưa được trang bị)
            bool isNewWeaponToUnlock = (upgrade.newWeapon != null && upgrade.newWeapon != equippedWeaponData);

            // ĐK 4: Thẻ chưa đạt cấp độ tối đa (maxLevel)
            int currentLvl = GetCardLevel(upgrade);
            bool isNotMaxLevel = currentLvl < upgrade.maxLevel;

            if ((isGlobalUpgrade || isUpgradeForEquipped || isNewWeaponToUnlock) && isNotMaxLevel)
            {
                validUpgrades.Add(upgrade);
            }
        }

        // 3. Xáo trộn ngẫu nhiên danh sách đã lọc (Fisher-Yates Shuffle) để không bị trùng
        for (int i = validUpgrades.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Đổi chỗ phần tử i và randomIndex
            CardUpgrade temp = validUpgrades[i];
            validUpgrades[i] = validUpgrades[randomIndex];
            validUpgrades[randomIndex] = temp;
        }

        // 4. Lấy ra số lượng 'count' phần tử từ danh sách đã xáo trộn
        List<CardUpgrade> result = new List<CardUpgrade>();
        int actualCount = Mathf.Min(count, validUpgrades.Count); // Đề phòng trường hợp danh sách hợp lệ ít hơn số 'count' yêu cầu

        for (int i = 0; i < actualCount; i++)
        {
            result.Add(validUpgrades[i]);
        }

        return result;
    }


}