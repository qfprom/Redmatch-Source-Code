using UnityEngine;

namespace MK.Glass
{
	public static class MKGlassFreeMaterialHelper
	{
		public static class PropertyNames
		{
			public const string SHOW_MAIN_BEHAVIOR = "_MKEditorShowMainBehavior";

			public const string SHOW_LIGHT_BEHAVIOR = "_MKEditorShowLightBehavior";

			public const string SHOW_RENDER_BEHAVIOR = "_MKEditorShowRenderBehavior";

			public const string SHOW_SPECULAR_BEHAVIOR = "_MKEditorShowSpecularBehavior";

			public const string SHOW_RIM_BEHAVIOR = "_MKEditorShowRimBehavior";

			public const string MAIN_TEXTURE = "_MainTex";

			public const string MAIN_COLOR = "_Color";

			public const string MAIN_TINT = "_MainTint";

			public const string BUMP_MAP = "_BumpMap";

			public const string DISTORTION = "_Distortion";

			public const string RIM_COLOR = "_RimColor";

			public const string RIM_SIZE = "_RimSize";

			public const string RIM_INTENSITY = "_RimIntensity";

			public const string SPECULAR_SHININESS = "_Shininess";

			public const string SPEC_COLOR = "_SpecColor";

			public const string SPECULAR_INTENSITY = "_SpecularIntensity";

			public const string EMISSION_COLOR = "_EmissionColor";

			public const string EMISSION = "_Emission";
		}

		public static void SetMainTint(Material material, float tint)
		{
			material.SetFloat("_MainTint", tint);
		}

		public static float GetMainTint(Material material)
		{
			return material.GetFloat("_MainTint");
		}

		public static void SetMainTexture(Material material, Texture tex)
		{
			material.SetTexture("_MainTex", tex);
		}

		public static Texture GetMainTexture(Material material)
		{
			return material.GetTexture("_MainTex");
		}

		public static void SetMainColor(Material material, Color color)
		{
			material.SetColor("_Color", color);
		}

		public static Color GetMainColor(Material material)
		{
			return material.GetColor("_Color");
		}

		public static void SetNormalmap(Material material, Texture tex)
		{
			material.SetTexture("_BumpMap", tex);
		}

		public static Texture GetBumpMap(Material material)
		{
			return material.GetTexture("_BumpMap");
		}

		public static void SetDistortion(Material material, float distortion)
		{
			material.SetFloat("_Distortion", distortion);
		}

		public static float GetDistortion(Material material)
		{
			return material.GetFloat("_Distortion");
		}

		public static void SetRimColor(Material material, Color color)
		{
			material.SetColor("_RimColor", color);
		}

		public static Color GetRimColor(Material material)
		{
			return material.GetColor("_RimColor");
		}

		public static void SetRimSize(Material material, float size)
		{
			material.SetFloat("_RimSize", size);
		}

		public static float GetRimSize(Material material)
		{
			return material.GetFloat("_RimSize");
		}

		public static void SetRimIntensity(Material material, float intensity)
		{
			material.SetFloat("_RimIntensity", intensity);
		}

		public static float GetRimIntensity(Material material)
		{
			return material.GetFloat("_RimIntensity");
		}

		public static void SetSpecularShininess(Material material, float shininess)
		{
			material.SetFloat("_Shininess", shininess);
		}

		public static float GetSpecularShininess(Material material)
		{
			return material.GetFloat("_Shininess");
		}

		public static void SetSpecularColor(Material material, Color color)
		{
			material.SetColor("_SpecColor", color);
		}

		public static Color GetSpecularColor(Material material)
		{
			return material.GetColor("_SpecColor");
		}

		public static void SetSpecularIntensity(Material material, float intensity)
		{
			material.SetFloat("_SpecularIntensity", intensity);
		}

		public static float GetSpecularIntensity(Material material)
		{
			return material.GetFloat("_SpecularIntensity");
		}

		public static void SetEmissionColor(Material material, Color color)
		{
			material.SetColor("_EmissionColor", color);
		}

		public static Color GetEmissionColor(Material material)
		{
			return material.GetColor("_EmissionColor");
		}
	}
}
