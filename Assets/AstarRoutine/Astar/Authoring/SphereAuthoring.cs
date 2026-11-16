using AstarRoutine.Astar.Components;
using Unity.Entities;
using UnityEngine;

namespace AstarRoutine.Astar.Authoring
{
    public class SphereAuthoring:MonoBehaviour
    {
        public class SphereBaker : Baker<SphereAuthoring>
        {
            public override void Bake(SphereAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ATestSphere());
                Debug.Log("Sphere Baker");
            }
        }
    }
}