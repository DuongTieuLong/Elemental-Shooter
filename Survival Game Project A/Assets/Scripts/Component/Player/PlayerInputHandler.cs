using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private Weapon currentWeapon; // Tham chiếu tới script vũ khí hiện tại
    private InputSystem_Actions playerInput;

    void Awake()
    {
        playerInput = new InputSystem_Actions();
        playerInput.Player.Attack.performed += OnFireStarted;
        playerInput.Player.Attack.canceled += OnFireCanceled;
    }

    private void OnFireStarted(InputAction.CallbackContext context)
    {
        if (currentWeapon != null) currentWeapon.IsPullingTrigger = true;
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        if (currentWeapon != null) currentWeapon.IsPullingTrigger = false;
    }

    private void OnEnable() => playerInput.Player.Enable();
    private void OnDisable() => playerInput.Player.Disable();
}