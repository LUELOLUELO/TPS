using System;
using Unity.Entities;

namespace AstarRoutine.Astar.Components
{
    public struct AsGridNodesBufferComponent:IBufferElementData
    {
        public AsNode node;
    }

    public struct AsNode : IEquatable<AsNode>, IHeapItem<AsNode>
    {
        public int x;
        public int y;
        public int GHost;
        public int HHost;
        public bool isAccessible;
        public int penalty;

        public bool Equals(AsNode other)
        {
            return x == other.x && y == other.y;
        }

        public int CompareTo(AsNode other)
        {
            throw new NotImplementedException();
        }

        public int heapIndex { get; set; }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        public int heapIndex { get; set; }
    }
}