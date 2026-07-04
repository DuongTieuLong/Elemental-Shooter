using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MeleeConfig", menuName = "Weapon/Melee Config")]
public class MeleeConfigData : ScriptableObject
{
    [System.Serializable]
    public struct MeleeElementVisual
    {
        public ElementType element;
        public GameObject slashVFXPrefab;  // Hiệu ứng đường chém (slash trail) tương ứng với hệ
        public GameObject hitVFXPrefab;    // Hiệu ứng nổ khi chém trúng địch
        public AudioClip customHitSFX;      // Âm thanh khi chém trúng (ví dụ tiếng lửa bùng, sét giật)
    }

    public List<MeleeElementVisual> elementVisuals;
    public GameObject defaultSlashVFXPrefab;
    public AudioClip defaultHitSFX;

    public MeleeElementVisual GetVisualForElement(ElementType element)
    {
        return elementVisuals.Find(v => v.element == element);
    }
}
