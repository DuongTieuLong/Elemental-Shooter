using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip bossBgmClip; // Nhạc Boss
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;

    [Header("SFX Clips")]
    public AudioClip shootClip;
    public AudioClip hitClip;
    public AudioClip enemyDeathClip;
    public AudioClip explosionClip;
    public AudioClip upgradeSelectClip;
    public AudioClip gameOverClip;
    public AudioClip victoryClip;

    [Header("SFX Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 25;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;

    private List<AudioSource> sfxPool = new List<AudioSource>();
    private Transform poolContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXPool();
            SetupBGM();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Tự động lắng nghe các sự kiện để phát âm thanh mà không cần gọi thủ công
        GameEvents.OnPlayerSpawned += StartBGM;
        GameEvents.OnPlayerDeath += PlayGameOverSound;
        GameEvents.OnVictory += PlayVictorySound;
        GameEvents.OnEnemyKilled += PlayEnemyDeathSound;
        GameEvents.OnUpgradeSelected += PlayUpgradeSound;
        GameEvents.OnDamageDealt += PlayHitSound;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSpawned -= StartBGM;
        GameEvents.OnPlayerDeath -= PlayGameOverSound;
        GameEvents.OnVictory -= PlayVictorySound;
        GameEvents.OnEnemyKilled -= PlayEnemyDeathSound;
        GameEvents.OnUpgradeSelected -= PlayUpgradeSound;
        GameEvents.OnDamageDealt -= PlayHitSound;
    }

    private void InitializeSFXPool()
    {
        GameObject containerObj = new GameObject("SFX_Pool_Container");
        containerObj.transform.SetParent(transform);
        poolContainer = containerObj.transform;

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject sfxObj = new GameObject("SFX_Source");
        sfxObj.transform.SetParent(poolContainer);
        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 0 = 2D Sound (phù hợp với game 2D này)
        sfxPool.Add(source);
        return source;
    }

    private void SetupBGM()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = bgmVolume;
        bgmSource.clip = bgmClip;
    }

    public void StartBGM(Transform playerTransform)
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.volume = bgmVolume;
            if (!bgmSource.isPlaying || bgmSource.clip != bgmClip)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
                Debug.Log("BGM Started");
            }
        }
    }

    public void PlayBossBGM()
    {
        if (bgmSource != null && bossBgmClip != null)
        {
            bgmSource.clip = bossBgmClip;
            bgmSource.Play();
            Debug.Log("Boss BGM Started");
        }
    }

    public void ResumeNormalBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
            Debug.Log("Normal BGM Resumed");
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, Vector3 position = default, float volumeScale = 1f, float pitchRandomness = 0.12f)
    {
        if (clip == null) return;

        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource != null)
        {
            availableSource.gameObject.transform.position = position;
            availableSource.clip = clip;
            availableSource.volume = sfxVolume * volumeScale;

            // Biến đổi pitch ngẫu nhiên một chút để âm thanh bớt nhàm chán (Game Juice)
            availableSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);

            availableSource.Play();
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Dò tìm nguồn âm thanh đang rảnh rỗi
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                return sfxPool[i];
            }
        }

        // Nếu hết nguồn rảnh rỗi nhưng chưa vượt giới hạn pool, tạo thêm nguồn mới
        if (sfxPool.Count < maxPoolSize)
        {
            return CreateNewAudioSource();
        }

        // Cực chẳng đã, tái sử dụng nguồn âm thanh đầu tiên đang phát
        return sfxPool[0];
    }

    // Các hàm lắng nghe sự kiện tĩnh
    private void PlayEnemyDeathSound(Vector3 deathPos, int expReward)
    {
        PlaySFX(enemyDeathClip, deathPos, 0.9f);
    }

    private void PlayUpgradeSound(CardUpgrade upgrade)
    {
        PlaySFX(upgradeSelectClip, Vector3.zero, 1.0f, 0.05f);
    }

    private void PlayGameOverSound()
    {
        StopBGM();
        PlaySFX(gameOverClip, Vector3.zero, 1.0f, 0f);
    }

    private void PlayVictorySound()
    {
        StopBGM();
        PlaySFX(victoryClip, Vector3.zero, 1.0f, 0f);
    }

    private void PlayHitSound(float damage, Vector3 hitPos, ElementType element)
    {
        // Phản ứng nổ đã phát tiếng nổ lớn riêng trong script nổ,
        // ở đây chỉ phát tiếng trúng đạn vật lý nhỏ cho đạn thường
        if (element != ElementType.None)
        {
            PlaySFX(hitClip, hitPos, 0.45f, 0.15f);
        }
        else
        {
            PlaySFX(hitClip, hitPos, 0.3f, 0.15f);
        }
    }
}
