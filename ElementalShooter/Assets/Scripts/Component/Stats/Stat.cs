using System;
using System.Collections.Generic;

public enum ModifierType { Flat, Percent }

public class Stat
{
    public float BaseValue;
    private float _finalValue;
    private bool _isDirty = true;

    private readonly List<StatModifier> _modifiers = new();
    private int _nextModifierId = 0; // ← thêm bộ đếm ID

    public event Action<float, float> OnValueChanged;



    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _finalValue = CalculateFinalValue();
                _isDirty = false;
            }
            return _finalValue;
        }
    }

    public float GetPreviewValue(float amount, ModifierType type)
    {
        float final = BaseValue;
        float sumPercent = 0;

        foreach (var mod in _modifiers)
        {
            if (mod.Type == ModifierType.Flat) final += mod.Value;
            else if (mod.Type == ModifierType.Percent) sumPercent += mod.Value;
        }

        if (type == ModifierType.Flat) final += amount;
        else if (type == ModifierType.Percent) sumPercent += amount;

        return final * (1 + sumPercent);
    }

    public int AddModifier(float amount, ModifierType type) // ← đổi void → int
    {
        int id = _nextModifierId++;
        _modifiers.Add(new StatModifier(id, amount, type));
        var oldValue = Value;
        _isDirty = true;
        OnValueChanged?.Invoke(oldValue, Value);
        return id; // ← trả ID về
    }



    public void RemoveModifier(int id) // ← nhận ID thay vì value
    {
        int index = _modifiers.FindIndex(m => m.Id == id);
        if (index >= 0)
        {
            _modifiers.RemoveAt(index);
            var oldValue = Value;
            _isDirty = true;
            OnValueChanged?.Invoke(oldValue, Value);
        }
    }

    public void ClearAllModifiers()
    {
        var oldValue = Value;
        _modifiers.Clear();
        _isDirty = true;
        OnValueChanged?.Invoke(oldValue, Value);
    }

    private float CalculateFinalValue()
    {
        float final = BaseValue;
        float sumPercent = 0;

        foreach (var mod in _modifiers)
        {
            if (mod.Type == ModifierType.Flat) final += mod.Value;
            else if (mod.Type == ModifierType.Percent) sumPercent += mod.Value;
        }

        // Tính phần trăm dựa trên (Base + Flat)
        return final * (1 + sumPercent);
    }
}

public struct StatModifier
{
    public int Id;
    public float Value;
    public ModifierType Type;

    public StatModifier(int id, float value, ModifierType type)
    {
        Id = id;
        Value = value;
        Type = type;
    }
}

[System.Serializable]
public struct StatChange
{
    public StatType type;
    public ModifierType modType; // Loại modifier: Flat hay Percent
    public float value;
}
