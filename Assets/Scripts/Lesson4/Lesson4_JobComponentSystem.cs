using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Lesson4
{
	[DisableAutoCreation]
	struct CreateDummyDataJob : IJob
	{
		public ExclusiveEntityTransaction commands;
    	public Entity prefab;
		public int count;

		public void Execute()
		{
			for(int Indx = 0; Indx < count; ++Indx)
			{
				var e = commands.Instantiate(prefab);
				commands.SetComponentData(e, new DummyData());
			}
		}
	}
}
