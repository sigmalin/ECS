using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Lesson2
{

    public class Lesson2_ECSBoostrap : MonoBehaviour 
    {
        [SerializeField] UnityEngine.UI.Text countText;

        World TestECSWorld;

        // Use this for initialization
        void Start()
        {
            TestECSWorld = new World("Chunk Iteration");

            TestECSWorld.CreateManager(typeof(MovementSystem));
            (TestECSWorld.CreateManager(typeof(DisplaySystem)) as DisplaySystem).MessageText = countText;

            EntityManager entityManager = TestECSWorld.GetOrCreateManager<EntityManager>();    

            
            var entity1 = entityManager.CreateEntity(
                                                    ComponentType.Create<RiderData>(),
                                                    ComponentType.Create<BikeData>());

            entityManager.SetComponentData(entity1, new RiderData() { Distance = 0f, Speed = 0f });
            entityManager.SetComponentData(entity1, new BikeData() { Stamina = 1f });

            var entity2 = entityManager.CreateEntity(
                                                    ComponentType.Create<RiderData>(),
                                                    ComponentType.Create<CarData>());

            entityManager.SetComponentData(entity2, new RiderData() { Distance = 0f, Speed = 0f });
            entityManager.SetComponentData(entity2, new CarData() { Acceleration = 0.5f, Energy = 1f });

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
