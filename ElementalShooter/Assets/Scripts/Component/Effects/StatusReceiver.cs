using System.Collections.Generic;
using UnityEngine;

public enum ElementType
{
    None,
    Fire,
    Electric,
    Ice,
    Wind
}

public class StatusReceiver : MonoBehaviour, IPoolable, IStatusReceiver
{
    public List<EffectMapping> effectTemplates;
    public ReactionDatabase db;

    private Dictionary<ElementType, ElementData> appliedElements = new();
    private Dictionary<ElementType, IStatusEffect> templateCache;

    public StatusEffectController Controller => controller;
    private StatusEffectController controller;

    private void Awake()
    {
        templateCache = new Dictionary<ElementType, IStatusEffect>();
        foreach (var mapping in effectTemplates)
        {
            if (mapping.effectTemplate is IStatusEffect effect)
                templateCache[mapping.type] = effect;
        }
        controller = GetComponent<StatusEffectController>();
    }

    public void ReceiveElement(ElementData incoming)
    {
        if (incoming.Type == ElementType.None) return;

        // Kiểm tra phản ứng
        foreach (var activeType in appliedElements.Keys)
        {
            var reaction = db.GetReaction(activeType, incoming.Type);
            if (reaction != null)
            {
                // Lấy dữ liệu của nguyên tố cũ đã dính từ trước
                ElementData existing; 
                appliedElements.TryGetValue(activeType, out existing);

                // Thực hiện phản ứng với đầy đủ thông tin của cả 2 nguồn
                reaction.effect.Execute(gameObject, existing, incoming);

                if (reaction.consumeElements) RemoveElementKey(activeType);

                // xóa nguyên tố mới nếu phản ứng có tiêu hao nguyên tố
                ElementType consumed = activeType;
                bool shouldConsume = reaction.consumeElements;
                if (shouldConsume)
                    RemoveElementKey(consumed);
                return;
            }
        }

        appliedElements[incoming.Type] = incoming;
        
        var template = FindTemplate(incoming.Type);
        if (template == null)
        {
            Debug.LogWarning($"[StatusReceiver] No status template found for element: {incoming.Type} on {gameObject.name}! Make sure it is assigned in the effectTemplates list on the Enemy prefab Inspector.");
        }
        else
        {
            Debug.Log($"[StatusReceiver] Applying status effect for element: {incoming.Type} on {gameObject.name} with multiplier: {incoming.Multiplier}");
            template.Apply(gameObject, this, incoming.Multiplier);
        }
    }



    public IStatusEffect FindTemplate(ElementType element)
    {
        return templateCache.TryGetValue(element, out var effect) ? effect : null;
    }



    public void RemoveElementKey(ElementType elementType)
    {
        if(appliedElements.ContainsKey(elementType))
            appliedElements.Remove(elementType);
    }

    public void OnSpawn()
    {
        appliedElements.Clear();
    }

    public void OnDespawn()
    {
        appliedElements.Clear();
    }
}
[System.Serializable]
public struct EffectMapping 
{
    public ElementType type;
    public ScriptableObject effectTemplate; 
}