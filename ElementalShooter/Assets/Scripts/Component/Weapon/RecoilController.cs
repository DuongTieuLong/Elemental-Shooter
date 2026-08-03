using UnityEngine;

public class RecoilController : MonoBehaviour
{
    private WeaponAim weaponAim; // Tham chiếu đến WeaponAim để áp dụng recoil
    private Weapon weapon; // Tham chiếu đến Weapon để lấy dữ liệu vũ khí


    private void Awake()
    {
        weaponAim = GetComponent<WeaponAim>();
        weapon = GetComponent<Weapon>();
    }

    public void ApplyRecoil(float currentRecoil)
    {
        float baseSpread = 0f;
        var rangedData = weapon.CurrentData.attackData as RangedStrategyData;
        if (rangedData != null)
        {
            baseSpread = rangedData.bulletSpread;
        }

        Debug.Log("recoil Multiplier: " + currentRecoil);
        Debug.Log("recoil force: " + rangedData.recoilForce);
        if (rangedData != null && weaponAim != null)
        {
            weaponAim.TriggerRecoil(rangedData.recoilForce *
                currentRecoil, rangedData.recoilRecovery *
                currentRecoil);
        }
    }
}
