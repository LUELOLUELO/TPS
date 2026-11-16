using Unity.Entities;

namespace AstarRoutine.Astar.Components
{
    public struct AsIslandBufferComponent:IBufferElementData
    {
        public int islandId;
    }
}