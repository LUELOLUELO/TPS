using AstarRoutine.Astar.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.Playables;

namespace AstarRoutine.Astar.Systems
{
    [UpdateBefore(typeof(IslandCheckSystem))]
    [BurstCompile]
    public partial struct CheckObstacleSystem:ISystem
    {
        private bool hasCheckObstacles;
        private bool hasCheckPenalty;
        private ComponentLookup<AObstacle> obstacleLookup;
        private ComponentLookup<APenalty> penaltyLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            hasCheckObstacles = false;
            hasCheckPenalty = false;

            // 初始注册组件类型，GetComponentLookup 记录 TypeIndex 但不立即读数据
            obstacleLookup = state.GetComponentLookup<AObstacle>();
            penaltyLookup = state.GetComponentLookup<APenalty>();
        }

        public void OnUpdate(ref SystemState state)
        {
            obstacleLookup.Update(ref state);
            penaltyLookup.Update(ref state);
            foreach (var (grid,transform,entity) in SystemAPI.Query<RefRW<AsGrid>,RefRW<LocalTransform>>().WithEntityAccess())
            {
                if (!grid.ValueRO.refreshGrid)
                {
                    hasCheckObstacles = false;
                    hasCheckPenalty = false;
                    grid.ValueRW.refreshGrid = true;
                }

                if (!hasCheckObstacles)
                {
                    DynamicBuffer<AsGridNodesBufferComponent> nodes = SystemAPI.GetBuffer<AsGridNodesBufferComponent>(entity);
                    CheckObstacles(grid.ValueRW, nodes, transform.ValueRW,ref state);
                    hasCheckObstacles = true;
                }
            }
        }

