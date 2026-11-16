using AstarRoutine.ECS_DEMO.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AstarRoutine.ECS_DEMO
{
    public class SpawnerAuthoring:MonoBehaviour
    {
        public GameObject prefab;
        public float rotateSpeed;
        public float3 moveSpeed;
        public class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new RotateComponent
                {
                    radiansPerSecond = authoring.rotateSpeed
                });
                AddComponent(entity,new MoveSpeedComponent()
                {
                    moveSpeed = authoring.moveSpeed
                });
            }
        }
    }
}