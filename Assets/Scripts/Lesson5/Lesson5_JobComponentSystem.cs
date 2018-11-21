using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using Random = Unity.Mathematics.Random;

namespace Lesson5
{
	[DisableAutoCreation]
	public class MovementSystem : JobComponentSystem
	{
		[BurstCompile(Accuracy.Med, Support.Relaxed)]
		struct MovementDataJob : IJobParallelFor
		{
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        	public ArchetypeChunkComponentType<PositionData> PositionDataType;
            public ArchetypeChunkComponentType<DestinationData> DestinationDataType;
            public float delta;

			public void Execute(int chunkIndex)
			{
				ArchetypeChunk chunk = chunks[chunkIndex];

				Process(chunk);
			}

			unsafe void Process(ArchetypeChunk chunk)
			{
				NativeArray<PositionData> positions = chunk.GetNativeArray(PositionDataType);
                NativeArray<DestinationData> destinations = chunk.GetNativeArray(DestinationDataType);

				var posPtr = (PositionData*)positions.GetUnsafePtr();
                var destPtr = (DestinationData*)destinations.GetUnsafePtr();

				int length = positions.Length;

				for(int i = 0; i < length; ++i, ++posPtr, ++destPtr)
				{
					UnityEngine.Vector3 pos  = posPtr->Value;
                    UnityEngine.Vector3 dest  = destPtr->Value;
                    float velocity = destPtr->Velocity;

                    UnityEngine.Vector3 dir = (dest - pos).normalized;

                    pos += velocity * delta * dir;

					posPtr->Value  = pos;				
				}
			}
		}

		EntityArchetypeQuery query;

		protected override void OnCreateManager()
        {
            query = new EntityArchetypeQuery
			{
				All = new ComponentType[] {
						ComponentType.Create<PositionData>(),
						ComponentType.Create<DestinationData>()
					},
				Any = System.Array.Empty<ComponentType>(),
				None = System.Array.Empty<ComponentType>()
			};
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new MovementDataJob()
			{
				chunks = EntityManager.CreateArchetypeChunkArray(query, Allocator.TempJob),
				PositionDataType    = GetArchetypeChunkComponentType<PositionData>(false),  // isReadOnly = false
                DestinationDataType = GetArchetypeChunkComponentType<DestinationData>(false),// isReadOnly = false
                delta = UnityEngine.Time.deltaTime
			};
			return job.Schedule(job.chunks.Length, 16, inputDeps);
		}
	}

	[DisableAutoCreation]
    public class CheckArriveSystem : JobComponentSystem
	{
		[BurstCompile]
		struct CheckArriveJob : IJobProcessComponentData<PositionData, DestinationData>
		{
            public NativeArray<Random> randomsJob;

			public void Execute([ReadOnly] ref PositionData pos, ref DestinationData dest)
			{
                UnityEngine.Vector3 diff = dest.Value - pos.Value;

                if( UnityEngine.Mathf.Abs(diff.x) + UnityEngine.Mathf.Abs(diff.y) + UnityEngine.Mathf.Abs(diff.z) <= 0.1f)
                {
                    var random = randomsJob[0];

                    dest.Value.x = random.NextFloat(-5f, 5f);
                    dest.Value.z = random.NextFloat(-5f, 5f);
                    dest.Velocity = random.NextFloat(0.1f, 1f);

                    randomsJob[0] = random;
                }
			}
		}

        NativeArray<Random> randoms;

        protected override void OnCreateManager()
        {
            randoms = new NativeArray<Random>(1, Allocator.Persistent)
            {
                [0] = new Random(123456789)
            };
        }

        protected override void OnDestroyManager()
        {
            randoms.Dispose();
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new CheckArriveJob()
            {
                randomsJob = randoms
			};
			//return job.Schedule(this, inputDeps);
			return job.ScheduleSingle(this, inputDeps);
		}
	}

	[DisableAutoCreation]
	public class UpdateDistanceFromCameraSystem : JobComponentSystem
	{
		[BurstCompile(Accuracy.Med, Support.Relaxed)]
		struct MovementDataJob : IJobParallelFor
		{
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        	public ArchetypeChunkComponentType<PositionData> PositionDataType;
            public ArchetypeChunkComponentType<DistanceFromCameraData> DistanceDataType;
            public UnityEngine.Vector3 posCamera;
			public UnityEngine.Vector3 dirCamera;

			public void Execute(int chunkIndex)
			{
				ArchetypeChunk chunk = chunks[chunkIndex];

				Process(chunk);
			}

			unsafe void Process(ArchetypeChunk chunk)
			{
				NativeArray<PositionData> positions = chunk.GetNativeArray(PositionDataType);
                NativeArray<DistanceFromCameraData> distances = chunk.GetNativeArray(DistanceDataType);

				var posPtr = (PositionData*)positions.GetUnsafePtr();
                var disPtr = (DistanceFromCameraData*)distances.GetUnsafePtr();

				int length = positions.Length;

				for(int i = 0; i < length; ++i, ++posPtr, ++disPtr)
				{
					UnityEngine.Vector3 pos  = posPtr->Value;
                    
					UnityEngine.Vector3 linePointToPoint = pos - posCamera;

					float t = UnityEngine.Vector3.Dot(linePointToPoint, dirCamera);

					pos -= (posCamera + dirCamera * t);

					disPtr->Value  = pos.sqrMagnitude;//pos.magnitude;				
				}
			}
		}

		EntityArchetypeQuery query;
		UnityEngine.Transform cameraTrans;

		protected override void OnCreateManager()
        {
            query = new EntityArchetypeQuery
			{
				All = new ComponentType[] {
						ComponentType.Create<PositionData>(),
						ComponentType.Create<DistanceFromCameraData>()
					},
				Any = System.Array.Empty<ComponentType>(),
				None = System.Array.Empty<ComponentType>()
			};
        }

		protected override void OnDestroyManager()
        {
            cameraTrans = null;
        }

		public void Initial(UnityEngine.Transform cam)
        {
            cameraTrans = cam;
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new MovementDataJob()
			{
				chunks = EntityManager.CreateArchetypeChunkArray(query, Allocator.TempJob),
				PositionDataType    = GetArchetypeChunkComponentType<PositionData>(false),  // isReadOnly = false
                DistanceDataType = GetArchetypeChunkComponentType<DistanceFromCameraData>(false),// isReadOnly = false
                posCamera = cameraTrans.position,
				dirCamera = cameraTrans.right
			};
			return job.Schedule(job.chunks.Length, 16, inputDeps);
		}
	}
}
