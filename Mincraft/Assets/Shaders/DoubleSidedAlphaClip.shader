Shader "Test/DoubleSidedAlphaClip"
{
	Properties
	{
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Clipping("_Clipping", Range(0,1)) = 0.5

	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "False"
			"RenderType" = "Transparent"
		}


		Cull Off
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Fog { }


		CGPROGRAM
		#pragma surface surf BlinnPhongEditor  vertex:vert
		#pragma target 2.0


		sampler2D _MainTex;
		float _Clipping;

		struct EditorSurfaceOutput {
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Gloss;
			half Specular;
			half Alpha;
			half4 Custom;
		};

		inline half4 LightingBlinnPhongEditor(EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			return half4(s.Albedo.r, s.Albedo.g, s.Albedo.b, 1);
		}

		struct Input {
			float2 uv_MainTex;

		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
		}


		void surf(Input IN, inout EditorSurfaceOutput o) 
		{
			float4 Tex2D0 = tex2D(_MainTex,(IN.uv_MainTex.xyxy).xy);
			clip(Tex2D0.aaaa - _Clipping.xxxx);

			o.Emission = Tex2D0;
		}
		ENDCG
	}
	Fallback "Diffuse"
}