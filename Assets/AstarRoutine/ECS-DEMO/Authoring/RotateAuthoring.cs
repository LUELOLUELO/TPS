using AstarRoutine.ECS_DEMO.Components;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

namespace AstarRoutine.ECS_DEMO
{
    public class RotateAuthoring:MonoBehaviour
    {
        public float rotationSpeed;

        public class RotateBaker : Baker<RotateAuthoring>
        {
            public override void Bake(RotateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new RotateComponent
                {
                    radiansPerSecond =  authoring.rotationSpeed,
                });
            }
        }
    }
}