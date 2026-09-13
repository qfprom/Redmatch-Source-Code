Shader "MK/Glass/Free" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,0.1)
		_MainTex ("Color (RGB)", 2D) = "white" {}
		_MainTint ("Main Tint", Range(0, 2)) = 0
		[Toggle] _AlbedoMap ("Color source map", Float) = 0
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Distortion ("Distortion", Range(0, 1)) = 0.3
		_Shininess ("Shininess", Range(0.01, 1)) = 0.275
		_SpecColor ("Specular Color", Vector) = (1,1,1,0.5)
		_SpecularIntensity ("Intensity", Range(0, 2)) = 0.5
		_EmissionColor ("Emission Color", Vector) = (0,0,0,1)
		_RimColor ("Rim Color", Vector) = (1,1,1,1)
		_RimSize ("Rim Size", Range(0, 5)) = 2.3
		_RimIntensity ("Intensity", Range(0, 1)) = 0.3
		[HideInInspector] _MKEditorShowMainBehavior ("Main Behavior", Float) = 1
		[HideInInspector] _MKEditorShowRenderBehavior ("Render Behavior", Float) = 0
		[HideInInspector] _MKEditorShowSpecularBehavior ("Specular Behavior", Float) = 0
		[HideInInspector] _MKEditorShowRimBehavior ("Rim Behavior", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
	Fallback "Legacy Shaders/Transparent/Diffuse"
	//CustomEditor "MK.Glass.MKGlassFreeEditor"
}