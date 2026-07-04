using UnityEngine;

public enum GameState
{
    MainMenu,
    Gameplay,
    Paused,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GameState currentGameState = GameState.MainMenu;
    private GameState previousGameState = GameState.Gameplay;

    [Header("Stats & Score")]
    private int currentScore = 0;
    private int killCount = 0;
    private float survivalTimer = 0f;
    private int lastReportedSecond = 0;

    private int highScore = 0;
    private float bestSurvivalTime = 0f;

    public GameState CurrentState => currentGameState;
    public int CurrentScore => currentScore;
    public int KillCount => killCount;
    public float SurvivalTime => survivalTimer;
    public int HighScore => highScore;
    public float BestSurvivalTime => bestSurvivalTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeAuto()
    {
        if (Instance == null)
        {
            GameManager existing = FindAnyObjectByType<GameManager>();
            if (existing == null)
            {
                GameObject gm = new GameObject("GameManager (Auto Created)");
                existing = gm.AddComponent<GameManager>();
            }
            Instance = existing;
            DontDestroyOnLoad(Instance.gameObject);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ GameManager xuyên suốt các Scene
            LoadGameData();

            // Tự động thiết lập trạng thái phù hợp tùy thuộc vào Scene bắt đầu chơi trong Editor
            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeSceneName == "MainMenu")
            {
                currentGameState = GameState.MainMenu;
            }
            else
            {
                // Nếu người chơi bắt đầu trực tiếp từ Scene chơi game để test
                currentGameState = GameState.Gameplay;
                Time.timeScale = 1f;
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        GameEvents.OnVictory += HandleVictory;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.OnVictory -= HandleVictory;
    }

    private void Update()
    {
        // Chỉ đếm thời gian khi đang thực sự chơi game
        if (currentGameState == GameState.Gameplay)
        {
            survivalTimer += Time.deltaTime;
            
            // Báo cáo thay đổi thời gian mỗi giây (tránh update UI mỗi frame)
            int currentSecond = Mathf.FloorToInt(survivalTimer);
            if (currentSecond > lastReportedSecond)
            {
                lastReportedSecond = currentSecond;
                GameEvents.RaisePlayTimeTicked(currentSecond);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentGameState != GameState.Paused)
        {
            previousGameState = currentGameState;
        }

        currentGameState = newState;
        Debug.Log($"Game State changed to: {newState}");

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                AudioListener.pause = false;
                break;

            case GameState.Gameplay:
                Time.timeScale = 1f;
                AudioListener.pause = false;
                // Nếu đang resume từ Paused thì không reset thông số
                if (previousGameState != GameState.Gameplay) 
                {
                    currentScore = 0;
                    killCount = 0;
                    survivalTimer = 0f;
                    lastReportedSecond = 0;
                    GameEvents.RaisePlayTimeTicked(0);
                    GameEvents.RaiseKillCountChanged(0);
                }
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                CheckAndSaveHighScore();
                break;

            case GameState.Victory:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                CheckAndSaveHighScore();
                break;
        }

        GameEvents.RaiseStateChanged(newState);
    }

    public void TogglePause()
    {
        if (currentGameState == GameState.Gameplay)
        {
            ChangeState(GameState.Paused);
        }
        else if (currentGameState == GameState.Paused)
        {
            ChangeState(GameState.Gameplay); // Sẽ phục hồi thời gian và âm thanh
        }
    }

    private void HandlePlayerDeath()
    {
        ChangeState(GameState.GameOver);
    }

    private void HandleEnemyKilled(Vector3 deathPosition, int expReward)
    {
        if (currentGameState == GameState.Gameplay)
        {
            currentScore += 10; // Mỗi quái chết cộng 10 điểm
            killCount++;
            GameEvents.RaiseKillCountChanged(killCount);
        }
    }

    private void HandleVictory()
    {
        ChangeState(GameState.Victory);
    }

    private void CheckAndSaveHighScore()
    {
        Debug.Log($"Checking High Score. Current Score: {currentScore} (High: {highScore}), Survival Time: {survivalTimer:F2}s (Best: {bestSurvivalTime:F2}s)");
        bool isNewRecord = false;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            isNewRecord = true;
        }

        if (survivalTimer > bestSurvivalTime)
        {
            bestSurvivalTime = survivalTimer;
            isNewRecord = true;
        }

        if (isNewRecord)
        {
            GameData dataToSave = new GameData
            {
                highScore = this.highScore,
                bestSurvivalTime = this.bestSurvivalTime
            };
            SaveSystem.Save(dataToSave);
            Debug.Log($"New High Score Saved! Score: {highScore}, Time: {bestSurvivalTime:F2}s");
        }
        else
        {
            Debug.Log("No new high score record achieved.");
        }
    }

    private void LoadGameData()
    {
        GameData loadedData = SaveSystem.Load();
        highScore = loadedData.highScore;
        bestSurvivalTime = loadedData.bestSurvivalTime;
    }

    public void RestartGame()
    {
        // Khi restart game, state phải được set lại Gameplay
        // Biến previousGameState nên được set khác Gameplay để reset thông số
        previousGameState = GameState.GameOver; 
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        ChangeState(GameState.Gameplay);
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        ChangeState(GameState.MainMenu);
    }
}
