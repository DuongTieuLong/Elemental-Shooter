using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardUpgrade", menuName = "ScriptableObjects/CardUpgrade", order = 4)]
public class CardUpgrade : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;
    public List<StatChange> changes; // Thẻ này có thể cộng nhiều thứ cùng lúc
    public ElementType newElement = ElementType.None; // Nếu thẻ này mở khóa hệ mới
    public WeaponData newWeapon; // Nếu thẻ này mở khóa vũ khí mới
    
    [Header("Requirements")]
    public WeaponData requiredWeapon; // Chỉ xuất hiện khi người chơi trang bị vũ khí này (null = xuất hiện toàn cục)

    [Header("Levels")]
    public int maxLevel = 5; // Cấp độ tối đa có thể nâng cấp
}

public enum StatType { MaxHealth, Health, Damage, MoveSpeed, AttackSpeed, Range, ProjectileCount, Weapon }