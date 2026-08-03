using System.Collections;
using UnityEngine;

public class WeaponFire : MonoBehaviour
{

    private float _fireTimer = -1f; // -1 nghĩa là dùng thời gian mặc định từ weaponstats
    private bool canFire = true;
    private Coroutine fireCooldownCoroutine;
    private Weaponstats weaponstats;
    private Weapon weapon;

    private void Awake()
    {
        weaponstats = GetComponent<Weaponstats>();
        weapon = GetComponent<Weapon>();
    }

    private void OnEnable()
    {
        weapon.OnWeaponChanged += OnWeaponChanged;
    }

    /// <summary>
    /// Gọi hàm này để bắn. Trả về true nếu bắn thành công và bắt đầu đếm cooldown.
    /// </summary>
    public bool TryFire()
    {
        if (!canFire) return false;

        // Bắn thành công -> Khóa bắn và bắt đầu đếm ngược
        canFire = false;

        // Tính thời gian cooldown
        float cooldownTime = GetCurrentCooldownTime();

        // Bắt đầu Coroutine đếm ngược
        fireCooldownCoroutine = StartCoroutine(StartFireCooldown(cooldownTime));

        return true;
    }



    private void OnWeaponChanged(WeaponData newWeaponData)
    {
        // Khi vũ khí thay đổi, reset lại cooldown
        canFire = true;
        _fireTimer = 0f; // Reset về mặc định từ weaponstats
        if (fireCooldownCoroutine != null)
        {
            StopCoroutine(fireCooldownCoroutine);
            fireCooldownCoroutine = null;
        }
    }

    private IEnumerator StartFireCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);

        canFire = true;
        _fireTimer = 0f; // Reset về mặc định sau khi đếm xong
        fireCooldownCoroutine = null;
    }

    private float GetCurrentCooldownTime()
    {
        float attackSpeed = weaponstats.GetFinalAttackSpeed();
        _fireTimer = attackSpeed > 0 ? 1f / attackSpeed : 1f; // Nếu attackSpeed <= 0, đặt cooldown mặc định là 1 giây
        return _fireTimer;
    }
}



