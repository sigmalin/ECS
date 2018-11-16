using Unity.Entities;

namespace Lesson2
{
    public struct RiderData : IComponentData
    {
        public float Distance;
		public float Speed;
    }


    public struct BikeData : IComponentData
    {
		public float Stamina;
    }

	public struct CarData : IComponentData
    {		
        public float Acceleration;
		public float Energy;
    }
}
