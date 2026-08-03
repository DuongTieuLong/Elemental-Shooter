using UnityEngine;

public class SpreadCalculator : MonoBehaviour
{
    private PlayerMovement playerMovement; // Tham chiếu đến PlayerMovement để kiểm tra trạng thái di chuyển
    private StatHandler statHandler;
    private Weapon weapon; // Tham chiếu đến Weapon để lấy dữ liệu vũ khí

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        statHandler = GetComponent<StatHandler>();
        weapon = GetComponent<Weapon>();
    }

    public float GetCurrentRecoilMultiplier()
    {
        float recoilMultiplier = 1.0f;
        if (playerMovement != null)
        {
            if (playerMovement.IsSprinting)
            {
                recoilMultiplier = 1.5f;

            }
            else if (!playerMovement.IsMoving)
            {
                recoilMultiplier = 0.5f; // Giảm 50% khi đứng yên
            }
        }

        if (Camera.main != null && UnityEngine.InputSystem.Mouse.current != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            float distanceToMouse = Vector2.Distance(transform.position, mouseWorldPos);
            float range = statHandler.GetValue(StatType.Range); // Lấy giá trị range từ StatHandler)
            if (distanceToMouse > range)
            {
                recoilMultiplier *= 1.3f; // Tăng thêm 30% khi ngắm quá xa            }
            }
        }
        return recoilMultiplier;
    }

    public float GetCurrentSpread()
    {
        float baseSpread = 0f;
        var rangedData = weapon.CurrentData.attackData as RangedStrategyData;
        if (rangedData != null)
        {
            baseSpread = rangedData.bulletSpread;
        }

        return baseSpread * GetCurrentRecoilMultiplier();
    }

}
