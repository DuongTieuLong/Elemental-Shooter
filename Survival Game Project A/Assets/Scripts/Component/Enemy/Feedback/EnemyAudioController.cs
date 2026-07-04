using UnityEngine;

namespace Component.Enemy.Feedback
{
    [RequireComponent(typeof(AudioSource))]
    public class EnemyAudioController : MonoBehaviour
    {
        private AudioSource _audioSource;
        private EnemyController _enemyController;
        private EnemyHealth _enemyHealth;

        [Header("Audio Clips")]
        public AudioClip attackClip;
        public AudioClip hitClip;
        public AudioClip deathClip;
        public AudioClip skillCastClip;

        [Header("Settings")]
        [Range(0f, 1f)] public float volume = 0.8f;
        public bool randomPitch = true;
        [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
        [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _enemyController = GetComponentInParent<EnemyController>();
            _enemyHealth = GetComponentInParent<EnemyHealth>();
            
            // Cấu hình cơ bản cho AudioSource
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f; // 3D sound nếu cần, hoặc 0f cho 2D
        }

        private void OnEnable()
        {
            if (_enemyController != null)
            {
                _enemyController.OnAttackExecuteEvent += PlayAttackSound;
                _enemyController.OnSkillCastEvent += PlaySkillCastSound;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.onHit += PlayHitSound;
                _enemyHealth.onDeath += PlayDeathSound;
            }
        }

        private void OnDisable()
        {
            if (_enemyController != null)
            {
                _enemyController.OnAttackExecuteEvent -= PlayAttackSound;
                _enemyController.OnSkillCastEvent -= PlaySkillCastSound;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.onHit -= PlayHitSound;
                _enemyHealth.onDeath -= PlayDeathSound;
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;
            
            if (randomPitch)
            {
                _audioSource.pitch = Random.Range(minPitch, maxPitch);
            }
            else
            {
                _audioSource.pitch = 1f;
            }

            _audioSource.PlayOneShot(clip, volume);
        }

        private void PlayAttackSound() => PlayClip(attackClip);
        private void PlayHitSound() => PlayClip(hitClip);
        private void PlayDeathSound() => PlayClip(deathClip);
        private void PlaySkillCastSound() => PlayClip(skillCastClip);
    }
}
