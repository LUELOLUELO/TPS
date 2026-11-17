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
            LocalTransform sphereTransform = SystemAPI.GetComponent<LocalTransform>(sphere);
            AsAgentComponent ag = state.EntityManager.GetComponentData<AsAgentComponent>(prefab);
            ag.stoppingDistance = 3;
            ag.moveSpeed = 10;
            ag.turnSpeed = 10;
            SystemAPI.SetComponent(prefab,ag);
            hasSpwaned = true;
            NativeArray<int> distances = new NativeArray<int>(2, Allocator.Temp);
            NativeArray<int> numbers = new NativeArray<int>(2, Allocator.Temp);
            distances[0] = 10;
            distances[1] = 11;
            numbers[0] = 100;
            numbers[1] = 100;
            for (int i = 0; i < distances.Length; i++)
            {
                int distance = distances[i];
                int number = numbers[i];
                for (int j = 0; j < number; j++)
                {
                    float angle = (math.PI*2)*j/number;
                    float3 spawnPos = +new float3(sphereTransform.Position.x+math.cos(angle)*distance, 1, sphereTransform.Position.z+math.sin(angle)*distance);
                    LocalTransform transform = new LocalTransform()
                    {
                        Position = spawnPos,
                        Rotation = quaternion.LookRotationSafe(-spawnPos, Vector3.up),
                        Scale = 1,
                    };
                    AsPathFindingRequester request = new AsPathFindingRequester()
                    {
                        RequestEntity = Entity.Null,
                        RequestGiven = false,
                        Destination = spawnPos + (transform.Forward() * distance * 2),
                    };
                    AsConstantTargetFollower follower = new AsConstantTargetFollower
                    {
                        target = sphere,
                    };
                    Entity spawn = ecb.Instantiate(prefab);
                    ecb.SetComponent(spawn, transform);
                    ecb.SetComponent(spawn, request);
                    ecb.SetComponent(spawn, follower);
                }
            }
            //UnityEngine.Debug.Log("生成follower");
            distances.Dispose();
            numbers.Dispose();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}