using AstarRoutine.Astar.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AstarRoutine.Astar.Demos
{
    [RequireMatchingQueriesForUpdate]
    public partial struct GTSpawnerSystem:ISystem
    {
        private bool hasSpwaned;
        public void OnUpdate(ref SystemState state)
        {
            if (hasSpwaned)
            {
                return;
            }
            Entity prefab = Entity.Null;
            foreach (var prefabComp in SystemAPI.Query<RefRW<AsAgentPrefabComponent>>())
            {
                prefab = prefabComp.ValueRW.prefab;
            }
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            if (prefab == Entity.Null)
            {
                ecb.Dispose();
                return;
            }

            //获取实体小球作为target
            Entity sphere = SystemAPI.GetSingletonEntity<ATestSphere>();
            AsAgentComponent ag = state.EntityManager.GetComponentData<AsAgentComponent>(prefab);
            ag.stoppingDistance = 3;
            ag.moveSpeed = 10;
            ag.turnSpeed = 10;
            ecb.SetComponent(prefab,ag);
            hasSpwaned = true;
            NativeArray<int> distances = new NativeArray<int>(2, Allocator.Temp);
            NativeArray<int> numbers = new NativeArray<int>(2, Allocator.Temp);
            distances[0] = 10;
            distances[1] = 11;
            numbers[0] = 100;
            numbers[1] = 110;
            for (int i = 0; i < distances.Length; i++)
            {
                int distance = distances[i];
                int number = numbers[i];
                for (int j = 0; j < number; j++)
                {
                    Entity spawn = ecb.Instantiate(prefab);
                    float angle = (Mathf.PI*2)*j/number;
                    float3 spawnPos = new float3(Mathf.Cos(angle)*distance, 1, Mathf.Sin(angle)*distance);
                    ecb.SetComponent(spawn, new LocalTransform()
                    {
                        Position = spawnPos,
                        Rotation = quaternion.LookRotationSafe(-spawnPos, Vector3.up),
                        Scale = 1,
                    });
                }
            }
        }
    }
}