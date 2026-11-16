using System;
using AstarRoutine.Astar.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class ShowGrid:MonoBehaviour
{
    private EntityManager _entityManager;
    [SerializeField]
    public Color[] gridColors;
    [SerializeField]
    public Color lineColor;
    public bool showGrid;
    public bool showPenalties;
    public bool showGridIslands;
    public Color penaltyColor;
    
    public int maxPenalty;
    private void OnDrawGizmos()
    {
        if (_entityManager == default)
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
        if (!showGrid)
            return;
        var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<AsGrid>());
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        foreach (var entity in entities)
        {
            AsGrid grid = _entityManager.GetComponentData<AsGrid>(entity);
            DynamicBuffer<AsGridNodesBufferComponent> nodes =
                _entityManager.GetBuffer<AsGridNodesBufferComponent>(entity);
            DynamicBuffer<AsIslandBufferComponent> islands =
                _entityManager.GetBuffer<AsIslandBufferComponent>(entity);
            LocalTransform transform =  _entityManager.GetComponentData<LocalTransform>(entity);
            int d = grid.nodeXCount > grid.nodeYCount ? grid.nodeXCount : grid.nodeYCount;
            Vector3 startCorner = new Vector3(transform.Position.x,transform.Position.y,transform.Position.z)
                                  -Vector3.forward*grid.nodeYCount*grid.nodeSize/2
                                  -Vector3.right*grid.nodeXCount*grid.nodeSize/2;
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
                    Color gridColor = gridColors[0];
                    Gizmos.color = nodes[index].node.isAccessible ? gridColor : Color.red;
                    if (showGridIslands && Application.isPlaying && islands.Length > 0 && nodes[index].node.isAccessible)
                    {
                        int islandId = islands[index].islandId;
                        if (islandId >= gridColors.Length)
                        {
                            ResizeGridColors(islandId);
                        }
                        gridColor = gridColors[islandId]; 
                    }
                    
                    if (showPenalties&&nodes[index].node.penalty >0)
                    {
                        float rate = math.clamp(nodes[index].node.penalty * 1.0f / maxPenalty*1.0f,0,1);
                        Gizmos.color = Color.Lerp(gridColor, penaltyColor, rate);
                    }
                    else
                    {
                        Gizmos.color = gridColor;
                    }

                    if (showGrid)
                    {
                        Gizmos.DrawCube(startCorner + Vector3.right*(2*x*grid.nodeSize+grid.nodeSize)/2+Vector3.forward*(2*y*grid.nodeSize+grid.nodeSize)/2,
                            new Vector3(grid.nodeSize,0.1f,grid.nodeSize));
                        Gizmos.color = lineColor;
                        Gizmos.DrawWireCube(startCorner + Vector3.right*(2*x*grid.nodeSize+grid.nodeSize)/2+Vector3.forward*(2*y*grid.nodeSize+grid.nodeSize)/2,
                            new Vector3(grid.nodeSize,0.1f,grid.nodeSize));
                    }
                }
            }
        }

        entities.Dispose();
    }

    private void ResizeGridColors(int outIndex)
    {
        Color[] newColors = new Color[outIndex+1];
        int index = 0;
        while (index <= outIndex)
        {
            if (index < gridColors.Length)
            {
                newColors[index] = gridColors[index];
            }
            else
            {
                newColors[index] = new Color(UnityEngine.Random.Range(0, 1f),
                    UnityEngine.Random.Range(0, 1f),
                    UnityEngine.Random.Range(0, 1f));
            }

            index++;
        }
        gridColors = newColors;
    }
}