        private void CheckObstacles( AsGrid grid, DynamicBuffer<AsGridNodesBufferComponent> nodes, LocalTransform transform,ref SystemState state)
        {
            int d = grid.nodeXCount > grid.nodeYCount? grid.nodeXCount:grid.nodeYCount;
            for (int x = 0; x < grid.nodeXCount; x++)
            {
                for (int y = 0; y < grid.nodeYCount; y++)
                {
                    int index = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        index = y * d + x;
                    }
                    else
                    {
                        index = x * d + y;
                    }
                    AsGridNodesBufferComponent node =  nodes[index];
                    node.node.penalty = 0;
                    node.node.isAccessible = true;
                    nodes[index] = node;
                    //获取当前帧的Physical World
                    EntityQueryBuilder queryBuilder =
                        new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
                    EntityQuery query = state.EntityManager.CreateEntityQuery(queryBuilder);
                    var physicalWorld = query.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                    query.Dispose();
                    
                    //构造边长为grid.nodeSize的3D碰撞盒
                    float3 extents = new float3(1, 1, 1) * grid.nodeSize / 2;
                    float3 worldCoord = Tools.Utility.GridPositionToWorldCoord(grid,x,y,transform);
                    NativeList<DistanceHit> hitResults = new NativeList<DistanceHit>(Allocator.Temp);
                    
                    bool isAccessible = !physicalWorld.OverlapBox(worldCoord,quaternion.identity,extents,
                        ref hitResults,new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = (uint)grid.InaccessibleLMIndex|(uint)grid.PenaltyLMIndex,
                        GroupIndex = 0,
                    });

                    for (int i = 0; i < hitResults.Length; i++)
                    {
                        //检查障碍物
                        if (obstacleLookup.HasComponent(hitResults[i].Entity))
                        {
                            var newNode = nodes[index];
                            newNode.node.isAccessible = isAccessible;
                            AObstacle obstacle = obstacleLookup[hitResults[i].Entity];
                            newNode.node.penalty = obstacle.Penalty;
                            nodes[index] = newNode;
                        }
                        
                        //检查惩罚区域
                        if (penaltyLookup.HasComponent(hitResults[i].Entity))
                        {
                            var newNode = nodes[index];
                            APenalty penalty = penaltyLookup[hitResults[i].Entity];
                            newNode.node.penalty = penalty.Penalty;
                            nodes[index] = newNode;
                        }
                    }
                    hitResults.Dispose();
                }
            }

            //做一次简化的高斯模糊。我们希望障碍物周围的惩罚值是均匀递减的，这样使得寻路路径倾向于远离障碍物
            BlurPenalties(grid, nodes);
        }

        private void BlurPenalties(AsGrid grid,DynamicBuffer<AsGridNodesBufferComponent> nodes)
        {
            int blurSize = grid.blurSize;
            int kernalSize = blurSize*2+1;
            NativeArray<int> horizontalPass = new NativeArray<int>(grid.nodeXCount*grid.nodeYCount, Allocator.TempJob);
            NativeArray<int> verticalPass = new NativeArray<int>(grid.nodeXCount*grid.nodeYCount, Allocator.TempJob);
            int d = grid.nodeXCount>grid.nodeYCount? grid.nodeXCount:grid.nodeYCount;
            
            //先水平过滤一遍
            for (int y = 0; y < grid.nodeYCount; y++)
            {
                for (int i = -blurSize; i <= blurSize; i++)
                {
                    int clampIdx = math.clamp(i,0,blurSize);
                    //被采样的源节点索引
                    int index = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        index = clampIdx + d * y;
                    }
                    else
                    {
                        index = clampIdx * d + y;
                    }
                    //horizontalPass索引
                    int hpIndex = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        hpIndex = y * d + 0;
                    }
                    else
                    {
                        hpIndex = 0 * d + y;
                    }
                    horizontalPass[hpIndex]+=nodes[index].node.penalty;
                }
                //使用滑动窗口计算和，好处是不用每轮都累加1次
                for (int x = 1; x < grid.nodeXCount; x++)
                {
                    int removeIdx = math.clamp(x-blurSize-1,0,grid.nodeXCount-1);
                    int addIdx = math.clamp(x+blurSize,0,grid.nodeXCount-1);

                    int removeIndex = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        removeIndex = removeIdx+d*y;
                    }
                    else
                    {
                        removeIndex = removeIdx * d + y;
                    }
                    int addIndex = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        addIndex = addIdx + d * y;
                    }
                    else
                    {
                        addIndex = addIdx * d + y;
                    }
                    
                    int hpIndex = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        hpIndex = x + d * y;
                    }
                    else
                    {
                        hpIndex = x*d+y;
                    }

                    int lasthpIndex = 0;
                    if (grid.nodeXCount > grid.nodeYCount)
                    {
                        lasthpIndex = x-1 + d * y;
                    }
                    else
                    {
                        lasthpIndex = (x-1)*d+y;
                    }
                    horizontalPass[hpIndex] = horizontalPass[lasthpIndex]-nodes[removeIndex].node.penalty+nodes[addIndex].node.penalty;
                }
            }
            
            //第二遍，竖直方向滤波一次
            for (int x = 0; x < grid.nodeXCount; x++)
            {
                // ── 初始化 y=0 的滑动窗口 ──
                // 累加 horizontalPass 中 [0-blurSize, 0+blurSize] 行的值
                for (int y = -blurSize; y <= blurSize; y++)
                {
                    int clampY = math.clamp(y, 0, blurSize);  // 上方超出边界时 clamp 到第 0 行

                    // 源索引（horizontalPass 中第 clampY 行、第 x 列）
                    int index;
                    if (grid.nodeXCount > grid.nodeYCount)
                        index = x + clampY * d;
                    else index = clampY + x * d;

                    // 目标：y=0 的垂直过滤结果
                    int vpIndex;
                    if (grid.nodeXCount > grid.nodeYCount)
                        vpIndex = x + 0 * d;
                    else vpIndex = 0 + x * d;

                    verticalPass[vpIndex] += horizontalPass[index];
                }

                // ── 滑动窗口：从 y=1 向下推进 ──
                for (int y = 1; y < grid.nodeYCount; y++)
                {
                    // 当前节点的垂直过滤结果目标索引
                    int vpIndex;
                    if (grid.nodeXCount > grid.nodeYCount)
                        vpIndex = x + y * d;
                    else vpIndex = y + x * d;

                    // 上一行（y-1）的垂直过滤结果索引
                    int prevvpIndex;
                    if (grid.nodeXCount > grid.nodeYCount)
                        prevvpIndex = x + (y - 1) * d;
                    else prevvpIndex = (y - 1) + x * d;

                    // 离开窗口的行索引（y - blurSize - 1，超出上边界时 clamp）
                    int removeIndex = math.clamp(y - blurSize - 1, 0, grid.nodeYCount-1);
                    // 进入窗口的行索引（y + blurSize，超出下边界时 clamp）
                    int addIndex = math.clamp(y + blurSize, 0, grid.nodeYCount - 1);

                    // 离开行在 horizontalPass 中的索引
                    int removevpIndex;
                    if (grid.nodeXCount > grid.nodeYCount)
                        removevpIndex = x + removeIndex * d;
                    else removevpIndex = removeIndex + x * d;

                    // 进入行在 horizontalPass 中的索引
                    int addvpIndex;
                    if (grid.nodeXCount > grid.nodeYCount)
                        addvpIndex = x + addIndex * d;
                    else addvpIndex = addIndex + x * d;

                    // 垂直方向滑动：新窗口和 = 上一窗口和 - 离开行的水平和 + 进入行的水平和
                    verticalPass[vpIndex] = verticalPass[prevvpIndex]
                        - horizontalPass[removevpIndex]
                        + horizontalPass[addvpIndex];

                    // ── 归一化并写回节点 ──
                    // verticalPass[vpIndex] = 7×7 核内所有惩罚之和（水平和 × 垂直和）
                    // 除以 kernelSize² = 7×7 = 49，得到平均惩罚值（四舍五入）
                    // 这就是该节点最终的模糊惩罚值
                    int blurredWeight = (int)math.round((float)verticalPass[vpIndex] / (kernalSize * kernalSize));

                    // 目标节点索引
                    int index;
                    if (grid.nodeXCount > grid.nodeYCount)
                        index = x + y * d;
                    else index = y + x * d;

                    // 写回：只更新 Penalty，IsAccessible 保持障碍物扫描时的结果不变
                    AsNode newNode = nodes[index].node;
                    newNode.penalty = blurredWeight;
                    nodes[index] = new AsGridNodesBufferComponent() { node = newNode };
                }
            }
            horizontalPass.Dispose();
            verticalPass.Dispose();
        }
    }
}