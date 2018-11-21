using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Lesson2
{
	[DisableAutoCreation]
	public class MovementSystem : JobComponentSystem
	{
		[BurstCompile(Accuracy.Med, Support.Relaxed)]
		struct MovementDataJob : IJobParallelFor
		{
			[ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        	public ArchetypeChunkComponentType<RiderData> RiderDataType;
			public ArchetypeChunkComponentType<BikeData>  BikeDataDataType;
			public ArchetypeChunkComponentType<CarData>   CarDataDataType;
			public float time; 

			public void Execute(int chunkIndex)
			{
				ArchetypeChunk chunk = chunks[chunkIndex];

				if (chunk.Has(BikeDataDataType))
					ProcessBike(chunk);
				else if (chunk.Has(CarDataDataType))
					ProcessCar(chunk);
			}

			unsafe void ProcessBike(ArchetypeChunk chunk)
			{
				NativeArray<RiderData> riders = chunk.GetNativeArray(RiderDataType);
				NativeArray<BikeData> bikes = chunk.GetNativeArray(BikeDataDataType);

				var ridersPtr = (RiderData*)riders.GetUnsafePtr();
				var bikesPtr = (BikeData*)bikes.GetUnsafePtr();

				int length = riders.Length;

				for(int i = 0; i < length; ++i, ++ridersPtr, ++bikesPtr)
				{
					float stamina  = bikesPtr->Stamina;
					float speed    = ridersPtr->Speed;
					float distance = ridersPtr->Distance;

					if(stamina <= 0f)
					{
						speed = 0f;
						stamina = 1f;
					}

					speed += stamina;
					distance += speed * time;

					stamina -= time;

					bikesPtr->Stamina  = stamina;
					ridersPtr->Speed    = speed;
					ridersPtr->Distance = distance;					
				}
			}

			unsafe void ProcessCar(ArchetypeChunk chunk)
			{
				NativeArray<RiderData> riders = chunk.GetNativeArray(RiderDataType);
				NativeArray<CarData> cars = chunk.GetNativeArray(CarDataDataType);

				var ridersPtr = (RiderData*)riders.GetUnsafePtr();
				var carsPtr = (CarData*)cars.GetUnsafePtr();

				int length = riders.Length;

				for(int i = 0; i < length; ++i, ++ridersPtr, ++carsPtr)
				{
					float energy  = carsPtr->Energy;
					float acceleration  = carsPtr->Acceleration;
					float speed    = ridersPtr->Speed;
					float distance = ridersPtr->Distance;

					if(energy <= 0f)
					{
						speed = 0f;
						energy = 1f;
					}

					speed += acceleration * time;
					distance += speed * time;

					energy -= time;		

					carsPtr->Energy = energy;
					carsPtr->Acceleration = acceleration;
					ridersPtr->Speed = speed;
					ridersPtr->Distance = distance;			
				}
			}
		}

		EntityArchetypeQuery query;
		protected override void OnCreateManager()
        {
            query = new EntityArchetypeQuery
			{
				All = new ComponentType[] {
						ComponentType.Create<RiderData>()
					},
				Any = new ComponentType[] {
						ComponentType.ReadOnly<BikeData>(),
						ComponentType.ReadOnly<CarData>()
					},
				None = System.Array.Empty<ComponentType>()
			};
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new MovementDataJob()
			{
				chunks = EntityManager.CreateArchetypeChunkArray(query, Allocator.TempJob),
				RiderDataType    = GetArchetypeChunkComponentType<RiderData>(false), // isReadOnly = false
				BikeDataDataType = GetArchetypeChunkComponentType<BikeData>(false),  // isReadOnly = false
				CarDataDataType  = GetArchetypeChunkComponentType<CarData>(false),   // isReadOnly = false
				time = UnityEngine.Time.deltaTime
			};
			return job.Schedule(job.chunks.Length, 16, inputDeps);
		}
	}
}
