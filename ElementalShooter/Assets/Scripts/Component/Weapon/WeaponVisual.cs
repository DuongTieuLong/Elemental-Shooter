using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    public Transform muzzle;
    public SpriteRenderer leftHandRenderer;

    public SpriteRenderer weaponRenderer;

    private Weapon weapon;

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
    }

    private void OnEnable()
    {
        weapon.OnWeaponChanged += EquipWeapon;
    }

    private void OnDisable()
    {
        weapon.OnWeaponChanged -= EquipWeapon;
    }

    public void EquipWeapon(WeaponData newData)
    {
        weaponRenderer.sprite = newData.weaponIcon;
        leftHandRenderer.sortingOrder = newData.gripStyle == WeaponGripStyle.TwoHanded ? 4 : 0; // Đặt sorting order dựa trên kiểu cầm
    }

}
