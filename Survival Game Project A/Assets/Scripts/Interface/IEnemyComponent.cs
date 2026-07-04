public interface IEnemyComponent
{
    void Initialize(EnemyController controller);
    void OnSpawnComponent();
    void OnDespawnComponent();
    void OnUpdateComponent(float deltaTime);
}
