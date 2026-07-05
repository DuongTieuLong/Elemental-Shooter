using UnityEngine;
using System.Collections.Generic;


public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private Transform playerTransform;
    public float CurrentDifficultyMultiplier { get; private set; } = 1.0f;

    // Danh sách các Enemy đang hoạt động trên màn hình
    private List<EnemyController> _activeEnemies = new List<EnemyController>();

    // Lưới không gian tối ưu cho truy vấn phân tách bầy
    public SpatialHashGrid EnemyGrid { get; private set; }

    private void Awake()
    {
        Instance = this;
        EnemyGrid = new SpatialHashGrid(1f); // Cell size 1 đơn vị để chia nhỏ lưới hơn nữa

        // Bỏ qua hoàn toàn va chạm vật lý giữa quái và quái để cứu CPU
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerSpawned += SetPlayerTarget;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSpawned -= SetPlayerTarget;
    }

    public void SetPlayerTarget(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }


    public void RegisterEnemy(EnemyController enemy)
    {
        _activeEnemies.Add(enemy);
        // "Đăng ký" lắng nghe tin buồn từ Enemy
        enemy.OnDeathRequestPool += CollectEnemy;
    }

    public void UnregisterEnemy(EnemyController enemy)
    {
        _activeEnemies.Remove(enemy);
        enemy.OnDeathRequestPool -= CollectEnemy;
    }

    private void CollectEnemy(GameObject prefab, GameObject enemyObj)
    {
        // Manager thực hiện việc thu hồi 
        PoolManager.Instance.ReturnToPool(prefab, enemyObj);
    }

    public void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public Transform PlayerTransform => playerTransform;

    private void Update()
    {
        if (playerTransform == null) return;

        float deltaTime = Time.deltaTime;
        Vector3 playerPos = playerTransform.position;

        // 1. Cập nhật Spatial Grid (Cực nhanh, < 0.5ms)
        EnemyGrid.Clear();
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            var enemy = _activeEnemies[i];
            enemy.CachedPosition = enemy.transform.position;
            EnemyGrid.Add(enemy);
        }

        // 2. Vòng lặp tập trung duy nhất xử lý di chuyển cho hàng trăm con
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            _activeEnemies[i].DoUpdate(playerPos, deltaTime);
        }
        if(CurrentDifficultyMultiplier < 3)
            CurrentDifficultyMultiplier = 1.0f + (Time.timeSinceLevelLoad / 15f) * 0.05f;
    }
}

// Cấu trúc lưới không gian (Spatial Hash Grid) tối ưu không xả rác GC
public class SpatialHashGrid
{
    private Dictionary<Vector2Int, List<EnemyController>> _grid = new Dictionary<Vector2Int, List<EnemyController>>();
    private float _cellSize;
    private Stack<List<EnemyController>> _listPool = new Stack<List<EnemyController>>();

    public SpatialHashGrid(float cellSize)
    {
        _cellSize = cellSize;
    }

    public void Clear()
    {
        foreach (var kvp in _grid)
        {
            kvp.Value.Clear();
            _listPool.Push(kvp.Value);
        }
        _grid.Clear();
    }

    private Vector2Int GetCellCoords(Vector2 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / _cellSize), Mathf.FloorToInt(pos.y / _cellSize));
    }

    public void Add(EnemyController enemy)
    {
        Vector2Int cell = GetCellCoords(enemy.CachedPosition);
        if (!_grid.TryGetValue(cell, out List<EnemyController> list))
        {
            list = _listPool.Count > 0 ? _listPool.Pop() : new List<EnemyController>();
            _grid[cell] = list;
        }
        list.Add(enemy);
    }

    // Truy vấn cực nhanh danh sách quái trong bán kính, điền vào mảng cho sẵn để không sinh GC
    public int GetEnemiesInRadius(Vector2 pos, float radius, EnemyController[] results)
    {
        int count = 0;
        int maxResults = results.Length;
        Vector2Int centerCell = GetCellCoords(pos);
        int cellRadius = Mathf.CeilToInt(radius / _cellSize);
        float sqrRadius = radius * radius;

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector2Int cell = centerCell + new Vector2Int(x, y);
                if (_grid.TryGetValue(cell, out List<EnemyController> list))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (count >= maxResults) return count; // Tối ưu: Dừng ngay lập tức nếu đã đầy buffer (tránh O(N^2) khi 500 quái dính chùm)

                        var enemy = list[i];
                        if ((enemy.CachedPosition - pos).sqrMagnitude <= sqrRadius)
                        {
                            results[count++] = enemy;
                        }
                    }
                }
            }
        }
        return count;
    }
}
