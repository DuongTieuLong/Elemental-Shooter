using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public GameObject UpgradePanel;
    public List<UpgradeCardUI> cardUIList;

    [Header("Reroll References")]
    public Button rerollButton;
    public TextMeshProUGUI rerollText;

    [Header("Player Stats Summary")]
    public TextMeshProUGUI playerStatsText;

    private UpgradeManager _upgradeManager;
    StatHandler playerStats;

    private void Awake()
    {
        playerStats =  FindAnyObjectByType<StatHandler>();
        _upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(OnRerollClicked);
        }
    }

    void OnEnable()
    {
        GameEvents.OnRequestUpgradeUI += ShowUpgradePanel;
        GameEvents.OnStatUpdated += ClosePanel;
    }

    private void Start()
    {
        RefershPlayerData();
    }

    public void ShowUpgradePanel(List<CardUpgrade> cardsUpgrade)
    {
        Debug.Log("Show upgrade on level up");
        
        // Setup card UI slots
        for (int i = 0; i < cardUIList.Count; i++) 
        {
            if (i < cardsUpgrade.Count)
            {
                cardUIList[i].gameObject.SetActive(true);
                cardUIList[i].Setup(cardsUpgrade[i]);
            }
            else
            {
                cardUIList[i].gameObject.SetActive(false);
            }
        }

        // Cập nhật bảng tóm tắt chỉ số của player
        RefershPlayerData();

        UpdateRerollUI();
        UpgradePanel.SetActive(true);
    }

    private void OnRerollClicked()
    {
        if (_upgradeManager != null && _upgradeManager.RerollCount > 0)
        {
            _upgradeManager.RerollUpgrades();
        }
    }

    private void UpdateRerollUI()
    {
        if (_upgradeManager != null)
        {
            if (rerollText != null)
            {
                rerollText.text = $"Reroll ({_upgradeManager.RerollCount}/3)";
            }
            if (rerollButton != null)
            {
                rerollButton.interactable = _upgradeManager.RerollCount > 0;
            }
        }
    }

    public void RefershPlayerData()
    {
        if (playerStats != null && playerStatsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b><size=125%><color=#FFA500>CHỈ SỐ NHÂN VẬT</color></size></b>\n");
            sb.AppendLine($"Máu tối đa: <color=#00FF00>{playerStats.GetValue(StatType.MaxHealth).ToString("F2")}</color>");
            sb.AppendLine($"Sát thương: <color=#FF3333>{playerStats.GetValue(StatType.Damage).ToString("F2")}</color>");
            sb.AppendLine($"Tốc chạy: <color=#3399FF>{playerStats.GetValue(StatType.MoveSpeed).ToString("F2")}</color>");
            sb.AppendLine($"Tốc bắn: <color=#FFFF33>{playerStats.GetValue(StatType.AttackSpeed).ToString("F2")}</color>");
            sb.AppendLine($"Tầm đánh: <color=#FF33FF>{playerStats.GetValue(StatType.Range).ToString("F2")}</color>");

            // Tìm vũ khí hiện có để show thêm chỉ số vũ khí
            Weapon activeWeapon = FindAnyObjectByType<Weapon>();
            if (activeWeapon != null && activeWeapon.data != null)
            {
                sb.AppendLine($"\n<b><size=115%><color=#E0E0E0>Vũ khí: {activeWeapon.data.weaponName}</color></size></b>");
                sb.AppendLine($"Sát thương VK: <color=#FF3333>{activeWeapon.GetFinalDamage().ToString("F2")}</color>");
                sb.AppendLine($"Tốc bắn VK: <color=#FFFF33>{activeWeapon.GetFinalAttackSpeed().ToString("F2")}</color>");
                sb.AppendLine($"Tia đạn: <color=#33FFFF>{activeWeapon.GetFinalProjectileCount().ToString("F2")}</color>");
            }

            playerStatsText.text = sb.ToString();
        }
    }
    public void ClosePanel()
    {
        UpgradePanel?.SetActive(false);
        RefershPlayerData();
    }

    private void OnDisable()
    {
        GameEvents.OnRequestUpgradeUI -= ShowUpgradePanel;
        GameEvents.OnStatUpdated -= ClosePanel;
    }
}
