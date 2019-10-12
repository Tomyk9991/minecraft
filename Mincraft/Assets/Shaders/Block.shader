Shader "Minecraft/Block" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_GlobalLightLevel("Global light level", Range(0.0, 1.0)) = 1
		_minGlobalLightLevel("Minimum light level", float) = 0.15
		_maxGlobalLightLevel("Maximum light level", float) = 0.80
	}

		SubShader{
			Tags {
				"RenderType" = "Opaque"
			}

			Lighting Off

			Pass
			{
				CGPROGRAM
				#pragma vertex vertexFunction
				#pragma fragment fragmentFunction

				#include "UnityCG.cginc"

				struct vertexInformation {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR;
				};

				struct pixelInformation {
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Color;

				float _GlobalLightLevel;
				float _minGlobalLightLevel;
				float _maxGlobalLightLevel;

				pixelInformation vertexFunction(vertexInformation v) {
					pixelInformation o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.color = v.color;
					return o;
				}

				fixed4 fragmentFunction(pixelInformation i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					
					//float shade = (_maxGlobalLightLevel - _minGlobalLightLevel) * _GlobalLightLevel + _minGlobalLightLevel;
					float shade = lerp(_minGlobalLightLevel, _maxGlobalLightLevel, _GlobalLightLevel);
					shade *= i.color.a;

					shade = clamp(1. - shade, _minGlobalLightLevel, _maxGlobalLightLevel);

					col = lerp(col, float4(0, 0, 0, 1), shade);


					return col;
				}

				ENDCG
			}
		}
}
