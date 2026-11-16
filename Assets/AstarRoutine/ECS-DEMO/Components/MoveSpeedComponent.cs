using Unity.Entities;
using Unity.Mathematics;

namespace AstarRoutine.ECS_DEMO.Components
{
    public struct MoveSpeedComponent:IComponentData
    {
        public float3 moveSpeed;
    }
}