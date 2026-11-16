using AstarRoutine.Astar.Components;
using Unity.Entities;
using UnityEngine;

namespace AstarRoutine.Astar.Authoring
{
    public class APenaltyAuthoring:MonoBehaviour
    {
        public int Penalty;

        public class APenaltyBaker : Baker<AObstacleAuthoring>
        {
            public override void Bake(AObstacleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new APenalty()
                {
                    Penalty = authoring.Penalty,
                });
            }
        }
    }
}