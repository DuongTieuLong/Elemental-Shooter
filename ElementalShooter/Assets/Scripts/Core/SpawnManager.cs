using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<WaveConfig> allWaves; // Danh sách cấu hình các đợt sóng
    [SerializeField] private int currentWaveIndex = 0; // Chỉ số của đợt sóng hiện tại
    private List<GameObject> activeEnemies = new List<GameObject>(); // Danh sách các kẻ địch đang hoạt động

    public Transform player;
    public float spawnRadiusMin = 10f; // Khoảng cách tối thiểu từ player để spawn
    public float spawnRadiusMax = 20f; // Khoảng cách tối đa từ player để spawn

    public void Start()
    {
        StartCoroutine(SpawnWaveRoutine());
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
        player = playerTransform;
        Debug.Log("Player target set for SpawnManager.");
    }

    IEnumerator SpawnWaveRoutine()
    {
        while (currentWaveIndex < allWaves.Count)
        {
            WaveConfig currentWave = allWaves[currentWaveIndex];

            IWaveStrategy strategy = GetStrategyForWave(currentWave.waveType);
            strategy.Init(this, currentWave, currentWaveIndex);

            yield return StartCoroutine(strategy.ExecuteWave());

            currentWaveIndex++;
            Debug.Log("Chuyển sang đợt sóng tiếp theo: " + (currentWaveIndex + 1));
        }

        Debug.Log("Tất cả các đợt sóng đã kết thúc! Chiến thắng!");
        GameEvents.RaiseVictory();
    }

    private IWaveStrategy GetStrategyForWave(WaveType type)
    {
        switch (type)
        {
            case WaveType.TimeBased: return new TimeWaveStrategy();
            case WaveType.KillBased: return new KillWaveStrategy();
            case WaveType.Boss: return new BossWaveStrategy();
            default: return new TimeWaveStrategy();
        }
    }

    public void SpawnEnemy(GameObject prefab)
    {
        if(player == null)
        {
            Debug.LogWarning("Player not found! Cannot spawn enemies.");
            return; // Không thể spawn nếu không tìm thấy Player
        }
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = PoolManager.Instance.Get(prefab, spawnPos);
        activeEnemies.Add(enemy);
    }

    public List<GameObject> GetActiveEnemies()
    {
        return activeEnemies;
    }

    Vector2 GetRandomSpawnPosition()
    {
        return SpawnUtils.GetRandomCirclePos(player.position, spawnRadiusMin, spawnRadiusMax);
    }
}
