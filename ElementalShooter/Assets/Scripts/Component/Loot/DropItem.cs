using UnityEngine;

public abstract class DropItem : MonoBehaviour, IPoolable
{
    public LootType lootType;
    public int value;
    public bool isAttracting { get; protected set; }
    public Transform target { get; protected set; }

    [Header("Movement Settings")]
    public float initialSpeed = 2f;
    public float acceleration = 25f;
    public float maxSpeed = 30f;
    public float currentSpeed;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public float baseScale = 1f;

    public virtual void Initialize(int startValue, LootType type)
    {
        value = startValue;
        lootType = type;
        isAttracting = false;
        target = null;
        currentSpeed = 0f;
        transform.localScale = Vector3.one * baseScale;
    }

    public virtual void Merge(int additionalValue)
    {
        value += additionalValue;
        
        // Cập nhật hiển thị khi gộp (to hơn một chút, có thể đổi màu rực rỡ hơn)
        float newScale = transform.localScale.x + 0.1f;
        newScale = Mathf.Min(newScale, baseScale * 2.5f); // Tối đa to gấp 2.5 lần
        transform.localScale = Vector3.one * newScale;
        
        if (spriteRenderer != null)
        {
            // Làm hạt sáng hơn một chút
            Color c = spriteRenderer.color;
            c.r = Mathf.Clamp01(c.r + 0.1f);
            c.g = Mathf.Clamp01(c.g + 0.1f);
            c.b = Mathf.Clamp01(c.b + 0.1f);
            spriteRenderer.color = c;
        }
    }

    public virtual void StartAttracting(Transform playerTarget)
    {
        if (isAttracting) return;
        isAttracting = true;
        target = playerTarget;
        currentSpeed = initialSpeed;
    }

    public abstract void OnCollected();

    public virtual void OnSpawn()
    {
        isAttracting = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; // Reset màu
        }
    }

    public virtual void OnDespawn()
    {
        isAttracting = false;
        target = null;
    }
}
