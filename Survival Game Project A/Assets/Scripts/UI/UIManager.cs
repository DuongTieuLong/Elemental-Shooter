using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Canvas Settings")]
    [SerializeField] private Canvas screenSpaceCanvas;
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Pooling Settings")]
    [SerializeField] private int initialPoolSize = 30;

    private Queue<EnemyHealthBar> _healthBarPool = new Queue<EnemyHealthBar>();
    private List<EnemyHealthBar> _activeHealthBars = new List<EnemyHealthBar>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Đảm bảo UIManager tồn tại suốt màn chơi
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        if (screenSpaceCanvas == null)
        {
            screenSpaceCanvas = FindFirstObjectByType<Canvas>();
            if (screenSpaceCanvas == null)
            {
                GameObject canvasObj = new GameObject("ScreenSpaceCanvas");
                screenSpaceCanvas = canvasObj.AddComponent<Canvas>();
                screenSpaceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        if (healthBarPrefab == null)
        {
            Debug.LogWarning("UIManager: Health Bar Prefab chưa được gán!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewHealthBar();
        }
    }

    private EnemyHealthBar CreateNewHealthBar()
    {
        if (healthBarPrefab == null) return null;

        GameObject go = Instantiate(healthBarPrefab, screenSpaceCanvas.transform);
        go.SetActive(false);
        
        if (go.TryGetComponent<EnemyHealthBar>(out var bar))
        {
            _healthBarPool.Enqueue(bar);
            return bar;
        }
        
        Debug.LogError("UIManager: Prefab thanh máu thiếu component EnemyHealthBar!");
        Destroy(go);
        return null;
    }

    /// <summary>
    /// Cấp phát một thanh máu UI Screen Space cho quái vật.
    /// </summary>
    public EnemyHealthBar SpawnHealthBar(Transform enemyTarget)
    {
        if (enemyTarget == null) return null;

        EnemyHealthBar bar = null;
        
        // Lấy từ Pool hoặc tạo mới nếu Pool hết sạch
        if (_healthBarPool.Count > 0)
        {
            bar = _healthBarPool.Dequeue();
        }
        else
        {
            bar = CreateNewHealthBar();
        }

        if (bar != null)
        {
            bar.gameObject.SetActive(true);
            bar.SetupTarget(enemyTarget);
            _activeHealthBars.Add(bar);
        }

        return bar;
    }

    /// <summary>
    /// Thu hồi thanh máu UI Screen Space về Pool và dọn dẹp trạng thái.
    /// </summary>
    public void DespawnHealthBar(EnemyHealthBar healthBar)
    {
        if (healthBar == null) return;

        // Reset các thiết lập, stop các coroutine
        healthBar.ResetUI();
        healthBar.gameObject.SetActive(false);
        
        _activeHealthBars.Remove(healthBar);

        if (!_healthBarPool.Contains(healthBar))
        {
            _healthBarPool.Enqueue(healthBar);
        }
    }
}
