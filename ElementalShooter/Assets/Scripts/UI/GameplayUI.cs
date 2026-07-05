using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [Header("Wave UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private CanvasGroup waveProgressGroup; // Dùng để fade out thanh tiến trình
    [SerializeField] private Slider waveSlider;
    [SerializeField] private GameObject bossHpPanel; // Panel chứa thanh máu Boss

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI killCountText;

    [Header("Combo UI")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private float punchScaleAmount = 1.5f;
    [SerializeField] private float punchDuration = 0.1f;
    
    private Coroutine punchCoroutine;
    private Coroutine bossWaveTransitionCoroutine;

    private void OnEnable()
    {
        GameEvents.OnWaveProgressChanged += HandleWaveProgressChanged;
        GameEvents.OnPlayTimeTicked += HandlePlayTimeTicked;
        GameEvents.OnKillCountChanged += HandleKillCountChanged;
        GameEvents.OnComboUpdated += HandleComboUpdated;
        GameEvents.OnWaveStarted += HandleWaveStarted;
        GameEvents.OnWaveEnded += HandleWaveEnded;

        // Init mặc định
        if (comboText != null) comboText.text = "";
        
        if (bossHpPanel != null) bossHpPanel.SetActive(false);
        if (waveProgressGroup != null) waveProgressGroup.alpha = 1f;
    }

    private void OnDisable()
    {
        GameEvents.OnWaveProgressChanged -= HandleWaveProgressChanged;
        GameEvents.OnPlayTimeTicked -= HandlePlayTimeTicked;
        GameEvents.OnKillCountChanged -= HandleKillCountChanged;
        GameEvents.OnComboUpdated -= HandleComboUpdated;
        GameEvents.OnWaveStarted -= HandleWaveStarted;
        GameEvents.OnWaveEnded -= HandleWaveEnded;
    }

    private void HandleWaveProgressChanged(float progress, int currentWave)
    {
        if (waveText != null)
        {
            waveText.SetText("WAVE {0}", currentWave);
        }
        
        if (waveSlider != null)
        {
            waveSlider.value = progress;
        }
    }

    private void HandleWaveStarted(WaveType type)
    {
        if (type == WaveType.Boss)
        {
            if (waveText != null) waveText.text = "BOSS WAVE!";
            
            if (bossWaveTransitionCoroutine != null) StopCoroutine(bossWaveTransitionCoroutine);
            bossWaveTransitionCoroutine = StartCoroutine(TransitionBossWaveRoutine(true));
        }
        else if (type == WaveType.KillBased)
        {
            // Có thể đổi text thành "FINAL WAVE" hoặc gì đó tuỳ ý
            // Ở đây giữ logic gọi HandleWaveProgressChanged cho nó ghi đè "WAVE X"
        }
    }

    private void HandleWaveEnded(WaveType type)
    {
        if (type == WaveType.Boss)
        {
            if (bossWaveTransitionCoroutine != null) StopCoroutine(bossWaveTransitionCoroutine);
            bossWaveTransitionCoroutine = StartCoroutine(TransitionBossWaveRoutine(false));
        }
    }

    private IEnumerator TransitionBossWaveRoutine(bool isStarting)
    {
        CanvasGroup bossCanvasGroup = bossHpPanel != null ? bossHpPanel.GetComponent<CanvasGroup>() : null;
        
        if (isStarting)
        {
            // 1. Fade out wave progress bar
            if (waveProgressGroup != null)
            {
                yield return StartCoroutine(FadeGroup(waveProgressGroup, 0f, 1f));
            }
            
            // 2. Hiện và Fade in boss hp
            if (bossHpPanel != null && bossCanvasGroup != null)
            {
                bossHpPanel.SetActive(true);
                yield return StartCoroutine(FadeGroup(bossCanvasGroup, 1f, 1f));
            }
            else if (bossHpPanel != null)
            {
                bossHpPanel.SetActive(true);
            }
        }
        else
        {
            // 1. Fade out boss hp
            if (bossHpPanel != null && bossCanvasGroup != null)
            {
                yield return StartCoroutine(FadeGroup(bossCanvasGroup, 0f, 1f));
                bossHpPanel.SetActive(false);
            }
            else if (bossHpPanel != null)
            {
                bossHpPanel.SetActive(false);
            }
            
            // 2. Fade in wave progress bar
            if (waveProgressGroup != null)
            {
                yield return StartCoroutine(FadeGroup(waveProgressGroup, 1f, 1f));
            }
        }
    }

    private IEnumerator FadeGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        group.alpha = targetAlpha;
    }

    private void HandlePlayTimeTicked(int seconds)
    {
        if (playTimeText != null)
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            playTimeText.text = $"{minutes:00}:{remainingSeconds:00}";
        }
    }

    private void HandleKillCountChanged(int killCount)
    {
        if (killCountText != null)
        {
            killCountText.SetText("KILLS: {0}", killCount);
       
        }
    }

    private void HandleComboUpdated(int comboCount)
    {
        if (comboText == null) return;

        if (comboCount > 1) // Chỉ hiển thị combo khi từ 2 hit trở lên
        {
            comboText.SetText("{0} HITS!", comboCount);

            // Đổi màu rực hơn khi combo cao
            if (comboCount > 50) comboText.color = Color.red;
            else if (comboCount > 20) comboText.color = new Color(1f, 0.5f, 0f); // Cam
            else comboText.color = Color.white;

            // Chạy hiệu ứng giật UI
            if (punchCoroutine != null) StopCoroutine(punchCoroutine);
            punchCoroutine = StartCoroutine(PunchScaleRoutine());
        }
        else
        {
            comboText.text = "";
        }
    }

    private IEnumerator PunchScaleRoutine()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * punchScaleAmount;
        float elapsed = 0f;

        // Phóng to lên
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Thu nhỏ lại
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            comboText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        comboText.transform.localScale = originalScale;
    }
}
