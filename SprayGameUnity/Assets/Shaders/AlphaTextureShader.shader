Shader "Custom/AlphaTextureShader"
{
	Properties
   	{
      	_MainTex ("Texture", 2D) = "white" {}
      	_Color ("Main Color", Color) = (1, 1, 1, 1)
      	_Alpha ("Alpha", Float) = 0.25
   	}
   	
	SubShader
	{
		Tags { "Queue" = "Transparent" }
	
		Pass
		{
			ZWrite Off
         	Blend SrcAlpha OneMinusSrcAlpha 
         
			CGPROGRAM
			
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
	
			//Textures
	        sampler2D _MainTex;
			uniform float4 _Color; 
			float _Alpha;
			
			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};
	
			float4 _MainTex_ST;
	
			v2f vert (appdata_full v)
			{
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	            
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
	            fixed4 texColor = _Color * tex2D(_MainTex, i.uv.xy);
	            texColor.a = _Alpha * texColor.a;
	            
				return texColor;
			}
			
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
