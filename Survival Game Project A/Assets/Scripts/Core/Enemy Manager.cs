using UnityEngine;
using System.Collections.Generic;


public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private Transform playerTransform;
    public float CurrentDifficultyMultiplier { get; private set; } = 1.0f;

    // Danh sách các Enemy đang hoạt động trên màn hình
    private List<EnemyController> _activeEnemies = new List<EnemyController>();

    private void Awake() => Instance = this;

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

        // Vòng lặp tập trung duy nhất xử lý di chuyển cho hàng trăm con
        for (int i = 0; i < _activeEnemies.Count; i++)
        {
            _activeEnemies[i].DoUpdate(playerPos, deltaTime);
        }
        if(CurrentDifficultyMultiplier < 3)
            CurrentDifficultyMultiplier = 1.0f + (Time.timeSinceLevelLoad / 15f) * 0.05f;
    }
}
