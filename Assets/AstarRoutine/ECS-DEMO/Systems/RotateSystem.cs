using AstarRoutine.ECS_DEMO.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace AstarRoutine.ECS_DEMO.Systems
{
    [BurstCompile]
    public partial struct RotateSystem:ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<RotateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (transform, rotateComp)in SystemAPI.Query<RefRW<LocalTransform>,RefRO<RotateComponent>>())
            {
                transform.ValueRW = transform.ValueRO.RotateY(deltaTime*rotateComp.ValueRO.radiansPerSecond);
            }
        }
    }
}