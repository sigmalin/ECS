using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Lesson1
{

    public class Lesson1_ECSBoostrap : MonoBehaviour 
    {
        [SerializeField] UnityEngine.UI.Text countText;

        World[] TestECSWorlds;

        RangeData Range;

        // Use this for initialization
        void Start()
        {
            TestECSWorlds = new World[2];

            TestECSWorlds[0] = new World("Test ECS 1");
            TestECSWorlds[1] = new World("Test ECS 2");

            Range.min = 0;
            Range.max = 100;

            ///
            SetTest_ECS_1();

            SetTest_ECS_2();

            RunWorld_1();
        }

        void OnDestroy()
        {
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);

            if(TestECSWorlds != null)
            {
                for(int Indx = 0; Indx < TestECSWorlds.Length; ++Indx)
                {
                    TestECSWorlds[Indx].Dispose();
                }

                TestECSWorlds = null;
            }		
        }

        void SetTest_ECS_1()
        {
            TestECSWorlds[0].CreateManager(typeof(CountSystem));
            TestECSWorlds[0].CreateManager(typeof(ThresholdSystem));
            TestECSWorlds[0].CreateManager(typeof(DisplaySystem), countText);

            EntityManager entityManager = TestECSWorlds[0].GetOrCreateManager<EntityManager>();        
    /*
            #region Add Shared
            var sampleArchetype = entityManager.CreateArchetype(typeof(CountData));
            var entity = entityManager.CreateEntity(sampleArchetype);

            entityManager.AddSharedComponentData(entity, Range);
            #endregion
    */
            #region Set Shared
            var sampleArchetype = entityManager.CreateArchetype(typeof(CountData), typeof(RangeData));
            var entity = entityManager.CreateEntity(sampleArchetype);

            entityManager.SetSharedComponentData(entity, Range);
            #endregion
        }

        void SetTest_ECS_2()
        {
            TestECSWorlds[1].CreateManager(typeof(MultiCountSystem));
            TestECSWorlds[1].CreateManager(typeof(ThresholdSystem));
            TestECSWorlds[1].CreateManager(typeof(DisplaySystem), countText);
            
            

            EntityManager entityManager = TestECSWorlds[1].GetOrCreateManager<EntityManager>();    

            var sampleArchetype = entityManager.CreateArchetype(typeof(CountData), typeof(RangeData));
            
            var entity1 = entityManager.CreateEntity(sampleArchetype);
            entityManager.SetComponentData(entity1, new CountData() { count = 0 });
            entityManager.SetSharedComponentData(entity1, Range);

            var entity2 = entityManager.CreateEntity(sampleArchetype);
            entityManager.SetComponentData(entity2, new CountData() { count = 50 });
            entityManager.SetSharedComponentData(entity2, Range);
        }

        public void RunWorld_1()
        {
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(TestECSWorlds[0]);	
        }

        public void RunWorld_2()
        {
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(TestECSWorlds[1]);	
        }
    }
}
