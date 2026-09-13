Shader "Unlit/PixelBurnEffect" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "black" {}
		[Space(10)] _GlowColor ("Glow Color", Vector) = (1,1,1,1)
		_FadeColor ("Fade Color", Vector) = (1,1,1,1)
		[Space(10)] [Header(Adjust Effect Parameters)] _Speed ("Effect Speed", Range(0, 30)) = 10
		_Start ("Start", Range(-2, 0.999)) = 0.5
		_End ("End", Range(0.001, 5)) = 0.9
		_TexCutoff ("Texture Cutoff Alpha", Range(0, 1)) = 0.5
		_GlowCutoff ("Glow Cutoff Alpha", Range(0, 1)) = 0.3
		[IntRange] _PixelLevel ("Pixelization Level", Range(0, 512)) = 80
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

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
}