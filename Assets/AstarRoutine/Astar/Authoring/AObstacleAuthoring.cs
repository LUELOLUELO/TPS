using AstarRoutine.Astar.Components;
using Unity.Entities;
using UnityEngine;

namespace AstarRoutine.Astar.Authoring
{
    public class AObstacleAuthoring:MonoBehaviour
    {
        public int Penalty;

        public class ObstacleBaker : Baker<AObstacleAuthoring>
        {
            public override void Bake(AObstacleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AObstacle()
                {
                    Penalty = authoring.Penalty,
                });
            }
        }
    }
}