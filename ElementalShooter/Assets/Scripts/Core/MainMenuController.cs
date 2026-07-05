using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;

    private void Start()
    {
        // Tải dữ liệu điểm cao từ SaveSystem để hiển thị lên Menu
        GameData data = SaveSystem.Load();
        if (highScoreText != null)
        {
            highScoreText.text = $"HIGH SCORE: {data.highScore}\nBEST TIME: {data.bestSurvivalTime:F1}s";
        }
    }

    public void PlayGame()
    {
        // Gọi GameManager để chuyển trạng thái sang chơi game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Gameplay);
        }
        // Nạp scene gameplay chính (ví dụ scene index 1 hoặc tên scene)
        SceneManager.LoadScene(1);
    }
}