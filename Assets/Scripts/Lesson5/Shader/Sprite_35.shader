Shader "Unlit/Shader Model 35/Sprite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Column ("Column Count", Float ) = 0
		_Row ("Row Count", Float ) = 0

		[HideInInspector] _diff_U ("Diff Of Axis U", Float) = 0
        [HideInInspector] _diff_V ("Diff Of Axis V", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		ZTest LEQUAL
		ZWrite Off
		Blend SrcAlpha OneminusSrcAlpha
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _diff_U;
			float _diff_V;

			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _ColumnIndx)
				UNITY_DEFINE_INSTANCED_PROP(float, _RowIndx)
            UNITY_INSTANCING_BUFFER_END(Props)
			
			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);


				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float2(	(UNITY_ACCESS_INSTANCED_PROP(Props, _ColumnIndx) + v.uv.x) * _diff_U, 
								(UNITY_ACCESS_INSTANCED_PROP(Props, _RowIndx)    + v.uv.y) * _diff_V);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}

	CustomEditor "Sprite_GUI"
}
