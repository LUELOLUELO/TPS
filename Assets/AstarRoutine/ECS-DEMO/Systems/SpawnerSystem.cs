using AstarRoutine.ECS_DEMO.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AstarRoutine.ECS_DEMO.Systems
{
    public partial struct SpawnerSystem:ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<SpawnerComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            
            EntityManager entityManager = systemState.EntityManager;
            if (entityManager == null)
            {
                return;
            }
            foreach (var (SpawnerComp, entity) in SystemAPI.Query<RefRW<SpawnerComponent>>().WithEntityAccess())
            {
                if (SpawnerComp.ValueRW.hasSpawned)
                    continue;
                int count = SpawnerComp.ValueRO.spawnCount;
                float radius = SpawnerComp.ValueRW.radius;
                //Debug.Log($"准备生成{count}个方块");
                for (int i = 0; i < count; i++)
                {
                    Entity prefab = entityManager.Instantiate(SpawnerComp.ValueRO.spawnePrefab);
                    float angle = UnityEngine.Random.Range(0, 2*Mathf.PI);
                    float r = math.sqrt(UnityEngine.Random.Range(0f, radius * radius));
                    float  x = r*Mathf.Cos(angle);
                    float  z = r*Mathf.Sin(angle);
                    
                    entityManager.SetComponentData(prefab, new LocalTransform()
                    {
                        Position = new float3(x,0,z),
                        Rotation = quaternion.identity,
                        Scale = 1,
                    });

                }
                SpawnerComp.ValueRW.hasSpawned = true;
                //Debug.Log($"共生成{count}个方块");
            }
            
        }
    }
}