using AstarRoutine.Astar.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace AstarRoutine.Astar.Authoring
{
    public class AsAgentAuthoring:MonoBehaviour
    {
        public float moveSpeed;
        public float radius;
        public float turnSpeed;
        public float height;
        public float stoppingDistance;//距离目的地多远算抵达终点
        public bool hasInitialDestination;
        public class AsAgentBaker : Baker<AsAgentAuthoring>
        {
            public override void Bake(AsAgentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AsAgentComponent()
                {
                    moveSpeed = authoring.moveSpeed,
                    radius = authoring.radius,
                    turnSpeed = authoring.turnSpeed,
                    height = authoring.height,
                    stoppingDistance = authoring.stoppingDistance,
                    hasInitialDestination =  authoring.hasInitialDestination,
                    currentVelocity = new float3(0,0,0)
                });
            }
        }
    }
}