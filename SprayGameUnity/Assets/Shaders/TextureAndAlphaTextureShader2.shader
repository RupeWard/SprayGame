Shader "Custom/TextureAndAlphaTexture2" {
	Properties
   	{
      	_MainTex ("Texture", 2D) = "white" {}
      	_Color ("Main Color", Color) = (1, 1, 1, 1)
      	_AlphaTex ("Alpha Texture", 2D) = "white" {}
      	_Alpha ("Alpha", Float) = 0.25
      	_Phase ("Phase",Float) = 0.0
      	_AlphaMin("AlphaMin",Float) = 0.0
      	_AlphaMax("AlphaMax",Float) = 1.0
      	_AlphaPower("AlphaPower",Float) = 1.0
   	}
   	
	SubShader
	{
		Tags { "Queue" = "Transparent" }
	
		Pass
		{
			//ZWrite Off
         	Blend SrcAlpha OneMinusSrcAlpha 
         	//Cull Off
         
			CGPROGRAM
			
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
	
			//Textures
	        sampler2D _MainTex;
	        sampler2D _AlphaTex;
			uniform float4 _Color; 
			float _Alpha;
			float _Phase;
			float _Repeats;
			float _AlphaMin;
			float _AlphaMax;
			float _AlphaPower;
			
			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
				float2  uv2 : TEXCOORD1;
			};
	
			float4 _MainTex_ST;
			float4 _AlphaTex_ST;
	
			v2f vert (appdata_full v)
			{
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	            o.uv2 = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
//	            fixed4 texColor = _Color * i.uv.y;//
//	            fixed4 texColor = tex2D(_MainTex, i.uv.xy);
	            fixed4 texColor = _Color * tex2D(_MainTex, i.uv) ;
	            texColor.a = tex2D(_AlphaTex, i.uv);
//				fixed4 texColor = _Color;
				
	            texColor.a = pow( i.uv2.y * (_AlphaMax - _AlphaMin) + _AlphaMin, _AlphaPower);
				texColor.a = texColor.a * _Alpha;
	            return texColor;
			}
			
			ENDCG
		}
	}
	
	FallBack "Diffuse"

}