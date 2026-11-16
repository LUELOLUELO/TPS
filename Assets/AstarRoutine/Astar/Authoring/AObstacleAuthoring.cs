using AstarRoutine.Astar.Components;
using Unity.Entities;
using UnityEngine;

namespace AstarRoutine.Astar.Authoring
{
    public class APenaltyAuthoring:MonoBehaviour
    {
        public int Penalty;

        public class ObstacleBaker : Baker<APenaltyAuthoring>
        {
            public override void Bake(APenaltyAuthoring authoring)
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