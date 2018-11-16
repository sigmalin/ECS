using Unity.Entities;

namespace Lesson3
{
	[UpdateBefore(typeof(Lesson3Group_1))]
	sealed class EndFrameBarrierSystem : BarrierSystem
	{		
	}

    [UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
    class Lesson3Group_1{}

	[UpdateAfter(typeof(Lesson3Group_1))]
    class Lesson3Group_2{}

#region Lesson3Group_1
	[UpdateInGroup(typeof(Lesson3Group_1))]
    sealed class ConsumptionSystem : ComponentSystem
    {
        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<LifeData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<LifeData>();

			for(int i=0; i<source.Length; i++)
            {
				LifeData life = source[i];
				life.Life -= UnityEngine.Time.deltaTime;
				source[i] = life;
            }
        }
    }

	[UpdateInGroup(typeof(Lesson3Group_1))]
	[UpdateAfter(typeof(ConsumptionSystem))]
    sealed class DestroySystem : ComponentSystem
    {
        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.ReadOnly<LifeData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
			var entities = group.GetEntityArray();
            var source = group.GetComponentDataArray<LifeData>();

			for(int i=0; i<source.Length; i++)
            {
                if(source[i].Life <= 0f) 
                {
					PostUpdateCommands.DestroyEntity(entities[i]);
                }
            }
        }
    }
#endregion

#region Lesson3Group_2
	[UpdateInGroup(typeof(Lesson3Group_2))]
    sealed class CountDownSystem : ComponentSystem
    {
        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<SpawnData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<SpawnData>();

			for(int i=0; i<source.Length; i++)
            {
				SpawnData spawn = source[i];
				spawn.CountDown -= UnityEngine.Time.deltaTime;
				source[i] = spawn;
            }
        }
    }

	[UpdateInGroup(typeof(Lesson3Group_2))]
	[UpdateAfter(typeof(CountDownSystem))]
    sealed class SpawnSystem : ComponentSystem
    {
		public EntityArchetype type;

        ComponentGroup group;		

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<SpawnData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<SpawnData>();

			for(int i=0; i<source.Length; i++)
            {
                if(source[i].CountDown <= 0f) 
                {
					SpawnData spawn = source[i];
					spawn.CountDown += UnityEngine.Random.Range(0.5f,10f);
					source[i] = spawn;

					PostUpdateCommands.CreateEntity(type);
					PostUpdateCommands.SetComponent(new LifeData() { Life = UnityEngine.Random.Range(0.5f,10f) });
                }
            }
        }
    }
#endregion


    [UpdateAfter(typeof(Lesson3Group_2))]
    sealed class DisplaySystem : ComponentSystem
    {
        public UnityEngine.UI.Text MessageText;

        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.ReadOnly<LifeData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<LifeData>();

            MessageText.text = source.Length.ToString();
        }
    }
}