using UnityEngine;

namespace Component.Enemy.Feedback
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private EnemyController _enemyController;
        private EnemyHealth _enemyHealth;
        private SpriteRenderer _spriteRenderer;

        // Animator Hashes for performance
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DieHash = Animator.StringToHash("Die");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _enemyController = GetComponentInParent<EnemyController>();
            _enemyHealth = GetComponentInParent<EnemyHealth>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (_enemyController != null)
            {
                _enemyController.OnAttackEvent += HandleAttack;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.onHit += HandleHit;
                _enemyHealth.onDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_enemyController != null)
            {
                _enemyController.OnAttackEvent -= HandleAttack;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.onHit -= HandleHit;
                _enemyHealth.onDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            if (_enemyController == null || _enemyHealth == null || _enemyHealth.isDead) return;

            // Update movement state
            bool isMoving = _enemyController.Rb.linearVelocity.sqrMagnitude > 0.1f;
            _animator.SetBool(IsMovingHash, isMoving);

        }

        private void HandleAttack()
        {
            if (_enemyHealth != null && _enemyHealth.isDead) return;
            _animator.SetTrigger(AttackHash);
        }

        // Called by Unity Animation Event
        public void TriggerAttackExecute()
        {
            if (_enemyHealth != null && _enemyHealth.isDead) return;
            if (_enemyController != null)
            {
                _enemyController.OnAttackExecuteEvent?.Invoke();
            }
        }

        private void HandleHit()
        {
            if (_enemyHealth != null && _enemyHealth.isDead) return;
            _animator.SetTrigger(HitHash);
        }

        private void HandleDeath()
        {
            _animator.SetBool(DieHash, true); // Use bool for death in case trigger gets consumed
            _animator.SetTrigger(DieHash);
        }

        // Called by Unity Animation Event at the end of death animation
        public void TriggerDeathComplete()
        {
            if (_enemyController != null)
            {
                Debug.Log("Death Animation Complete");
                _enemyController.CompleteDeath();
            }
        }

    }
}
