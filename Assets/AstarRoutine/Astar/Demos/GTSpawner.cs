using Unity.Entities;
using UnityEngine;

namespace AstarRoutine.Astar.Demos
{
    public struct AsAgentPrefabComponent : IComponentData
    {
        public Entity prefab;
    }
    public class GTSpawner:MonoBehaviour
    {
        public GameObject prefab;
        public class GTSpawnerBaker : Baker<GTSpawner>
        {
            public override void Bake(GTSpawner authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                var spawner = GetEntity(authoring.prefab,TransformUsageFlags.Dynamic);
                AddComponent(entity, new AsAgentPrefabComponent ()
                {
                    prefab = spawner,
                });
            }
        }
    }
}