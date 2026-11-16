using Unity.Entities;

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
    }
}