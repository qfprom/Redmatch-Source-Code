Shader "ShaderCourse/Week3/4.2.OverwatchShield" {
	Properties {
		_Alpha ("Alpha", Float) = 0.5
		_Color ("Color", Vector) = (0,0,0,0)
		_PulseIntensity ("Hex Pulse Intensity", Float) = 4
		_PulseDistanceModifier ("Hex Pulse Distance Modifier", Float) = 20
		_PulseTextureModifier ("Hex Pulse Distance Texture Modifier", Float) = 0.5
		_PulseCycleModifier ("Hex Pulse Cycle Modifier", Float) = 3
		_EdgePulseColor ("Edge Pulse Color", Vector) = (0,0,0,0)
		_EdgePulseIntensity ("Edge Pulse Intensity", Float) = 10
		_EdgeDistanceModifier ("Edge Pulse Distance Modifier", Float) = 20
		_EdgeCycleModifier ("Edge Pulse Cycle Modifier", Float) = 3
		_EdgeWidthModifier ("Edge Pulse Width Modifier", Range(0, 0.99)) = 0.99
		_EdgeFalloffExp ("Edge Falloff Exponent", Range(0.01, 50)) = 4
		_EdgeIntensity ("Edge Intensity", Float) = 20
		_IntersectFalloffExp ("Intersect Falloff Exponent", Range(0.01, 50)) = 4
		_IntersectIntensity ("Intersect Intensity", Float) = 20
		_HexTex ("Hex Texture", 2D) = "white" {}
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

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
}