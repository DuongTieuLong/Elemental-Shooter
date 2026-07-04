using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CardUpgrade _cardData;
    private int _level;

    public void Setup(CardUpgrade cardData, int level)
    {
        _cardData = cardData;
        _level = level;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardData == null || TooltipUI.Instance == null) return;

        string title = $"{_cardData.upgradeName} (Lv.{_level})";
        string desc = _cardData.description; // Giả sử CardUpgrade có trường description hoặc bạn có thể generate từ mảng changes

        // Nếu bạn muốn tự tạo description từ mảng changes:
        if (string.IsNullOrEmpty(desc) && _cardData.changes != null)
        {
            desc = "";
            foreach (var change in _cardData.changes)
            {
                string sign = change.value >= 0 ? "+" : "";
                string typeStr = change.modType == ModifierType.Percent ? "%" : "";
                desc += $"{change.type}: {sign}{change.value}{typeStr}\n";
            }
        }

        TooltipUI.Instance.ShowTooltip(title, desc, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }
}
