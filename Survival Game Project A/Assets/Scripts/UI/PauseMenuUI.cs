using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;

    [Header("Player Stats UI (Column 1)")]
    [SerializeField] private Image playerAvatar;
    [SerializeField] private TextMeshProUGUI statsCol1Text;

    [Header("Weapon Stats UI (Column 2)")]
    [SerializeField] private Image weaponAvatar;
    [SerializeField] private TextMeshProUGUI statsCol2Text;

    [Header("Upgrades UI (Grid)")]
    [SerializeField] private Transform upgradesGridParent;
    [SerializeField] private GameObject upgradeIconPrefab; // Prefab chứa Image và Text(để hiển thị Level)

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private StatHandler playerStatHandler;
    private UpgradeManager upgradeManager;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (retryButton != null) retryButton.onClick.AddListener(RetryGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        // Giả định PlayerStatHandler và UpgradeManager tồn tại trong scene
        playerStatHandler = FindAnyObjectByType<StatHandler>();
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
    }

    private void Update()
    {
        // Quản lý Input phím Esc bằng New Input System
        bool pausePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        
        // Thêm Gamepad Start button nếu cần
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            pausePressed = true;
        }

        if (pausePressed)
        {
            if (GameManager.Instance != null && 
               (GameManager.Instance.CurrentState == GameState.Gameplay || GameManager.Instance.CurrentState == GameState.Paused))
            {
                TogglePauseMenu();
            }
        }
    }

    public void PauseGame()
    {
        if (GameManager.Instance != null &&
              (GameManager.Instance.CurrentState == GameState.Gameplay || GameManager.Instance.CurrentState == GameState.Paused))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        GameManager.Instance.TogglePause();

        bool isPaused = GameManager.Instance.CurrentState == GameState.Paused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            RefreshUI();
        }
    }

    private void ResumeGame()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
        {
            TogglePauseMenu();
        }
    }

    private void RetryGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void ReturnToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    private void RefreshUI()
    {
        RefreshStats();
        RefreshUpgrades();
    }

    private void RefreshStats()
    {
        if (playerStatHandler == null) return;

        // Cập nhật Avatar Player
        if (playerAvatar != null && playerStatHandler.baseData != null)
        {
            playerAvatar.sprite = playerStatHandler.baseData.icon;
        }

        // Cột 1: Thông số cơ bản (Player)
        string col1 = $"Max Health: {playerStatHandler.GetValue(StatType.MaxHealth):F0}\n" +
                      $"Move Speed: {playerStatHandler.GetValue(StatType.MoveSpeed):F1}\n" +
                      $"Range: {playerStatHandler.GetValue(StatType.Range):F1}";
        
        if (statsCol1Text != null) statsCol1Text.text = col1;

        // Cột 2: Thông số chiến đấu (Weapon)
        Weapon activeWeapon = playerStatHandler.GetComponent<Weapon>();
        if (activeWeapon != null && activeWeapon.data != null)
        {
            if (weaponAvatar != null)
            {
                weaponAvatar.sprite = activeWeapon.data.weaponIcon;
            }

            string col2 = $"Damage: {activeWeapon.GetFinalDamage():F0}\n" +
                          $"Attack Speed: {activeWeapon.GetFinalAttackSpeed():F1}\n" +
                          $"Projectiles: {activeWeapon.GetFinalProjectileCount()}";
            
            if (statsCol2Text != null) statsCol2Text.text = col2;
        }
        else
        {
            if (statsCol2Text != null) statsCol2Text.text = "No Weapon";
        }
    }

    private void RefreshUpgrades()
    {
        if (upgradeManager == null || upgradesGridParent == null || upgradeIconPrefab == null) return;

        // Xóa grid cũ
        foreach (Transform child in upgradesGridParent)
        {
            Destroy(child.gameObject);
        }

        var activeUpgrades = upgradeManager.GetActiveUpgrades();
        foreach (var kvp in activeUpgrades)
        {
            CardUpgrade card = kvp.Key;
            int level = kvp.Value;

            GameObject iconObj = Instantiate(upgradeIconPrefab, upgradesGridParent);
            
            // Giả định Prefab có Component Image ở gốc hoặc child
            Image iconImg = iconObj.GetComponentInChildren<Image>();
            if (iconImg != null && card.icon != null)
            {
                iconImg.sprite = card.icon;
            }

            // Gắn dữ liệu cho UpgradeIconUI để hiển thị Tooltip khi hover
            UpgradeIconUI iconUI = iconObj.GetComponent<UpgradeIconUI>();
            if (iconUI == null) iconUI = iconObj.AddComponent<UpgradeIconUI>();
            iconUI.Setup(card, level);

            // Giả định Prefab có TextMeshProUGUI để hiển thị Level
            TextMeshProUGUI[] texts = iconObj.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var t in texts)
            {
                // Cập nhật text nào trông giống hiển thị Level
                t.text = $"Lv.{level}";
            }
        }
    }
}
