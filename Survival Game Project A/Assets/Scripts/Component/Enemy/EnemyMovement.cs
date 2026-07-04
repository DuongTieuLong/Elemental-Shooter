using UnityEngine;

public class EnemyMovement : MonoBehaviour, IEnemyComponent
{
    private EnemyController _controller;
    private Vector2 _currentVelocity;

    [Header("Context Steering Settings")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private float obstacleDangerWeight = 3f;
    [SerializeField] private float enemyDangerWeight = 0.5f;
    [SerializeField] private float minObstacleDist = 0.1f;
    [SerializeField] private float accel = 5f;
    [SerializeField] private bool showGizmos = false;

    // Buffers phi phân bổ rác GC cho di chuyển thông minh
    private static readonly Collider2D[] _separationBuffer = new Collider2D[16];
    
    // 8 hướng để chạy Context Steering
    private static readonly Vector2[] _directions = new Vector2[]
    {
        new Vector2(0, 1).normalized,   // N
        new Vector2(1, 1).normalized,   // NE
        new Vector2(1, 0).normalized,   // E
        new Vector2(1, -1).normalized,  // SE
        new Vector2(0, -1).normalized,  // S
        new Vector2(-1, -1).normalized, // SW
        new Vector2(-1, 0).normalized,  // W
        new Vector2(-1, 1).normalized   // NW
    };

    private float[] _interest = new float[8];
    private float[] _danger = new float[8];
    private float[] _result = new float[8];

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
    }

    public void OnSpawnComponent()
    {
        _currentVelocity = Vector2.zero;
    }

