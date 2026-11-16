using Unity.Entities;

namespace AstarRoutine.Astar.Components
{
    /// <summary>
    /// 障碍物表示不可通行
    /// </summary>
    public struct AObstacle:IComponentData
    {
        /// <summary>
        /// 障碍物上的惩罚值，我们希望寻路路径越远离障碍物越好，如果不加这个，最终寻路路径可能会贴着障碍物走
        /// </summary>
        public int Penalty;
    }
}