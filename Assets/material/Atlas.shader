Shader "Custom/Atlas" 
{
    Properties 
    {
      _MainTex ("Texture", 2D) = "white" {}
      _BumpMap ("Bumpmap", 2D) = "bump" {}
      _ratio ("Ratio", Float) = 0.5				// single texture size / total atlas size
      _SunColor ("Sun color", Color) = (1,1,1,1)
      _SunIntensity("Sun intensity", Range (0, 1)) = 1
    }
    
    SubShader 
    {
      Tags { "RenderType" = "Geometry" }
      
      pass
      {
	      CGPROGRAM
	      
	      #pragma vertex vert
		  #pragma fragment frag
	      #pragma target 4.0
	      #include "UnityCG.cginc"
	      
	      sampler2D _MainTex;
	      sampler2D _BumpMap;
	      float 	_ratio;
	      float4	_SunColor;
	      float		_SunIntensity;
	      
	      float4 _MainTex_ST;
	      
	      struct FragInput
	      {
	      	float2 uv : TEXCOORD0;
	      	float4 position : SV_POSITION;
	      	float4 color : Color;
	      };
	      
	      FragInput vert (appdata_full v) 
	      {
	      	FragInput fragInput;
	      
	       	fragInput.position = mul (UNITY_MATRIX_MVP, v.vertex);
	    	fragInput.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
	      
	        // choose texture with UV2
	      	int2 pos = int2(v.texcoord1.xy);
			
	      	//avoid filtering to acces nerby out pixels
	      	if (fragInput.uv.x < 0.015625) // number of pixel to recenter(2) / texture size(128)
	      		fragInput.uv.x += 0.015625;
	  		if (fragInput.uv.x > 0.984375) // 1 - 0,015625
	      		fragInput.uv.x -= 0.015625;
	      	if (fragInput.uv.y < 0.015625)
	      		fragInput.uv.y += 0.015625;
	      	if (fragInput.uv.y > 0.984375)
	      		fragInput.uv.y -= 0.015625;
			
			// tilling
			fragInput.uv = frac(fragInput.uv);
	      
	      	// shift
	      	fragInput.uv = (fragInput.uv * _ratio) + pos * _ratio;
	      	
	      	// color
	      	fragInput.color.rgb = v.color.rbg + v.color.a * _SunIntensity * _SunColor.rgb;

	      	//if (v.color.a < 0.01)
  	      		//fragInput.color = float4(1, 0, 0, 0);
	      	//else if (v.color.a < 0.26)
  	      		//fragInput.color = float4(0, 1, 0, 0);
	      	//else if (v.color.a < 0.51)
  	      		//fragInput.color = float4(0, 0, 1, 0);
	      	//else if (v.color.a < 0.76)
  	      		//fragInput.color = float4(1, 1, 0, 0);
  	      	//else
  	      		//fragInput.color = float4(0, 1, 1, 0);

	      	return fragInput;
	      }
	      
	      float4 frag(FragInput fragInput) : COLOR
	      {
	      	float2 dx = clamp(ddx(fragInput.uv), -0.004, 0.004);
	      	float2 dy = clamp(ddy(fragInput.uv), -0.004, 0.004);
	  		return tex2D (_MainTex, fragInput.uv, dx, dy) * fragInput.color;
	      }
	      
	      ENDCG
      }
    } 
}