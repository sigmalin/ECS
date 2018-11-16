using Unity.Entities;

namespace Lesson3
{
    public struct LifeData : IComponentData
    {
        public float Life;
    }

    public struct SpawnData : IComponentData
    {
        public float CountDown;
    }
}
