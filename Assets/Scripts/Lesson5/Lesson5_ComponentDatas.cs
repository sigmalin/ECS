using Unity.Entities;

namespace Lesson5
{
    public struct SpriteMeshesData : ISharedComponentData
    {
        public UnityEngine.Mesh[] Meshes;
    }

	public struct AnimationData : ISharedComponentData
    {
        public float interval;
		public int count;

		public void SetFPS(int _fps)
		{
			interval = _fps <= 0 ? 0f : 1f / _fps;
		}
    }

	public struct AnimationSpriteData : IComponentData
    {
        public int spriteIndx;
		public float time;
    }

    public struct PositionData : IComponentData
    {
        public UnityEngine.Vector3 Value;
    }

    public struct DestinationData : IComponentData
    {
        public UnityEngine.Vector3 Value;
        public float Velocity;
    }

    public struct DistanceFromCameraData : IComponentData
    {
        public float Value;
    }
}