    public void OnDespawnComponent()
    {
        _currentVelocity = Vector2.zero;
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if (_controller == null) return;

        if (_controller.isMovementLocked)
        {
            _controller.Rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
            return;
        }

        Transform playerTransform = EnemyManager.Instance.PlayerTransform;
        if (playerTransform == null)
        {
            _controller.Rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = playerTransform.position;
        Vector2 dirToPlayer = (targetPos - currentPos);
        float distanceToPlayer = dirToPlayer.magnitude;
        
        if (distanceToPlayer > 0)
        {
            dirToPlayer /= distanceToPlayer; // Normalize
        }

        float targetSpeed = _controller.moveSpeed.Value;
        float stopDist = _controller.stopDistance.Value;
        
        bool shouldKite = (distanceToPlayer < stopDist * 0.8f && stopDist > 0);
        bool shouldStop = (!shouldKite && distanceToPlayer <= stopDist && stopDist > 0);

        // 1. Context Steering: Đánh giá mảng Interest
        Vector2 currentDir = _currentVelocity.normalized;
        for (int i = 0; i < 8; i++)
        {
            float dotToPlayer = Vector2.Dot(dirToPlayer, _directions[i]);
            float dotToCurrent = Vector2.Dot(currentDir, _directions[i]);
            
            float baseInterest = dotToPlayer;
            if (shouldStop) baseInterest = 0f;
            else if (shouldKite) baseInterest = -dotToPlayer;

            // QUAN TRỌNG: Thêm quán tính (dotToCurrent * 0.3) để tránh hiện tượng oscillation (lắc qua lại)
            // Không clamp giới hạn 0 để các hướng vuông góc vẫn giữ được độ ưu tiên khi bị kẹt
            _interest[i] = baseInterest + (dotToCurrent * 0.3f);
            
            _danger[i] = 0f; // Reset Danger
            _result[i] = 0f;
        }

        // 2. Context Steering: Tính toán mảng Danger (Tránh chướng ngại vật và quái khác)
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = LayerMask.GetMask("Obstacles", "Enemy");
        filter.useLayerMask = true;
        filter.useTriggers = false; 

        int numColliders = Physics2D.OverlapCircle(currentPos, detectionRadius, filter, _separationBuffer);

        for (int i = 0; i < numColliders; i++)
        {
            Collider2D other = _separationBuffer[i];
            if (other == null) continue;
            
            if (other.gameObject == gameObject || (other.attachedRigidbody != null && other.attachedRigidbody.gameObject == gameObject)) 
                continue;

            bool isObstacle = ((1 << other.gameObject.layer) & LayerMask.GetMask("Obstacles")) != 0;
            
            Vector2 closestPoint = other.ClosestPoint(currentPos);
            Vector2 dirToObstacle = (closestPoint - currentPos);
            float distToObstacle = dirToObstacle.magnitude;
            
            if (distToObstacle < minObstacleDist)
            {
                dirToObstacle = UnityEngine.Random.insideUnitCircle.normalized;
                distToObstacle = minObstacleDist;
            }
            else
            {
                dirToObstacle /= distToObstacle; // normalize
            }

            if (isObstacle)
            {
                for (int j = 0; j < 8; j++)
                {
                    float dot = Vector2.Dot(dirToObstacle, _directions[j]);
                    if (dot > 0) 
                    {
                        float dangerValue = Mathf.Pow(dot, 2) * obstacleDangerWeight * (1f - (distToObstacle / detectionRadius));
                        if (dangerValue > _danger[j])
                        {
                            _danger[j] = dangerValue;
                        }
                    }
                }
            }
            else // Là quái (Enemy)
            {
                float enemyAvoidRadius = 1.0f; // Bán kính tách bầy cố định, không phụ thuộc detectionRadius
                if (distToObstacle > enemyAvoidRadius) continue;
                
                for (int j = 0; j < 8; j++)
                {
                    float dot = Vector2.Dot(dirToObstacle, _directions[j]);
                    if (dot > 0) 
                    {
                        // Dùng enemyDangerWeight và falloff dựa trên enemyAvoidRadius
                        float dangerValue = Mathf.Pow(dot, 2) * enemyDangerWeight * (1f - (distToObstacle / enemyAvoidRadius));
                        if (dangerValue > _danger[j])
                        {
                            _danger[j] = dangerValue;
                        }
                    }
                }
            }
        }

        // 3. Kết hợp kết quả (Interest - Danger)
        float maxResult = -Mathf.Infinity;
        Vector2 chosenDir = Vector2.zero;
        
        for (int i = 0; i < 8; i++)
        {
            _result[i] = _interest[i] - _danger[i];
            if (_result[i] > maxResult)
            {
                maxResult = _result[i];
                chosenDir = _directions[i];
            }
        }
        
        // Không dùng fallback -dirToPlayer nữa vì nó khiến đám quái ở sau cố đẩy ngược đám quái ở trước gây kẹt (Gridlock)
        // Thuật toán sẽ tự động chọn hướng có điểm cao nhất (ít tệ nhất) ngay cả khi tất cả đều âm.

        // 4. Áp dụng vận tốc
        Vector2 desiredVelocity = chosenDir * targetSpeed;

        _currentVelocity = Vector2.Lerp(_currentVelocity, desiredVelocity, deltaTime * accel);
        
        _controller.Rb.linearVelocity = _currentVelocity;

        Vector2 lookDir = _currentVelocity != Vector2.zero ? _currentVelocity : dirToPlayer;
        if (lookDir.x > 0.05f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (lookDir.x < -0.05f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector2 currentPos = transform.position;
        
        // Vẽ vòng tròn phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPos, detectionRadius);

        // Vẽ 8 hướng kết quả
        for (int i = 0; i < 8; i++)
        {
            if (_result != null && _result.Length > i)
            {
                if (_result[i] > 0)
                {
                    Gizmos.color = Color.Lerp(Color.blue, Color.green, _result[i]);
                    Gizmos.DrawRay(currentPos, _directions[i] * _result[i] * 2f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(currentPos, _directions[i] * Mathf.Abs(_result[i]));
                }
            }
        }
    }
#endif
}
