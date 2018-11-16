using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Lesson1
{
	public class MultiCountSystem : JobComponentSystem
	{
		[BurstCompile]
		struct CountDataJob : IJobProcessComponentData<CountData>
		{
			public void Execute(ref CountData Count)
			{
				Count.count++;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new CountDataJob();
			return job.Schedule(this, inputDeps);
		}
	}
}
