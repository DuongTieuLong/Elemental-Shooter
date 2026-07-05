using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardArea : MonoBehaviour, IPoolable
{
    private GameObject _basePrefab;
    private float _damage = 5f;
    private float _duration = 3f;
    private float _tickRate = 0.5f;

    private float _lifetimeTimer;
    private float _damageTimer;
    private bool _hasPlayer;
    private PlayerHealth _playerHealth;

    public void Setup(GameObject prefab, float damage, float duration, float tickRate)
    {
        _basePrefab = prefab;
        _damage = damage;
        _duration = duration;
        _tickRate = tickRate;
    }

    public void OnSpawn()
    {
        _lifetimeTimer = _duration;
        _damageTimer = 0f;
        _hasPlayer = false;
        _playerHealth = null;

        // Đảm bảo collider là Trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void OnDespawn()
    {
        _playerHealth = null;
        _hasPlayer = false;
    }

    private void Update()
    {
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
        {
            DespawnSelf();
            return;
        }

        if (_hasPlayer && _playerHealth != null)
        {
            if (_damageTimer > 0f)
            {
                _damageTimer -= Time.deltaTime;
            }

            if (_damageTimer <= 0f)
            {
                _playerHealth.TakeDamage(new DamageInfo
                {
                    Amount = _damage,
                    Element = ElementType.None
                });
                _damageTimer = _tickRate;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _hasPlayer = true;
            _playerHealth = collision.GetComponent<PlayerHealth>();
            
            // Gây sát thương ngay lập tức khi bước vào nếu sẵn sàng
            if (_damageTimer <= 0f)
            {
                _playerHealth.TakeDamage(new DamageInfo
                {
                    Amount = _damage,
                    Element = ElementType.None
                });
                _damageTimer = _tickRate;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _hasPlayer = false;
            _playerHealth = null;
        }
    }

    private void DespawnSelf()
    {
        if (gameObject.activeSelf)
        {
            if (_basePrefab != null && PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnToPool(_basePrefab, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
