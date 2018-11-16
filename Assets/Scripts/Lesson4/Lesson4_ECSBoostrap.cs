using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Jobs;

namespace Lesson4
{

    public class Lesson4_ECSBoostrap : MonoBehaviour 
    {
        [SerializeField] UnityEngine.UI.Text countText;

        World[] TestECSWorlds;
		EntityManager[] EntityMgrs;

		EntityArchetype type;

		UnityEngine.Coroutine co;

        // Use this for initialization
        void Start()
        {
			TestECSWorlds = new World[2];

            TestECSWorlds[0] = new World("Main World");
			TestECSWorlds[1] = new World("Entity Factory");

			EntityMgrs = new EntityManager[2];

			EntityMgrs[0] = TestECSWorlds[0].GetOrCreateManager<EntityManager>();    
			EntityMgrs[1] = TestECSWorlds[1].GetOrCreateManager<EntityManager>();   

			type = EntityMgrs[1].CreateArchetype(typeof(DummyData));

            (TestECSWorlds[0].CreateManager(typeof(DisplaySystem)) as DisplaySystem).MessageText = countText;


			ScriptBehaviourUpdateOrder.UpdatePlayerLoop(TestECSWorlds[0], TestECSWorlds[1]);	
        }

        void OnDestroy()
        {
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);

			if(EntityMgrs != null)
			{
				for(int Indx = 0; Indx < EntityMgrs.Length; ++Indx)
                {
                    EntityMgrs[Indx] = null;
                }

                EntityMgrs = null;
			}

            if(TestECSWorlds != null)
            {
                for(int Indx = 0; Indx < TestECSWorlds.Length; ++Indx)
                {
                    TestECSWorlds[Indx].Dispose();
                }

                TestECSWorlds = null;
            }		
        }

		IEnumerator CreateEntityFromAnotherWorld()
		{
            var entity = EntityMgrs[1].CreateEntity(type);

            ExclusiveEntityTransaction cmd = EntityMgrs[1].BeginExclusiveEntityTransaction();

            EntityMgrs[1].ExclusiveEntityTransactionDependency = new CreateDummyDataJob()
            {
                commands = cmd,
                prefab = entity,
                count = 999
            }.Schedule(EntityMgrs[1].ExclusiveEntityTransactionDependency);

            JobHandle.ScheduleBatchedJobs();

            yield return new WaitUntil(() => EntityMgrs[1].ExclusiveEntityTransactionDependency.IsCompleted);

            EntityMgrs[1].EndExclusiveEntityTransaction();
            EntityMgrs[0].MoveEntitiesFrom(EntityMgrs[1]);

            co = null;
		}

        void Update()
        {
            if(co == null && Input.GetMouseButtonDown(0))
            {
                co = StartCoroutine(CreateEntityFromAnotherWorld());
            }            
        }
    }
}
