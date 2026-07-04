using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Game Over UI References")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverTimeText;
    [SerializeField] private TextMeshProUGUI gameOverBestText;

    [Header("Victory UI References")]
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private TextMeshProUGUI victoryTimeText;
    [SerializeField] private TextMeshProUGUI victoryBestText;

    [Header("Buttons")]
    [SerializeField] private Button[] retryButtons;
    [SerializeField] private Button[] mainMenuButtons;

    private void OnEnable()
    {
        GameEvents.OnStateChanged += HandleStateChanged;

        // Đăng ký sự kiện Click cho các nút bấm
        foreach (var btn in retryButtons)
        {
            if (btn != null) btn.onClick.AddListener(OnRetryClicked);
        }
        foreach (var btn in mainMenuButtons)
        {
            if (btn != null) btn.onClick.AddListener(OnMainMenuClicked);
        }

        // Tự động ẩn các Panel khi bắt đầu
        HideAllPanels();
    }

    private void OnDisable()
    {
        GameEvents.OnStateChanged -= HandleStateChanged;

        foreach (var btn in retryButtons)
        {
            if (btn != null) btn.onClick.RemoveListener(OnRetryClicked);
        }
        foreach (var btn in mainMenuButtons)
        {
            if (btn != null) btn.onClick.RemoveListener(OnMainMenuClicked);
        }
    }

    private void HandleStateChanged(GameState newState)
    {
        HideAllPanels();

        if (newState == GameState.GameOver)
        {
            ShowGameOver();
        }
        else if (newState == GameState.Victory)
        {
            ShowVictory();
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (GameManager.Instance != null)
        {
            int score = GameManager.Instance.CurrentScore;
            float time = GameManager.Instance.SurvivalTime;
            int high = GameManager.Instance.HighScore;
            float bestTime = GameManager.Instance.BestSurvivalTime;

            if (gameOverScoreText != null) gameOverScoreText.text = $"SCORE: {score}";
            if (gameOverTimeText != null) gameOverTimeText.text = $"SURVIVED: {time:F1}s";
            if (gameOverBestText != null) gameOverBestText.text = $"BEST: {high} (Time: {bestTime:F1}s)";
        }
    }

    private void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);

        if (GameManager.Instance != null)
        {
            int score = GameManager.Instance.CurrentScore;
            float time = GameManager.Instance.SurvivalTime;
            int high = GameManager.Instance.HighScore;
            float bestTime = GameManager.Instance.BestSurvivalTime;

            if (victoryScoreText != null) victoryScoreText.text = $"FINAL SCORE: {score}";
            if (victoryTimeText != null) victoryTimeText.text = $"TIME CLEAR: {time:F1}s";
            if (victoryBestText != null) victoryBestText.text = $"RECORD BEST: {high} (Time: {bestTime:F1}s)";
        }
    }

    private void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void OnRetryClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
