using Unity.Entities;

namespace AstarRoutine.Astar.Components
{
    /// <summary>
    /// 与AObstacle不同：AObstacle表示不可通行，挂APenalty可以通行但是有代价
    /// </summary>
    public struct APenalty: IComponentData
    {
        public int Penalty;
    }
}