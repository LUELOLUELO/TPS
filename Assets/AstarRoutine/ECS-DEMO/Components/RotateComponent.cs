using Unity.Entities;

namespace AstarRoutine.ECS_DEMO.Components
{
    public struct RotateComponent:IComponentData
    {
        public float radiansPerSecond;
    }
}