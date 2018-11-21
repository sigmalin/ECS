using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering;
using Unity.Collections;


namespace Lesson5
{
    [UpdateInGroup(typeof(Lesson5Group))]   
    [UpdateAfter(typeof(AnimationSpriteSystem))] 
    [DisableAutoCreation]
    public class AnimationSpriteRenderSystem : ComponentSystem
	{
        protected ComponentGroup group;

        public CommandBuffer cmd;

        public UnityEngine.Camera camera;

        public UnityEngine.Material material;

		protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<AnimationSpriteData>(), ComponentType.ReadOnly<PositionData>(), 
                                                        ComponentType.ReadOnly<SpriteMeshesData>(), ComponentType.ReadOnly<DistanceFromCameraData>() };
            
            group = GetComponentGroup(componentTypes);
        }

        protected override void OnDestroyManager()
        {
            if(cmd != null)
            {
                if(camera != null)
                {
                    camera.RemoveCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterSkybox, cmd);
                    camera = null;
                }
                
                cmd.Release();
            }
            
            material = null;
        }

        public virtual void Initial(UnityEngine.Camera cam, UnityEngine.Material mat)
        {
            camera = cam;
            material = mat;

            cmd = new UnityEngine.Rendering.CommandBuffer();
            cmd.name = "AnimationSpriteRenderSystem";

            camera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterSkybox, cmd);
        }

        protected override void OnUpdate()
        {
			var distances = group.GetComponentDataArray<DistanceFromCameraData>();

            int[] sort = Heapsort(ref distances);

			OnRender(ref sort);
        }

		#region draw
		protected virtual void OnRender(ref int[] sort)
		{
			var sprites = group.GetComponentDataArray<AnimationSpriteData>();
            var pos = group.GetComponentDataArray<PositionData>();
            var meshes = group.GetSharedComponentDataArray<SpriteMeshesData>();

			cmd.Clear ();

            
            cmd.SetViewProjectionMatrices (camera.worldToCameraMatrix, UnityEngine.GL.GetGPUProjectionMatrix(camera.projectionMatrix, false));

            //for(int i=0; i<sprites.Length; i++)
            for(int indx=0; indx<sort.Length; indx++)
            {
                int i = sort[indx];
                AnimationSpriteData sprite = sprites[i];
                PositionData position = pos[i];
                SpriteMeshesData mesh = meshes[i];

                cmd.DrawMesh(mesh.Meshes[sprite.spriteIndx], UnityEngine.Matrix4x4.Translate(position.Value), material);
            }
			
		}
		#endregion

		#region Entity sort

        int[] Heapsort([ReadOnly] ref Unity.Entities.ComponentDataArray<DistanceFromCameraData> distances)
        {
            int count = distances.Length;
            int[] sort = new int[count];

            for(int Indx = 0; Indx < count; ++Indx)
                sort[Indx] = Indx;

            for (int i = count>>1; i >= 0; i--)
            {
                adjust(i, count-1, ref sort, ref distances);
            }

            for (int i = count-2; i >= 0; i--)
            {
                int swap = sort[i + 1];
                sort[i + 1] = sort[0];
                sort[0] = swap;
                adjust(0, i, ref sort, ref distances);
            }
            
            return sort;
        } 

        void adjust(int i, int n, ref int[] sort, [ReadOnly] ref Unity.Entities.ComponentDataArray<DistanceFromCameraData> distances)
        {
            int iPosition;
            int iChange;

            iPosition = sort[i];
            iChange = 2 * i;
            while (iChange <= n)
            {
                if (iChange < n && distances[sort[iChange]].Value > distances[sort[iChange + 1]].Value)
                {
                    iChange++;
                }
                if (distances[iPosition].Value <= distances[sort[iChange]].Value)
                {
                    break;
                }
                sort[iChange>>1] = sort[iChange];
                iChange <<= 1;
            }
            sort[iChange >> 1] = iPosition;
        }

		#endregion

	}

	[UpdateInGroup(typeof(Lesson5Group))]   
    [UpdateAfter(typeof(AnimationSpriteSystem))] 
    [DisableAutoCreation]
    public class AnimationSpriteRenderSystem_35 : AnimationSpriteRenderSystem
	{
		UnityEngine.MaterialPropertyBlock materialPropertyBlock;

		UnityEngine.Mesh instanceMesh;

		int columnCount;
		int rowCount;

        UnityEngine.Matrix4x4[] TRSs;
		float[] columnIndx;
		float[] rowIndx;

        int ColumnID;
        int RowID;

        const int INSTANCE_MAX = 1023;

		protected override void OnCreateManager()
        {
            base.OnCreateManager();

			materialPropertyBlock = new UnityEngine.MaterialPropertyBlock();

			instanceMesh = new UnityEngine.Mesh();

			instanceMesh.SetVertices(
				new System.Collections.Generic.List<UnityEngine.Vector3>()
				{
					new UnityEngine.Vector3(-0.25f,   0f,  0f),
					new UnityEngine.Vector3(-0.25f, 0.5f,  0f),
					new UnityEngine.Vector3( 0.25f,   0f,  0f),
					new UnityEngine.Vector3( 0.25f, 0.5f,  0f),
				}
			);

			instanceMesh.SetUVs(
				0,
				new System.Collections.Generic.List<UnityEngine.Vector2>()
				{
					new UnityEngine.Vector3(0f, 0f),
					new UnityEngine.Vector3(0f, 1f),
					new UnityEngine.Vector3(1f, 0f),
					new UnityEngine.Vector3(1f, 1f),
				}
			);

			instanceMesh.SetTriangles(
				new System.Collections.Generic.List<int>()
				{
					0,1,2,3,2,1
				},
				0
			);

			instanceMesh.UploadMeshData(true);


            TRSs = new UnityEngine.Matrix4x4[INSTANCE_MAX];
            columnIndx = new float[INSTANCE_MAX];
            rowIndx = new float[INSTANCE_MAX];

            ColumnID = UnityEngine.Shader.PropertyToID("_ColumnIndx");
            RowID = UnityEngine.Shader.PropertyToID("_RowIndx");
        }

        protected override void OnDestroyManager()
        {
			materialPropertyBlock = null;

            base.OnDestroyManager();
        }

		public override void Initial(UnityEngine.Camera cam, UnityEngine.Material mat)
		{
			base.Initial(cam, mat);
            mat.enableInstancing = true;

			columnCount = UnityEngine.Mathf.FloorToInt(mat.GetFloat("_Column"));
			rowCount = UnityEngine.Mathf.FloorToInt(mat.GetFloat("_Row"));
		}

		protected override void OnRender(ref int[] sort)
		{
			var sprites = group.GetComponentDataArray<AnimationSpriteData>();
            var pos = group.GetComponentDataArray<PositionData>();
            var meshes = group.GetSharedComponentDataArray<SpriteMeshesData>();

			int instanceCount = sort.Length;

            int renderCount = 0;

            cmd.Clear ();

            cmd.SetViewProjectionMatrices (camera.worldToCameraMatrix, UnityEngine.GL.GetGPUProjectionMatrix(camera.projectionMatrix, false));

			for(int indx=0; indx<instanceCount; indx++)
			{
                if(renderCount == INSTANCE_MAX)
                {
                    RenderMeshInstanced(renderCount);

                    renderCount = 0;
                }

                int i = sort[indx];

				TRSs[renderCount] = UnityEngine.Matrix4x4.TRS(	
											pos[i].Value, 
											UnityEngine.Quaternion.identity,
											UnityEngine.Vector3.one);                         

				int row = sprites[i].spriteIndx / columnCount;

				columnIndx[renderCount] = sprites[i].spriteIndx - (columnCount * row);
				rowIndx[renderCount] = rowCount - row - 1;

                ++renderCount;
			}

            if(renderCount != 0)
            {
                RenderMeshInstanced(renderCount);
            }
		}

        void RenderMeshInstanced(int instanceCount)
        {
            materialPropertyBlock.Clear();
			materialPropertyBlock.SetFloatArray(ColumnID, columnIndx);
			materialPropertyBlock.SetFloatArray(RowID, rowIndx);

            cmd.DrawMeshInstanced(instanceMesh, 0, material, -1, TRSs, instanceCount, materialPropertyBlock);
        }
	}

    [UpdateInGroup(typeof(Lesson5Group))]   
    [UpdateAfter(typeof(AnimationSpriteSystem))] 
    [DisableAutoCreation]
    public class AnimationSpriteRenderSystem_45 : AnimationSpriteRenderSystem
	{
		UnityEngine.Mesh instanceMesh;

		int columnCount;
		int rowCount;

        private UnityEngine.ComputeBuffer insObjBuffer;
        private UnityEngine.ComputeBuffer argsBuffer;
        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        int instanceCount;
        int InsObjSize;

        struct InsObj
        {
            public UnityEngine.Vector3 pos;
            public float col;
			public float row;
        };

        InsObj[] InsObjs;
        int InsObjsID;

		protected override void OnCreateManager()
        {
            base.OnCreateManager();

			instanceMesh = new UnityEngine.Mesh();

			instanceMesh.SetVertices(
				new System.Collections.Generic.List<UnityEngine.Vector3>()
				{
					new UnityEngine.Vector3(-0.25f,   0f,  0f),
					new UnityEngine.Vector3(-0.25f, 0.5f,  0f),
					new UnityEngine.Vector3( 0.25f,   0f,  0f),
					new UnityEngine.Vector3( 0.25f, 0.5f,  0f),
				}
			);

			instanceMesh.SetUVs(
				0,
				new System.Collections.Generic.List<UnityEngine.Vector2>()
				{
					new UnityEngine.Vector3(0f, 0f),
					new UnityEngine.Vector3(0f, 1f),
					new UnityEngine.Vector3(1f, 0f),
					new UnityEngine.Vector3(1f, 1f),
				}
			);

			instanceMesh.SetTriangles(
				new System.Collections.Generic.List<int>()
				{
					0,1,2,3,2,1
				},
				0
			);

			instanceMesh.UploadMeshData(true);

            instanceCount = 0;

            InsObjSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(InsObj));

    	    argsBuffer = new UnityEngine.ComputeBuffer(1, args.Length * sizeof(uint), UnityEngine.ComputeBufferType.IndirectArguments);		

            InsObjs = null;

            InsObjsID = UnityEngine.Shader.PropertyToID("InsObjs");
        }

        protected override void OnDestroyManager()
        {
			if(insObjBuffer != null)
            {
                insObjBuffer.Release();
                insObjBuffer = null;
            }

		    if(argsBuffer != null)
            {
                argsBuffer.Release();
                argsBuffer = null;
            }

            InsObjs = null;

            base.OnDestroyManager();
        }

		public override void Initial(UnityEngine.Camera cam, UnityEngine.Material mat)
		{
			base.Initial(cam, mat);
            mat.enableInstancing = true;
            
			columnCount = UnityEngine.Mathf.FloorToInt(mat.GetFloat("_Column"));
			rowCount = UnityEngine.Mathf.FloorToInt(mat.GetFloat("_Row"));
		}

		protected override void OnRender(ref int[] sort)
		{
			var sprites = group.GetComponentDataArray<AnimationSpriteData>();
            var pos = group.GetComponentDataArray<PositionData>();
            var meshes = group.GetSharedComponentDataArray<SpriteMeshesData>();

			int count = sort.Length;

            UpdateBuffer(count);
            

			for(int indx=0; indx<instanceCount; indx++)
			{
                int i = sort[indx];

				InsObjs[indx].pos = pos[i].Value;

				int row = sprites[i].spriteIndx / columnCount;

				InsObjs[indx].col = sprites[i].spriteIndx - (columnCount * row);
				InsObjs[indx].row = rowCount - row - 1;
			}

            insObjBuffer.SetData(InsObjs);

            material.SetBuffer(InsObjsID, insObjBuffer);

            cmd.Clear ();

            cmd.SetViewProjectionMatrices (camera.worldToCameraMatrix, UnityEngine.GL.GetGPUProjectionMatrix(camera.projectionMatrix, false));

            cmd.DrawMeshInstancedIndirect(instanceMesh, 0, material, -1, argsBuffer);            
		}

        void UpdateBuffer(int count)
        {
            if(instanceCount == count)
                return;

            instanceCount = count;

            ///

            if(insObjBuffer != null)
            {
                insObjBuffer.Release();
                insObjBuffer = null;
            }
            
            insObjBuffer = new UnityEngine.ComputeBuffer(instanceCount, InsObjSize);

            InsObjs = null;
            InsObjs = new InsObj[instanceCount];

            ///

            if (instanceMesh != null) {
                args[0] = (uint)instanceMesh.GetIndexCount(0); //subMeshIndex
                args[1] = (uint)instanceCount;
                args[2] = (uint)instanceMesh.GetIndexStart(0); //subMeshIndex
                args[3] = (uint)instanceMesh.GetBaseVertex(0); //subMeshIndex
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }

            argsBuffer.SetData(args);
        }
	}
}