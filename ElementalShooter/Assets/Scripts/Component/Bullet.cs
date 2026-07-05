using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float hitRadius = 0.1f;
    private LayerMask enemyLayer;

    private Vector2 _moveDirection;
    private bool _isDeactivating = false;
    private float _currentSpeed; // Dùng biến tạm để có thể dừng đạn
    private float lifetime = 0;

    private ElementType _element;
    private float _damage;
    private float _elementMultiplier = 1f;
    private GameObject basePrefab;

    private GameObject hitVFXPrefab;

    public void Initialized(Vector2 direction, DamageInfo info, GameObject prefab, LayerMask enemyLayer, GameObject hitVFXPrefab, float speed = 0)
    {
        this._moveDirection = direction.normalized;
        this._damage = info.Amount;
        this._element = info.Element;
        this._elementMultiplier = info.ElementMultiplier;
        this.basePrefab = prefab;
        this.enemyLayer = enemyLayer;

        this.hitVFXPrefab = hitVFXPrefab;

        _currentSpeed = this.speed; // Reset tốc độ
        if (speed != 0) _currentSpeed = speed;
  
        _isDeactivating = false;

        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Không dùng Invoke nữa, chúng ta quản lý vòng đời thủ công khi va chạm
     
    }

    void Update()
    {
        if (_isDeactivating) return;

        Vector2 currentPos = transform.position;
        float moveDistance = _currentSpeed * Time.deltaTime;
        Vector2 nextPos = currentPos + _moveDirection * moveDistance;

        RaycastHit2D hit = Physics2D.CircleCast(currentPos, hitRadius, _moveDirection, moveDistance, enemyLayer);

        if (hit.collider != null)
        {
            transform.position = hit.point + (_moveDirection * 0.1f);
            ApplyHit(hit.collider.gameObject);
            Debug.Log("hit"+ hit.collider.name);
        }
        else
        {
            transform.position = nextPos;
        }


        lifetime += Time.deltaTime;
        if (lifetime > 3) { Deactivate(); }
    }



    GameObject hitVFXObj;

    private void ApplyHit(GameObject target)
    {
        _isDeactivating = true;


        if (hitVFXPrefab != null)
        {
            // Lấy vị trí và góc xoay (rotation) hiện tại của viên đạn để gán cho VFX
            hitVFXObj = PoolManager.Instance.Get(hitVFXPrefab, target.transform.position);
            hitVFXObj.transform.rotation = gameObject.transform.rotation;


            var poolReturn = hitVFXObj.GetComponent<ParticlePoolReturn>();
            if (poolReturn != null)
            {
                poolReturn.SetBasePrefabs(hitVFXPrefab);
            }
        }

        // Chỉ xử lý Status nếu object đó có khả năng nhận Status (ví dụ: Cháy, Đóng băng...)
        if (target.TryGetComponent<IStatusReceiver>(out var status))
        {
            status.ReceiveElement(new ElementData
            {
                Type = _element,
                Multiplier = _elementMultiplier,
                SourceDamage = _damage
            });
        }


        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            // Object tự xử lý logic sát thương và nguyên tố bên trong class của nó
            damageable.TakeDamage(new DamageInfo
            {
                Amount = _damage,
                Element = _element
            });
        }
        Deactivate();
    }

    private void Deactivate()
    {
        if (gameObject.activeSelf)
        {
            StopAllCoroutines();
            PoolManager.Instance.ReturnToPool(basePrefab, gameObject);
        }
    }

    public void OnDespawn()
    {
        _isDeactivating = false;
        _currentSpeed = 0;
        lifetime = 0;
    }

    public void OnSpawn() { }
}

public struct ElementData
{
    public ElementType Type;
    public float Multiplier; // Hệ số từ kỹ năng/vũ khí
    public int SourceLevel;  // Level của người tung chiêu
    public float SourceDamage; // Damage gốc của người tung chiêu
}
