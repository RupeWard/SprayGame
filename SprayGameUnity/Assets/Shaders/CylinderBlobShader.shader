Shader "Custom/CylinderBlobShader"
{
	Properties
   	{
//      	_MainTex ("Texture", 2D) = "white" {}
      	_Color ("Main Color", Color) = (1, 1, 1, 1)
//      	_Alpha ("Alpha", Float) = 0.25
//     	_Phase ("Phase",Float) = 0.0
   	}
   	
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
	
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
			uniform float4 _Color; 
//			float _Alpha;
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
				float x = i.uv.x;
				float invx = 1 - x;

	            fixed4 texColor = _Color * (1 - invx * invx);
//				fixed4 texColor = _Color;
				
	            texColor.a = 1;
	            
				return texColor;
			}
			
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
