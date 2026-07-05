using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BulletConfigData", menuName = "Weapon/Bullet Config")]
public class BulletConfigData : ScriptableObject
{
    [System.Serializable]
    public struct ElementVisual
    {
        public ElementType element; // Thay ElementType bằng enum của bạn (Fire, Ice, Lightning,...)
        public GameObject customBulletPrefab; // Prefab đạn riêng cho hệ này (nếu có)
        public GameObject hitVFXPrefab;       // Hiệu ứng khi đạn trúng địch
        public GameObject muzzleVFXPrefab;    // Hiệu ứng tia lửa ở nòng súng khi bắn
    }

    public List<ElementVisual> elementVisuals;
    public GameObject defaultBulletPrefab;
    // Hàm tiện ích để tìm nhanh cấu hình theo Element
    public ElementVisual GetVisualForElement(ElementType element)
    {
        return elementVisuals.Find(v => v.element == element);
    }
}