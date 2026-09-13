using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class BloomRenderer : PostProcessEffectRenderer<Bloom>
	{
		private enum Pass
		{
			Prefilter13 = 0,
			Prefilter4 = 1,
			Downsample13 = 2,
			Downsample4 = 3,
			UpsampleTent = 4,
			UpsampleBox = 5,
			DebugOverlayThreshold = 6,
			DebugOverlayTent = 7,
			DebugOverlayBox = 8
		}

		private struct Level
		{
			internal int down;

			internal int up;
		}

		private Level[] m_Pyramid;

		private const int k_MaxPyramidSize = 16;

		public override void Init()
		{
			m_Pyramid = new Level[16];
			for (int i = 0; i < 16; i++)
			{
				m_Pyramid[i] = new Level
				{
					down = Shader.PropertyToID("_BloomMipDown" + i),
					up = Shader.PropertyToID("_BloomMipUp" + i)
				};
			}
		}

		public override void Render(PostProcessRenderContext context)
		{
			CommandBuffer command = context.command;
			command.BeginSample("BloomPyramid");
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.bloom);
			propertySheet.properties.SetTexture(ShaderIDs.AutoExposureTex, context.autoExposureTexture);
			float num = Mathf.Clamp(base.settings.anamorphicRatio, -1f, 1f);
			float num2 = ((!(num < 0f)) ? 0f : (0f - num));
			float num3 = ((!(num > 0f)) ? 0f : num);
			int num4 = Mathf.FloorToInt((float)context.screenWidth / (2f - num2));
			int num5 = Mathf.FloorToInt((float)context.screenHeight / (2f - num3));
			bool flag = context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass && context.camera.stereoTargetEye == StereoTargetEyeMask.Both;
			int num6 = ((!flag) ? num4 : (num4 * 2));
			int num7 = Mathf.Max(num4, num5);
			float num8 = Mathf.Log(num7, 2f) + Mathf.Min(base.settings.diffusion.value, 10f) - 10f;
			int num9 = Mathf.FloorToInt(num8);
			int num10 = Mathf.Clamp(num9, 1, 16);
			float num11 = 0.5f + num8 - (float)num9;
			propertySheet.properties.SetFloat(ShaderIDs.SampleScale, num11);
			float num12 = Mathf.GammaToLinearSpace(base.settings.threshold.value);
			float num13 = num12 * base.settings.softKnee.value + 1E-05f;
			Vector4 value = new Vector4(num12, num12 - num13, num13 * 2f, 0.25f / num13);
			propertySheet.properties.SetVector(ShaderIDs.Threshold, value);
			float x = Mathf.GammaToLinearSpace(base.settings.clamp.value);
			propertySheet.properties.SetVector(ShaderIDs.Params, new Vector4(x, 0f, 0f, 0f));
			int num14 = (base.settings.fastMode ? 1 : 0);
			RenderTargetIdentifier source = context.source;
			for (int i = 0; i < num10; i++)
			{
				int down = m_Pyramid[i].down;
				int up = m_Pyramid[i].up;
				int pass = ((i != 0) ? (2 + num14) : num14);
				context.GetScreenSpaceTemporaryRT(command, down, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, num6, num5);
				context.GetScreenSpaceTemporaryRT(command, up, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, num6, num5);
				command.BlitFullscreenTriangle(source, down, propertySheet, pass);
				source = down;
				num6 = ((!flag || num6 / 2 % 2 <= 0) ? (num6 / 2) : (1 + num6 / 2));
				num6 = Mathf.Max(num6, 1);
				num5 = Mathf.Max(num5 / 2, 1);
			}
			int num15 = m_Pyramid[num10 - 1].down;
			for (int num16 = num10 - 2; num16 >= 0; num16--)
			{
				int down2 = m_Pyramid[num16].down;
				int up2 = m_Pyramid[num16].up;
				command.SetGlobalTexture(ShaderIDs.BloomTex, down2);
				command.BlitFullscreenTriangle(num15, up2, propertySheet, 4 + num14);
				num15 = up2;
			}
			Color linear = base.settings.color.value.linear;
			float num17 = RuntimeUtilities.Exp2(base.settings.intensity.value / 10f) - 1f;
			Vector4 value2 = new Vector4(num11, num17, base.settings.dirtIntensity.value, num10);
			if (context.IsDebugOverlayEnabled(DebugOverlay.BloomThreshold))
			{
				context.PushDebugOverlay(command, context.source, propertySheet, 6);
			}
			else if (context.IsDebugOverlayEnabled(DebugOverlay.BloomBuffer))
			{
				propertySheet.properties.SetVector(ShaderIDs.ColorIntensity, new Vector4(linear.r, linear.g, linear.b, num17));
				context.PushDebugOverlay(command, m_Pyramid[0].up, propertySheet, 7 + num14);
			}
			Texture texture = ((!(base.settings.dirtTexture.value == null)) ? base.settings.dirtTexture.value : RuntimeUtilities.blackTexture);
			float num18 = (float)texture.width / (float)texture.height;
			float num19 = (float)context.screenWidth / (float)context.screenHeight;
			Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
			if (num18 > num19)
			{
				value3.x = num19 / num18;
				value3.z = (1f - value3.x) * 0.5f;
			}
			else if (num19 > num18)
			{
				value3.y = num18 / num19;
				value3.w = (1f - value3.y) * 0.5f;
			}
			PropertySheet uberSheet = context.uberSheet;
			if ((bool)base.settings.fastMode)
			{
				uberSheet.EnableKeyword("BLOOM_LOW");
			}
			else
			{
				uberSheet.EnableKeyword("BLOOM");
			}
			uberSheet.properties.SetVector(ShaderIDs.Bloom_DirtTileOffset, value3);
			uberSheet.properties.SetVector(ShaderIDs.Bloom_Settings, value2);
			uberSheet.properties.SetColor(ShaderIDs.Bloom_Color, linear);
			uberSheet.properties.SetTexture(ShaderIDs.Bloom_DirtTex, texture);
			command.SetGlobalTexture(ShaderIDs.BloomTex, num15);
			for (int j = 0; j < num10; j++)
			{
				if (m_Pyramid[j].down != num15)
				{
					command.ReleaseTemporaryRT(m_Pyramid[j].down);
				}
				if (m_Pyramid[j].up != num15)
				{
					command.ReleaseTemporaryRT(m_Pyramid[j].up);
				}
			}
			command.EndSample("BloomPyramid");
			context.bloomBufferNameID = num15;
		}
	}
}
