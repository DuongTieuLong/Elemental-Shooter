using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel;

    [Header("Balancing Settings")]
    [SerializeField] private int baseExp = 100;
    [SerializeField] private float exponent = 1.35f;

    private void OnEnable()
    {
        GameEvents.OnEnergyCollected += AddExperience;
        GameEvents.OnUpgradeSelected += HandleUpgradeSelected;
    }

    private void OnDisable()
    {
        GameEvents.OnEnergyCollected -= AddExperience;
        GameEvents.OnUpgradeSelected -= HandleUpgradeSelected;
    }

    private void HandleUpgradeSelected(CardUpgrade upgrade)
    {
        TryTriggerLevelUp();
    }

    public void AddExperience(int exp)
    {
        Debug.Log($"You receive {exp} Exp!");
        currentExp += exp; // Add experience points
        CheckLevelUp(); // Check if the player has leveled up
    }

    private int pendingLevels = 0; // Số lần được chọn thẻ đang chờ

    public void CheckLevelUp()
    {
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            UpdateExpToNextLevel(); 
            pendingLevels++;
        }

        // Nếu đang không Pause (tức là không trong menu nâng cấp) thì mới mở
        if (pendingLevels > 0 && Time.timeScale != 0)
        {
            TryTriggerLevelUp();
        }
    }

    private void UpdateExpToNextLevel()
    {
        // Sử dụng Mathf.Pow để tính số mũ và ép kiểu về int
        expToNextLevel = Mathf.RoundToInt(baseExp * Mathf.Pow(currentLevel, exponent));
    }

    // Tách hàm này ra để gọi lại nhiều lần
    public void TryTriggerLevelUp()
    {
        if (pendingLevels > 0)
        {
            pendingLevels--; // Giảm lượt ngay khi bắt đầu mở UI
            GameEvents.OnLevelUp?.Invoke();
        }
    }

}
