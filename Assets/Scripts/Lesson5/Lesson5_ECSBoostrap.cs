using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using System.Linq;

namespace Lesson5
{

    public class Lesson5_ECSBoostrap : MonoBehaviour 
    {
		World TestECSWorld;

		public Material mat;
		public Material mat_35;
		public Material mat_45;

        // Use this for initialization
        void Start()
        {
			List<Vector3> vertices = new List<Vector3>(16);
			List<Vector2> uvs = new List<Vector2>(16);
			List<int> indices = new List<int>(96);

			Sprite[] sprites = Resources.LoadAll<Sprite>("1300010303");
			Mesh[] meshes = sprites.Select(_ => 
			{
				Mesh mesh;
				_.FillSpriteMesh(out mesh);
				return mesh;
			}).ToArray();

			mat.mainTexture = sprites[0].texture;

			///
			SpriteMeshesData spriteMesh = new SpriteMeshesData()
			{
				Meshes = meshes
			};

			AnimationData AnimData = new AnimationData()
			{
				count = sprites.Length
			};
			AnimData.SetFPS(4);
			///

			TestECSWorld = new World("EntityCommandBuffer");

            EntityManager entityManager = TestECSWorld.GetOrCreateManager<EntityManager>();    

			var archetype = entityManager.CreateArchetype(
							typeof(SpriteMeshesData), typeof(AnimationData), typeof(AnimationSpriteData), 
							typeof(DestinationData), typeof(PositionData), typeof(DistanceFromCameraData));

            TestECSWorld.CreateManager(typeof(AnimationSpriteSystem));

			int shaderLv = SystemInfo.graphicsShaderLevel;

			if(45 <= shaderLv)
			{
				(TestECSWorld.CreateManager(typeof(AnimationSpriteRenderSystem_45)) as AnimationSpriteRenderSystem_45).Initial(Camera.main, mat_45);
			}
			else if(35 <= shaderLv)
			{
				(TestECSWorld.CreateManager(typeof(AnimationSpriteRenderSystem_35)) as AnimationSpriteRenderSystem_35).Initial(Camera.main, mat_35);
			}
			else
			{
				(TestECSWorld.CreateManager(typeof(AnimationSpriteRenderSystem)) as AnimationSpriteRenderSystem).Initial(Camera.main, mat);
			}

			TestECSWorld.CreateManager(typeof(MovementSystem));

			TestECSWorld.CreateManager(typeof(CheckArriveSystem));

			(TestECSWorld.CreateManager(typeof(UpdateDistanceFromCameraSystem)) as UpdateDistanceFromCameraSystem).Initial(Camera.main.transform);



			int count = 2048;
			for(int Indx = 0; Indx < count; ++Indx)
			{
				var entity = entityManager.CreateEntity(archetype);
				entityManager.SetComponentData(entity, new AnimationSpriteData() { spriteIndx = 3, time = 0f });
				
				entityManager.SetComponentData(entity, new PositionData() { Value = new Vector3(UnityEngine.Random.Range(-2f,2f),0f,UnityEngine.Random.Range(-2f,2f)) });
				entityManager.SetComponentData(entity, new DestinationData() { Value = new Vector3(UnityEngine.Random.Range(-2f,2f),0f,UnityEngine.Random.Range(-2f,2f)), Velocity = 1f });

				entityManager.SetSharedComponentData(entity, spriteMesh);
				entityManager.SetSharedComponentData(entity, AnimData);
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
