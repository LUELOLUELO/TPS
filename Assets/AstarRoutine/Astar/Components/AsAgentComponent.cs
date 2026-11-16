using Unity.Entities;
using Unity.Mathematics;

namespace AstarRoutine.Astar.Components
{
    public struct AsAgentComponent:IComponentData
    {
        public float moveSpeed;
        public float radius;
        public float turnSpeed;
        public float height;
        public float stoppingDistance;//距离目的地多远算抵达终点
        public bool hasInitialDestination;
        public float3 currentVelocity;
    }
}