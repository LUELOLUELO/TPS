using AstarRoutine.Astar.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace AstarRoutine.Astar.Authoring
{
    public class AsGridAuthoring:MonoBehaviour
    {
        public int nodeSize;
        public int xCount;
        public int yCount;

        public LayerMask InaccessibleLMIndex;
        public LayerMask PenaltyLMIndex;
        
        public int blurSize;
        public class AsGridBaker : Baker<AsGridAuthoring>
        {
            public override void Bake(AsGridAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AsGrid grid = new AsGrid()
                {
                    nodeSize = authoring.nodeSize,
                    nodeXCount = authoring.xCount,
                    nodeYCount = authoring.yCount,
                    refreshGrid = false,
                    refreshIsland = false,
                    InaccessibleLMIndex = authoring.InaccessibleLMIndex.value,
                    PenaltyLMIndex = authoring.PenaltyLMIndex.value,
                    blurSize = authoring.blurSize
                };
                AddComponent(entity,grid);
                DynamicBuffer<AsGridNodesBufferComponent> nodes = AddBuffer<AsGridNodesBufferComponent>(entity);
                //添加岛屿id组件
                AddBuffer<AsIslandBufferComponent>(entity);
                nodes.Length = grid.nodeXCount * grid.nodeYCount;
                int d = grid.nodeXCount > grid.nodeYCount ? grid.nodeXCount:grid.nodeYCount;
                for (int x = 0; x < grid.nodeXCount; x++)
                {
                    for (int y = 0; y < grid.nodeYCount; y++)
                    {
                        AsGridNodesBufferComponent node = new AsGridNodesBufferComponent()
                        {
                            node = new AsNode()
                            {
                                x = x,
                                y = y,
                                HHost = 0,
                                GHost = 0,
                                isAccessible = true,
                                penalty = 0,
                            }
                        };
                        
                        int index = 0;
                        if (grid.nodeXCount>grid.nodeYCount)
                        {
                            index = y * d + x;
                        }
                        else
                        {
                            index = x * d+ y;
                        }
                        nodes[index] = node;
                    }
                }
                //Debug.Log("AsGridBaker烘焙完成");
            }
        }
    }
}