using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.PostProcessing
{
	public sealed class PropertySheetFactory
	{
		private readonly Dictionary<Shader, PropertySheet> m_Sheets;

		public PropertySheetFactory()
		{
			m_Sheets = new Dictionary<Shader, PropertySheet>();
		}

		[Obsolete("Use PropertySheet.Get(Shader) with a direct reference to the Shader instead.")]
		public PropertySheet Get(string shaderName)
		{
			Shader shader = Shader.Find(shaderName);
			if (shader == null)
			{
				throw new ArgumentException(string.Format("Invalid shader ({0})", shaderName));
			}
			return Get(shader);
		}

		public PropertySheet Get(Shader shader)
		{
			if (shader == null)
			{
				throw new ArgumentException(string.Format("Invalid shader ({0})", shader));
			}
			PropertySheet value;
			if (m_Sheets.TryGetValue(shader, out value))
			{
				return value;
			}
			string name = shader.name;
			Material material = new Material(shader);
			material.name = string.Format("PostProcess - {0}", name.Substring(name.LastIndexOf('/') + 1));
			material.hideFlags = HideFlags.DontSave;
			Material material2 = material;
			value = new PropertySheet(material2);
			m_Sheets.Add(shader, value);
			return value;
		}

		public void Release()
		{
			foreach (PropertySheet value in m_Sheets.Values)
			{
				value.Release();
			}
			m_Sheets.Clear();
		}
	}
}
