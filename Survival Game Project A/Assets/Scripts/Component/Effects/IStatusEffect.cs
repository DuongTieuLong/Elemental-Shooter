using UnityEngine;
public interface IStatusEffect
{
    public void Apply(GameObject target, StatusReceiver receiver, float multiplier);
}