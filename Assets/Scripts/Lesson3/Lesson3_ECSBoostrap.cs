using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Lesson3
{

    public class Lesson3_ECSBoostrap : MonoBehaviour 
    {
        [SerializeField] UnityEngine.UI.Text countText;

        World TestECSWorld;

        // Use this for initialization
        void Start()
        {
            TestECSWorld = new World("EntityCommandBuffer");

            TestECSWorld.CreateManager(typeof(EndFrameBarrierSystem));
            EntityManager entityManager = TestECSWorld.GetOrCreateManager<EntityManager>();    

			var subtractArchetype = entityManager.CreateArchetype(typeof(LifeData));
            var addArchetype = entityManager.CreateArchetype(typeof(SpawnData));

            TestECSWorld.CreateManager(typeof(ConsumptionSystem));
			TestECSWorld.CreateManager(typeof(DestroySystem));

            TestECSWorld.CreateManager(typeof(CountDownSystem));
			(TestECSWorld.CreateManager(typeof(SpawnSystem)) as SpawnSystem).type = subtractArchetype;

            (TestECSWorld.CreateManager(typeof(DisplaySystem)) as DisplaySystem).MessageText = countText;
			

			int count = Random.Range(1,100);
			for(int Indx = 0; Indx < count; ++Indx)
			{
				var entity = entityManager.CreateEntity(subtractArchetype);

				entityManager.SetComponentData(entity, new LifeData() { Life = Random.Range(0.5f,10f) });
			}

            count = Random.Range(1,10);
			for(int Indx = 0; Indx < count; ++Indx)
			{
				var entity = entityManager.CreateEntity(addArchetype);

				entityManager.SetComponentData(entity, new SpawnData() { CountDown = Random.Range(0.5f,7.5f) });
			}

			ScriptBehaviourUpdateOrder.UpdatePlayerLoop(TestECSWorld);	
        }

        void OnDestroy()
        {
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);

            if(TestECSWorld != null)
            {
                TestECSWorld.Dispose();
                TestECSWorld = null;
            }		
        }
    }
}
