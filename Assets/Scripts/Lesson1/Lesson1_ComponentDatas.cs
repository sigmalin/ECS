using Unity.Entities;

namespace Lesson1
{
    public struct CountData : IComponentData
    {
        public int count;
    }

    [System.Serializable]
    public struct RangeData : ISharedComponentData
    {
        public int min, max;
    }
}