Shader "Custom/water" {
	Properties 
	{
		_Color ("Main Color", COLOR) = (1,1,1,1)
		_MainTex ("Main texture", 2D) = "white"
	}
	
	SubShader 
	{
		pass
		{
			Tags { "RenderType"="transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			cull off
			
			CGPROGRAM
	      	#pragma vertex vert
	      	#pragma fragment frag
	        #pragma target 4.0
	        #include "UnityCG.cginc"
	
			float4 _Color;
			sampler2D _MainTex;
			
			float4 _MainTex_ST;
			
			struct Output
			{
				float4 position : SV_POSITION;
				float4 color : Color;
				float2  uv : TEXCOORD0;
			};
			
			Output vert(appdata_base v)
			{
				Output output;
				
				output.position = mul (UNITY_MATRIX_MVP, v.vertex);
		    	output.color = _Color;
		    	output.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
		    	output.uv += _Time.y / 3;
		    	return output;
			}
			
			float4 frag(Output output) : COLOR
			{
				return output.color *  tex2D (_MainTex, output.uv);
			}
			
			ENDCG
		}
	} 
}
