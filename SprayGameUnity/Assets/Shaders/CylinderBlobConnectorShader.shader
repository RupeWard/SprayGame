Shader "Custom/CylinderBlobConnectorShader"
{
	Properties
   	{
//      	_MainTex ("Texture", 2D) = "white" {}
      	_Color1("Color1", Color) = (1, 1, 1, 1)
		_Color2("Color2", Color) = (1, 1, 1, 1)
		_Edge("Edge", Color) = (0, 0, 0, 1)
		_Alpha ("Alpha", Float) = 1.0
//     	_Phase ("Phase",Float) = 0.0
   	}
   	
	SubShader
	{
		Tags { "RenderQueue" = "Transparent" }
	
		Pass
		{
			//ZWrite Off
         	Blend SrcAlpha OneMinusSrcAlpha 
         	//Cull Front
         
			CGPROGRAM
			
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
	
			//Textures
//	        sampler2D _MainTex;
			uniform float4 _Color1; 
			uniform float4 _Color2;
			uniform float4 _Edge;
			float _Alpha;
//			float _Phase;
//			float _Repeats;
			
			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};
	
//			float4 _MainTex_ST;
	
			v2f vert (appdata_full v)
			{
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;// TRANSFORM_TEX(v.texcoord, _MainTex);
	            
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
//	            fixed4 texColor = _Color * i.uv.y;//
//	            fixed4 texColor = tex2D(_MainTex, i.uv.xy);
//				float x = i.uv.x;
	//			float invx = 1 - x;

	            fixed4 texColor = _Color1 * i.uv.x + _Color2 * (1.0 - i.uv.x);
//				fixed4 texColor = _Color;
				
			float x = 2.0 * abs(0.5 - i.uv.x);
			//float invx = 1 - x;
			float fraction = x * x * x;
			texColor = _Edge * fraction + texColor * (1 - fraction);

	            texColor.a = _Alpha;
	            
				return texColor;
			}
			
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
