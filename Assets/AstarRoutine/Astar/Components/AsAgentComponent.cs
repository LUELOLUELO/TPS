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
    /// <summary>
    /// 路径跟随组件，记录代理沿路径移动的状态。
    /// 与 ASPathBufferComponent 配合使用，追踪当前目标路径点索引
    /// </summary>
    public struct ASPathFollower : IComponentData
    {
        /// <summary>是否有可用路径（路径规划成功后由系统置为 true）</summary>
        public bool PathAvailable;

        /// <summary>当前追踪的路径点索引（在 ASPathBufferComponent 中的位置）</summary>
        public int TargetIndex;

        /// <summary>是否已到达最终目的地</summary>
        public bool DestinationReached;
    }
    public struct AsConstantTargetFollower:IComponentData
    {
        public Entity target;
    }
    public struct AsPathFindingRequester:IComponentData
    {
        public float3 Destination;
        public bool RequestGiven;
        /// <summary>
        /// 对应的寻路操作 Entity（由 PathfindingRequestSystem 自动创建和管理）。
        /// 不要手动设置此值
        /// </summary>
        public Entity RequestEntity;
    }
}