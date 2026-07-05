using UnityEngine;

// Kế thừa cả 2 interface
public class DamageReceiver : MonoBehaviour, IDamageable, IStatusReceiver
{
    private IDamageable _mainDamageable;
    private IStatusReceiver _mainStatusReceiver;

    private void Awake()
    {
        if (transform.parent != null)
        {
            // Cache lại cả 2 component từ Object cha
            _mainDamageable = transform.parent.GetComponentInParent<IDamageable>();
            _mainStatusReceiver = transform.parent.GetComponentInParent<IStatusReceiver>();
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        _mainDamageable?.TakeDamage(info);
        Debug.Log("HIt");
    }

    public void ReceiveElement(ElementData data)
    {
        // Chuyển tiếp hiệu ứng nguyên tố lên cha
        _mainStatusReceiver?.ReceiveElement(data);
    }
}