using Unity.Entities;
using Unity.Mathematics;

namespace AstarRoutine.Astar.Components
{
    public struct AsGrid:IComponentData
    {
        public int nodeSize;
        public int nodeXCount;
        public int nodeYCount;
        public bool refreshGrid;
        public bool refreshIsland;
        
        public int InaccessibleLMIndex;
        public int PenaltyLMIndex;

        public int blurSize;
        public DifferentIslandPolicy differentIslandPolicy;
    }

    public struct AsPathFindingParameters:IComponentData
    {
        /// <summary>
        /// 每帧处理的最大寻路请求数量
        /// </summary>
        public int maxPathfingOpsPerFrame;
    }
    public struct AsPathFindingOperationsBuffer:IBufferElementData
    {
        public Entity requestEntity;
        public AsPathFindingOperations operation;
    }

    public struct AsPathFindingOperations:IComponentData
    {
        public float3 startPoint;
        public float3 endPoint;
    }

    /// <summary>
    /// 当起点终点分不同岛屿时的策略
    /// </summary>
    public enum DifferentIslandPolicy
    {
        DontPath = 1,//不寻路
        GotoClose = 2,//去距离目标岛屿最近的地方
    }

    public struct AsPathFindingResult:IComponentData
    {
        public bool SearchFinished;
        public bool RouteFinished;
    }

    public struct AsPathBufferComponent : IBufferElementData
    {
        public float3 routinePoint;
    }
}