using UnityEngine;

public class BossTrailHazardSkill : MonoBehaviour, IEnemyComponent
{
    [Header("Trail Settings")]
    public float trailInterval = 0.35f;
    public GameObject trailPrefab;
    
    [Header("Hazard Block Damage Settings")]
    public float damage = 4f;
    public float duration = 3.2f;
    public float tickRate = 0.5f;

    private float _spawnTimer;
    private EnemyController _controller;
    private EnemyMovement _movement;

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
        _movement = controller.GetComponent<EnemyMovement>();
    }

    public void OnSpawnComponent()
    {
        _spawnTimer = trailInterval;
    }

    public void OnDespawnComponent()
    {
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if (_controller == null || trailPrefab == null) return;

        // Chỉ rải vệt nguy hiểm khi đang di chuyển (không bị khóa di chuyển)
        if (!_controller.isMovementLocked && _movement != null && _movement.CurrentVelocity.magnitude > 0.1f)
        {
            _spawnTimer -= deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = trailInterval;
                SpawnTrailPatch();
            }
        }
    }

    private void SpawnTrailPatch()
    {
        Vector2 spawnPos = transform.position;
        GameObject trailObj = PoolManager.Instance.Get(trailPrefab, spawnPos);
        HazardArea hazard = trailObj.GetComponent<HazardArea>();

        if (hazard != null)
        {
            hazard.Setup(trailPrefab, damage, duration, tickRate);
        }
    }
}
