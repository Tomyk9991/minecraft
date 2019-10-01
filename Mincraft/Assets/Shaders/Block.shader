﻿Shader "Unlit/Block"
{
    Properties
    {
		_Color ("Color to discard", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Clipping("Clipping", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags { 
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
        }
        
        Cull Off
        ZWrite On
        ZTest LEqual
        ColorMask RGBA
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vertexFunction
            #pragma fragment fragmentFunction
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct vertexInformation
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct pixelInformation
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Clipping;
			fixed4 _Color;

            pixelInformation vertexFunction (vertexInformation v)
            {
                pixelInformation o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 fragmentFunction (pixelInformation i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);

				half3 delta = abs(col.rgb - _Color.rgb);
				if ((delta.r + delta.g + delta.b) < 0.001)
					discard;

                return col;
            }
            ENDCG
        }
    }
}