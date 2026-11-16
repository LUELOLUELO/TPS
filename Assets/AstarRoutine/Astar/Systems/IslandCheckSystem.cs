using AstarRoutine.Astar.Components;
using Unity.Collections;
using Unity.Entities;

namespace AstarRoutine.Astar.Systems
{
    public partial struct IslandCheckSystem:ISystem
    {
        private bool hasCheckIsland;
        private int lastIslandId;
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (grid, entity) in SystemAPI.Query<RefRW<AsGrid>>().WithEntityAccess())
            {
                if (!grid.ValueRW.refreshIsland)
                {
                    grid.ValueRW.refreshIsland = true;
                    hasCheckIsland = false;
                }

                if (!hasCheckIsland)
                {
                    int currentIslandId = 0;
                    DynamicBuffer<AsIslandBufferComponent> islands = SystemAPI.GetBuffer<AsIslandBufferComponent>(entity);
                    DynamicBuffer<AsGridNodesBufferComponent> nodes = SystemAPI.GetBuffer<AsGridNodesBufferComponent>(entity);
                    int nodeXCount = grid.ValueRO.nodeXCount;
                    int nodeYCount = grid.ValueRO.nodeYCount;
                    islands.Length = nodeXCount*nodeYCount;
                    NativeArray<bool> visited = new NativeArray<bool>(nodeXCount*nodeYCount, Allocator.Temp);
                    int d = nodeXCount>nodeYCount? nodeXCount:nodeYCount;
                    for (int x = 0; x < nodeXCount; x++)
                    {
                        for (int y = 0; y < nodeYCount; y++)
                        {
                            int index = 0;
                            if (nodeXCount > nodeYCount)
                            {
                                index = x + y * d;
                            }
                            else
                            {
                                index = y+x*d;
                            }

                            if (!visited[index] && nodes[index].node.isAccessible)
                            {
                                BFSCheckIsland(grid.ValueRW, nodes, islands, visited, x,y, currentIslandId);
                                lastIslandId =  currentIslandId;
                                currentIslandId++;
                            }
                        }
                    }
                    hasCheckIsland =  true;
                }
            }
        }

        private void BFSCheckIsland(AsGrid grid, DynamicBuffer<AsGridNodesBufferComponent> nodes,
            DynamicBuffer<AsIslandBufferComponent> islands, NativeArray<bool> visited, int x, int y, int currentIslandId)
        {
            NativeQueue<(int,int)> quque = new NativeQueue<(int,int)>(Allocator.Temp);
            quque.Enqueue((x,y));
            int d = grid.nodeXCount>grid.nodeYCount? grid.nodeXCount : grid.nodeYCount;
            while (quque.Count > 0)
            {
                var pos = quque.Dequeue();
                //边界检测
                if (pos.Item1 < 0 || pos.Item2 < 0 || 
                    pos.Item1 >= grid.nodeXCount || 
                    pos.Item2 >= grid.nodeYCount)
                {
                    continue;
                }
                //计算索引
                int index = 0;
                if (grid.nodeXCount > grid.nodeYCount)
                {
                    index = pos.Item1 + pos.Item2 * d;
                }
                else
                {
                    index = pos.Item2 + pos.Item1 * d;
                }
                if (visited[index])
                    continue;
                //访问节点
                visited[index] = true;
                if (!nodes[index].node.isAccessible)
                {
                    //碰到障碍物直接continue
                    islands[index] = new AsIslandBufferComponent()
                    {
                        islandId =  currentIslandId,
                    };
                    continue;
                }
                //不是障碍物则分配岛屿id
                islands[index] = new AsIslandBufferComponent()
                {
                    islandId =  currentIslandId,
                };
                if(lastIslandId!=currentIslandId)
                    UnityEngine.Debug.Log(currentIslandId);
                //向四周扩展
                quque.Enqueue((pos.Item1+1, pos.Item2));
                quque.Enqueue((pos.Item1, pos.Item2+1));
                quque.Enqueue((pos.Item1-1, pos.Item2));
                quque.Enqueue((pos.Item1, pos.Item2-1));
            }
        }
    }
}