using UnityEngine;

public interface IAttackBehaviour
{
    public void BeginAttack();
    public void Tick(float deltaTime);

}
