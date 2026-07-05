using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Vector3 offset = new Vector3(20, -20, 0);
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(string title, string description, Vector3 position)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;

            // Offset to avoid cursor blocking
        
            tooltipPanel.transform.position = position + offset;
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
