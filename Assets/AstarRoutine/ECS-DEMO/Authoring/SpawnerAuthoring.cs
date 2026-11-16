using AstarRoutine.ECS_DEMO.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AstarRoutine.ECS_DEMO
{
    public class SpawnerAuthoring:MonoBehaviour
    {
        public GameObject prefab;
        public int spawnCount;
        public float spawnRadius;
        public class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity,new SpawnerComponent()
                {
                    spawnePrefab = GetEntity(authoring.prefab,TransformUsageFlags.Dynamic),
                    spawnCount = authoring.spawnCount,
                    hasSpawned = false,
                    radius = authoring.spawnRadius,
                });
                

                
            }
        }
    }
}