using UnityEngine;
using System.Collections;

namespace Component.Enemy.Feedback
{
    public class EnemyVFXController : MonoBehaviour
    {
        private EnemyController _enemyController;
        private EnemyHealth _enemyHealth;
        private SpriteRenderer _spriteRenderer;

        [Header("VFX Prefabs")]
        public GameObject hitVfxPrefab;
        public GameObject deathVfxPrefab;
        public GameObject skillCastVfxPrefab;

        [Header("Flash Settings")]
        public bool enableHitFlash = true;
        public Color flashColor = Color.red;
        public float flashDuration = 0.1f;
        
        private Color _originalColor = Color.white;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _enemyController = GetComponentInParent<EnemyController>();
            _enemyHealth = GetComponentInParent<EnemyHealth>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (_enemyController != null)
            {
                _enemyController.OnSkillCastEvent += SpawnSkillCastVFX;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.onHit += HandleHit;
                _enemyHealth.onDeath += SpawnDeathVFX;
            }
        }

        private void OnDisable()
        {
            if (_enemyController != null)
            {
                _enemyController.OnSkillCastEvent -= SpawnSkillCastVFX;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.onHit -= HandleHit;
                _enemyHealth.onDeath -= SpawnDeathVFX;
            }
            
            // Đảm bảo trả lại màu ban đầu khi bị disable
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        private void HandleHit()
        {
            SpawnHitVFX();
            
            if (enableHitFlash && gameObject.activeInHierarchy)
            {
                if (_flashCoroutine != null)
                {
                    StopCoroutine(_flashCoroutine);
                }
                _flashCoroutine = StartCoroutine(FlashRoutine());
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (_spriteRenderer == null) yield break;

            _spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            _spriteRenderer.color = _originalColor;
        }

        private void SpawnHitVFX()
        {
            if (hitVfxPrefab == null || PoolManager.Instance == null) return;
            // Sinh VFX tại tâm quái vật
            GameObject vfx = PoolManager.Instance.Get(hitVfxPrefab, transform.position);
            // Có thể thêm component tự hủy (ReturnToPool) trên VFX prefab
        }

        private void SpawnDeathVFX()
        {
            if (deathVfxPrefab == null || PoolManager.Instance == null) return;
            PoolManager.Instance.Get(deathVfxPrefab, transform.position);
        }

        private void SpawnSkillCastVFX()
        {
            if (skillCastVfxPrefab == null || PoolManager.Instance == null) return;
            PoolManager.Instance.Get(skillCastVfxPrefab, transform.position);
        }
    }
}
