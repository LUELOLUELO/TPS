using Unity.Entities;

namespace AstarRoutine.ECS_DEMO.Components
{
    public struct SpawnerComponent:IComponentData
    {
        public Entity spawnePrefab;
        public int spawnCount;
        public bool hasSpawned;
        public float radius;
        
    }
}