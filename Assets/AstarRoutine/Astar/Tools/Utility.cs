using AstarRoutine.Astar.Components;
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
                                 -Vector3.right*(grid.nodeXCount*grid.nodeSize)/2
                                 -Vector3.forward*(grid.nodeYCount+grid.nodeSize)/2;
            
            Vector3 worldCoord = gridCorner+Vector3.right*(2*x*grid.nodeSize+grid.nodeSize)/2
                +Vector3.forward*(2*y*grid.nodeSize+grid.nodeSize)/2;
            return worldCoord;
            
        }
    }
}