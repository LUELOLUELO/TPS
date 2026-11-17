using AstarRoutine.Astar.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace AstarRoutine.Astar.Systems
{
    public partial struct PathFindingRequestSystem:ISystem
    {
        private ComponentLookup<LocalTransform> AlllocalTransformLookup;
        private ComponentLookup<AsPathFindingRequester> AllasPathFindingRequesterLookup;
        private ComponentLookup<AsPathFindingResult> AllPathfindingResults;
        public void OnCreate(ref SystemState state)
        {
            AlllocalTransformLookup = state.GetComponentLookup<LocalTransform>();
            AllasPathFindingRequesterLookup = state.GetComponentLookup<AsPathFindingRequester>(true);
            AllPathfindingResults =  state.GetComponentLookup<AsPathFindingResult>();
        }

        public void OnUpdate(ref SystemState state)
        {
            AlllocalTransformLookup.Update(ref state);
            AllasPathFindingRequesterLookup.Update(ref state);
            AllPathfindingResults.Update(ref state);
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            //获取全局单例
            DynamicBuffer<AsGridNodesBufferComponent> nodes = SystemAPI.GetSingletonBuffer<AsGridNodesBufferComponent>();
            DynamicBuffer<AsPathFindingOperationsBuffer> requesters = SystemAPI.GetSingletonBuffer<AsPathFindingOperationsBuffer>();
            DynamicBuffer<AsIslandBufferComponent> islands = SystemAPI.GetSingletonBuffer<AsIslandBufferComponent>();
            
            AsPathFindingParameters parameters = new AsPathFindingParameters();
            //遍历所有寻路请求
            foreach (var (grid, transform,pathFindingParameter) in SystemAPI.Query<RefRW<AsGrid>, RefRW<LocalTransform>
                     ,RefRW<AsPathFindingParameters>>())
            {
                parameters.maxPathfingOpsPerFrame = pathFindingParameter.ValueRO.maxPathfingOpsPerFrame;
                if (grid.ValueRO.differentIslandPolicy == DifferentIslandPolicy.DontPath)
                {
                    //这个寻路策略需要岛屿数据准备就绪（islands.Length == nodes.Length）
                    if (islands.Length < nodes.Length)
                    {
                        return;
                    }
                }
                //遍历所有寻路请求
                foreach (var (requester,localtransform, requestEntity) in SystemAPI.Query<RefRW<AsPathFindingRequester>,RefRW<LocalTransform>>().WithEntityAccess())
                {
                    //有请求没提交说明有新请求
                    if (!requester.ValueRO.RequestGiven)
                    {
                        //策略是DontPath，那说明有可能不需要处理这个寻路请求
                        if (grid.ValueRO.differentIslandPolicy == DifferentIslandPolicy.DontPath)
                        {
                            //检查寻路起点和终点是否在一个岛屿，如果不在一个岛就可以不用处理
                            AsNode startNode = Tools.Utility.WorldPosToNode(transform.ValueRW.Position, grid.ValueRW,transform.ValueRW, nodes);
                            AsNode endNode = Tools.Utility.WorldPosToNode(requester.ValueRW.Destination, grid.ValueRW,transform.ValueRW, nodes);
                            int startIndex = 0;
                            int endIndex = 0;
                            int d = grid.ValueRO.nodeXCount>grid.ValueRO.nodeYCount ? grid.ValueRO.nodeXCount : grid.ValueRO.nodeYCount;
                            if (grid.ValueRO.nodeXCount > grid.ValueRO.nodeYCount)
                            {
                                startIndex = startNode.x + startNode.y * d;
                                endIndex = endNode.x + endNode.y * d;
                            }
                            else
                            {
                                startIndex = startNode.x*d + startNode.y ;
                                endIndex = endNode.x*d + endNode.y ;
                            }
                            bool inSameIsland = islands[startIndex].islandId == islands[endIndex].islandId;
                            if (!inSameIsland)
                            {
                                //不在同一个岛屿且策略是DontPath,那就不用处理寻路
                                requester.ValueRW.RequestGiven = true;
                                continue;
                            }
                        }
                        //执行到这里说明需要处理请求
                        
                        //判断这个请求是新增的(以前没有这个实体的寻路请求)还是更新的(以前有，但是寻路destination可能不一样，需要覆盖)
                        bool entityHasRequest = false;
                        for (int i = 0; i < requesters.Length; i++)
                        {
                            if (requesters[i].requestEntity == requestEntity)
                            {
                                //如果这个entity以前有请求过，但还没处理，就更新请求
                                AsPathFindingOperations operation = requesters[i].operation;
                                operation.startPoint = localtransform.ValueRO.Position;
                                operation.endPoint = requester.ValueRW.Destination;
                                requesters[i] = new AsPathFindingOperationsBuffer()
                                {
                                    requestEntity = requestEntity,
                                    operation = operation,
                                };
                                entityHasRequest = true;
                            }
                        }

                        if (!entityHasRequest)
                        {
                            //说明是新增的请求
                            requesters.Add(new AsPathFindingOperationsBuffer()
                            {
                                requestEntity = requestEntity,
                                operation = new AsPathFindingOperations()
                                {
                                    startPoint = localtransform.ValueRW.Position,
                                    endPoint = requester.ValueRW.Destination,
                                }
                            });
                        }
                        //请求标记已经处理
                        requester.ValueRW.RequestGiven = true;
                    }
                }
            }
            //接下来是处理请求队列
            int k = 0;
            while (k<parameters.maxPathfingOpsPerFrame &&requesters.Length>0)
            {
                //requesters[0].requestEntity是发起寻路请求的entity(场景中看得见的，能寻路的npc）
                AsPathFindingRequester requester = AllasPathFindingRequesterLookup[requesters[0].requestEntity];
                //requester.RequestEntity是存寻路计算结果的临时entity，寻路完成就被销毁了，所以要判断是否存在
                if (SystemAPI.Exists(requester.RequestEntity))
                {
                    //存在且寻路还没完成就实时更新寻路起点，终点
                    AsPathFindingResult result = AllPathfindingResults[requester.RequestEntity];
                    if (!result.SearchFinished)
                    {
                        SystemAPI.SetComponent(requester.RequestEntity, new AsPathFindingResult());
                        SystemAPI.SetComponent(requester.RequestEntity, new AsPathFindingOperations()
                        {
                            startPoint = AlllocalTransformLookup[requesters[0].requestEntity].Position,
                            endPoint = requester.Destination,
                        });
                    }
                    //寻路完成就不管了
                }
                else
                {
                    //创建entity，挂载组件存寻路状态结果
                    Entity e = state.EntityManager.CreateEntity();
                    requester.RequestEntity = e;
                    ecb.AddComponent<AsPathFindingOperations>(e, new AsPathFindingOperations()
                    {
                        startPoint = AlllocalTransformLookup[requesters[0].requestEntity].Position,
                        endPoint =  requester.Destination,
                    });
                    ecb.AddComponent<AsPathFindingResult>(e);
                    //挂载buffer存寻路的结果
                    ecb.AddBuffer<AsPathBufferComponent>(e);
                }
                AllasPathFindingRequesterLookup[requesters[0].requestEntity] = requester;
                requesters.RemoveAt(0);
                k++;
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}