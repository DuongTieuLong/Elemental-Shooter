using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public Image yellowFillImage; // Kéo YellowFill vào đây
    public CanvasGroup canvasGroup; // THÊM: Kéo CanvasGroup của UI này vào đây để ẩn/hiển mượt mà

    [Header("Settings")]
    public float smoothSpeed = 0.05f; // Tốc độ tụt của thanh vàng
    public float delayBeforeFall = 0.5f; // Thời gian chờ trước khi thanh vàng bắt đầu tuột
    public float durationToShow = 3f; // THÊM: Thời gian hiển thị sau khi nhận dame (3 giây)

    private Coroutine yellowBarCoroutine;
    private Coroutine autoHideCoroutine; // THÊM: Quản lý đếm ngược thời gian ẩn
    private Transform targetEnemy;
    private EnemyStatusUI statusUI;
    private Camera mainCam;

    public Vector3 offset = new Vector3(); // Khoảng cách trên đầu kẻ địch 

    private void Awake()
    {
        statusUI = GetComponent<EnemyStatusUI>();

        // Tự động lấy CanvasGroup nếu quên không kéo thả trong Inspector
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        mainCam = Camera.main;
    }

    /// <summary>
    /// Thiết lập mục tiêu kẻ địch để bám đuôi và truyền tham chiếu cho StatusUI.
    /// </summary>
    public void SetupTarget(Transform target)
    {
        targetEnemy = target;
        if (statusUI != null)
        {
            statusUI.SetupTarget(target);
        }

        // Reset slider hiển thị ngay lập tức để tránh nhấp nháy chỉ số cũ của quái trước
        if (healthSlider != null && yellowFillImage != null)
        {
            healthSlider.value = healthSlider.maxValue;
            yellowFillImage.fillAmount = 1f;
        }

        // Mặc định ban đầu sinh ra: Ẩn thanh máu đi (chỉ hiện khi bị đánh)
        SetUIAlpha(0f);
    }

    private void LateUpdate()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy) return;

        // Bỏ qua việc update RectTransform nếu thanh máu đang tàng hình (tiết kiệm cực nhiều CPU UpdateCanvas)
        if (canvasGroup != null && canvasGroup.alpha == 0f) return;

        // Chuyển đổi vị trí từ World Space sang Screen Space 
        if (mainCam != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(targetEnemy.position + offset); // 

            // Check nếu nằm trong phạm vi nhìn của camera (2D)
            if (screenPos.z > 0)
            {
                transform.position = screenPos; // [cite: 161]
            }
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth, bool forceShow = true)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        // CHỈ hiện thanh máu lên và đếm ngược ẩn đi nếu forceShow được cho phép
        if (forceShow)
        {
            SetUIAlpha(1f);

            if (gameObject.activeInHierarchy)
            {
                if (autoHideCoroutine != null) StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = StartCoroutine(AutoHideRoutine());
            }
        }

        // Hiệu ứng thanh vàng vẫn chạy bình thường để đồng bộ chỉ số
        if (gameObject.activeInHierarchy && yellowFillImage != null)
        {
            if (yellowBarCoroutine != null) StopCoroutine(yellowBarCoroutine);

            float targetRatio = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            yellowBarCoroutine = StartCoroutine(SmoothYellowBar(targetRatio));
        }
    }

    private IEnumerator SmoothYellowBar(float targetFillAmount)
    {
        if (yellowFillImage == null) yield break;

        // Đợi một chút trước khi thanh vàng bắt đầu chạy [cite: 116]
        yield return new WaitForSeconds(delayBeforeFall);

        // Đuổi theo giá trị của thanh máu chính
        while (Mathf.Abs(yellowFillImage.fillAmount - targetFillAmount) > 0.001f)
        {
            yellowFillImage.fillAmount = Mathf.Lerp(yellowFillImage.fillAmount, targetFillAmount, smoothSpeed);
            yield return null;
        }
        yellowFillImage.fillAmount = targetFillAmount;
        yellowBarCoroutine = null;
    }

    // THÊM: Coroutine đếm ngược thời gian không nhận dame
    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSeconds(durationToShow);

        // Hiệu ứng mờ dần (Fade out) thay vì ẩn cái rụp để trông đẹp hơn
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float rate = 1.0f / 0.3f; // Thời gian fade out là 0.3 giây
        float progress = 0.0f;

        while (progress < 1.0f)
        {
            progress += Time.deltaTime * rate;
            SetUIAlpha(Mathf.Lerp(startAlpha, 0f, progress));
            yield return null;
        }

        SetUIAlpha(0f);
        autoHideCoroutine = null;
    }

    // THÊM: Hàm Helper để thay đổi độ hiển thị của UI an toàn
    private void SetUIAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            // Nếu ẩn hoàn toàn (alpha = 0) thì chặn luôn tương tác chuột nếu có
            canvasGroup.blocksRaycasts = (alpha > 0f);
        }
    }

    /// <summary>
    /// Dọn dẹp trạng thái và dừng các Coroutine trước khi trả về Pool. [cite: 145]
                /// </summary>
    public void ResetUI()
    {
        if (yellowBarCoroutine != null)
        {
            StopCoroutine(yellowBarCoroutine); // [cite: 145]
            yellowBarCoroutine = null;
        }

        // THÊM: Dừng bộ đếm ẩn khi trả về pool
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        targetEnemy = null;

        if (statusUI != null)
        {
            statusUI.ResetStatusUI();
        }

        if (healthSlider != null)
        {
            healthSlider.value = healthSlider.maxValue;
        }

        if (yellowFillImage != null)
        {
            yellowFillImage.fillAmount = 1f;
        }

        // Đặt alpha về lại mặc định
        SetUIAlpha(1f);
    }
}