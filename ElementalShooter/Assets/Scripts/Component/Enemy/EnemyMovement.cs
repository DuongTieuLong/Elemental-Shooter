using UnityEngine;

public class EnemyMovement : MonoBehaviour, IEnemyComponent
{
    private EnemyController _controller;

    public Vector2 CurrentVelocity { get; private set; }

    [Header("Swarm Movement Settings")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private float separationRadius = 0.5f;
    [SerializeField] private float separationForceWeight = 15f;
    [SerializeField] private float obstacleAvoidWeight = 2f;
    [SerializeField] private float accel = 5f;
    [SerializeField] private bool showGizmos = false;

    // Buffers phi phân bổ rác GC
    private static readonly EnemyController[] _enemyBuffer = new EnemyController[16];
    private static readonly RaycastHit2D[] _raycastBuffer = new RaycastHit2D[1];
    
    private static int _obstacleMask = -1;

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
        if (_obstacleMask == -1)
        {
            _obstacleMask = LayerMask.GetMask("Obstacles");
        }
    }

    public void OnSpawnComponent()
    {
        CurrentVelocity = Vector2.zero;
    }

    public void OnDespawnComponent()
    {
        CurrentVelocity = Vector2.zero;
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if (_controller == null) return;

        if (_controller.isMovementLocked)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        Transform playerTransform = EnemyManager.Instance.PlayerTransform;
        if (playerTransform == null)
        {
            CurrentVelocity = Vector2.zero;
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

        // 1. Hướng đi mong muốn cơ bản
        Vector2 desiredDir = dirToPlayer;
        if (shouldStop) desiredDir = Vector2.zero;
        else if (shouldKite) desiredDir = -dirToPlayer;

        // 2. Lực đẩy tách bầy (Separation) dựa vào Spatial Hash Grid siêu nhẹ (Chạy mỗi frame mà không lo lag)
        Vector2 separationForce = Vector2.zero;
        int neighborCount = EnemyManager.Instance.EnemyGrid.GetEnemiesInRadius(currentPos, separationRadius, _enemyBuffer);
        
        for (int i = 0; i < neighborCount; i++)
        {
            var neighbor = _enemyBuffer[i];
            if (neighbor == _controller || neighbor == null) continue;
            
            Vector2 diff = currentPos - neighbor.CachedPosition;
            float distSqr = diff.sqrMagnitude;

            // Xử lý chống kẹt cứng (nếu 2 con đè khít đúng 1 tọa độ)
            if (distSqr < 0.0001f)
            {
                diff = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                distSqr = 0.01f;
            }

            float dist = Mathf.Sqrt(distSqr);

            if (dist < separationRadius)
            {
                // Quadratic falloff: Đẩy CỰC MẠNH khi khoảng cách rất gần, yếu dần khi xa
                float pushStrength = (separationRadius - dist) / separationRadius;
                separationForce += (diff / dist) * (pushStrength * pushStrength);
            }
        }

        // 3. Tránh vật cản bằng tia (Raycast) thay vì vẽ vòng tròn
        Vector2 obstacleForce = Vector2.zero;
        if (desiredDir != Vector2.zero)
        {
            // Bắn 1 tia ngắn về phía trước
            int hits = Physics2D.RaycastNonAlloc(currentPos, desiredDir, _raycastBuffer, detectionRadius, _obstacleMask);
            if (hits > 0)
            {
                var hit = _raycastBuffer[0];
                // Tạo lực đẩy ngược lại hướng pháp tuyến của tường để trượt dọc theo tường
                obstacleForce = hit.normal * (1f - (hit.distance / detectionRadius));
            }
        }

        // 4. Tổng hợp vector
        Vector2 finalDir = desiredDir + (obstacleForce * obstacleAvoidWeight);
        if (finalDir != Vector2.zero) finalDir.Normalize();

        // 5. Nội suy vận tốc & Di chuyển
        // Thêm lực tách bầy (separation) vào sau khi Normalize để chúng có thể vượt quá tốc độ tối đa hòng bùng ra ngoài nếu bị đè quá chặt
        Vector2 targetVelocity = (finalDir * targetSpeed) + (separationForce * separationForceWeight);
        CurrentVelocity = Vector2.Lerp(CurrentVelocity, targetVelocity, deltaTime * accel);
        
        // Cập nhật vị trí thông qua Kinematic Rigidbody thay vì Transform để không kích hoạt tính toán lại Transform/Physics tốn kém
        _controller.Rb.linearVelocity = CurrentVelocity;

        // Lật mặt quái vật
        Vector2 lookDir = CurrentVelocity != Vector2.zero ? CurrentVelocity : dirToPlayer;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPos, separationRadius);
        Gizmos.color = Color.red;
        if (CurrentVelocity != Vector2.zero)
        {
            Gizmos.DrawRay(currentPos, CurrentVelocity.normalized * detectionRadius);
        }
    }
#endif
}
