// Gói tin chứa mọi thứ về cú đánh
using UnityEngine;

public struct DamageInfo
{
    public float Amount;
    public ElementType Element;
    public float ElementMultiplier;

    public DamageInfo(float amount, ElementType element = ElementType.None, float elementMultiplier = 1f)
    {
        Amount = amount;
        Element = element;
        ElementMultiplier = elementMultiplier;
    }
}