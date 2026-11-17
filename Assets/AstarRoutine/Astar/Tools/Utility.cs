using AstarRoutine.Astar.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AstarRoutine.Astar.Tools
{
    public static class Utility
    {
        public static Vector3 GridPositionToWorldCoord(AsGrid grid,int x, int y, LocalTransform gridCenterTransform)
        {
            
            Vector3 gridCorner = new Vector3(gridCenterTransform.Position.x,gridCenterTransform.Position.y,gridCenterTransform.Position.z)
                                 -Vector3.right*(grid.nodeXCount*grid.nodeSize)/2.0f
                                 -Vector3.forward*(grid.nodeYCount+grid.nodeSize)/2.0f;
            
            Vector3 worldCoord = gridCorner+Vector3.right*(2*x*grid.nodeSize+grid.nodeSize)/2
                +Vector3.forward*(2*y*grid.nodeSize+grid.nodeSize)/2;
            return worldCoord;
            
        }

        /// <summary>
        /// 将世界坐标转换为最近网格节点（使用 NativeArray 版本，供 Job 内调用）。
        /// 超出网格范围的坐标会被 clamp 到边界节点
        /// </summary>
        /// <param name="_pos">世界坐标</param>
        /// <param name="_grid">网格参数</param>
        /// <param name="_gridLocalTransform">网格 Entity 的世界变换</param>
        /// <param name="_nodes">节点一维数组（扁平化二维数组）</param>
        /// <returns>距离世界坐标最近的网格节点</returns>
        public static AsNode WorldPosToNode(float3 _pos, AsGrid _grid, LocalTransform _gridLocalTransform, NativeArray<AsNode> _nodes)
        {
            // 计算归一化百分比（0~1），超界时 clamp
            float percentX = math.clamp((_pos.x - _gridLocalTransform.Position.x + (_grid.nodeXCount * _grid.nodeSize) / 2) / (_grid.nodeXCount * _grid.nodeSize), 0, 1);
            float percentY = math.clamp((_pos.z - _gridLocalTransform.Position.z + (_grid.nodeYCount * _grid.nodeSize) / 2) / (_grid.nodeYCount * _grid.nodeSize), 0, 1);

            // 映射到节点坐标（四舍五入到最近节点）
            int x = (int)math.round((_grid.nodeXCount - 1) * percentX);
            int y = (int)math.round((_grid.nodeYCount - 1) * percentY);

            // 计算一维数组索引（根据长宽大小选择主轴）
            int d = _grid.nodeXCount > _grid.nodeYCount ? _grid.nodeXCount : _grid.nodeYCount;

            int index = 0;
            if (_grid.nodeXCount > _grid.nodeYCount)
                index = x + y * d;
            else index = y + x * d;

            return _nodes[index];
        }

        /// <summary>
        /// 将世界坐标转换为最近网格节点（使用 DynamicBuffer 版本，供主线程调用）
        /// </summary>
        public static AsNode WorldPosToNode(float3 _pos, AsGrid _grid, LocalTransform _gridLocalTransform, DynamicBuffer<AsGridNodesBufferComponent> _nodes)
        {
            float percentX = math.clamp((_pos.x - _gridLocalTransform.Position.x + (_grid.nodeXCount * _grid.nodeSize) / 2) / (_grid.nodeXCount * _grid.nodeSize), 0, 1);
            float percentY = math.clamp((_pos.z - _gridLocalTransform.Position.z + (_grid.nodeYCount * _grid.nodeSize) / 2) / (_grid.nodeYCount * _grid.nodeSize), 0, 1);

            int x = (int)math.round((_grid.nodeXCount - 1) * percentX);
            int y = (int)math.round((_grid.nodeYCount - 1) * percentY);

            int d = _grid.nodeXCount > _grid.nodeYCount ? _grid.nodeXCount : _grid.nodeYCount;

            int index = 0;
            if (_grid.nodeXCount > _grid.nodeYCount)
                index = x + y * d;
            else index = y + x * d;

            return _nodes[index].node;
        }
    }
}