using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace ProBuilder.Core
{
	public static class pb_Material
	{
		private static Shader s_SelectionPickerShader;

		private static Material s_DefaultMaterial;

		private static Material s_FacePickerMaterial;

		private static Material s_VertexPickerMaterial;

		private static Material s_EdgePickerMaterial;

		private static Material s_UnityDefaultDiffuse;

		private static Material s_UnlitVertexColorMaterial;

		internal static Shader SelectionPickerShader
		{
			get
			{
				if (s_SelectionPickerShader == null)
				{
					s_SelectionPickerShader = Shader.Find("Hidden/ProBuilder/SelectionPicker");
				}
				return s_SelectionPickerShader;
			}
		}

		public static Material DefaultMaterial
		{
			get
			{
				if (s_DefaultMaterial == null)
				{
					RenderPipelineAsset renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
					if (renderPipelineAsset != null)
					{
						s_DefaultMaterial = renderPipelineAsset.GetDefaultMaterial();
					}
					else
					{
						s_DefaultMaterial = (Material)Resources.Load("Materials/Default_Prototype", typeof(Material));
						if (s_DefaultMaterial == null || !s_DefaultMaterial.shader.isSupported)
						{
							s_DefaultMaterial = UnityDefaultDiffuse;
						}
					}
				}
				return s_DefaultMaterial;
			}
		}

		internal static Material FacePickerMaterial
		{
			get
			{
				if (s_FacePickerMaterial == null)
				{
					Shader shader = Shader.Find("Hidden/ProBuilder/FacePicker");
					if (shader == null)
					{
						pb_Log.Error("pb_FacePicker.shader not found! Re-import ProBuilder to fix.");
					}
					if (s_FacePickerMaterial == null)
					{
						s_FacePickerMaterial = new Material(shader);
					}
					else
					{
						s_FacePickerMaterial.shader = shader;
					}
				}
				return s_FacePickerMaterial;
			}
		}

		internal static Material VertexPickerMaterial
		{
			get
			{
				if (s_VertexPickerMaterial == null)
				{
					s_VertexPickerMaterial = Resources.Load<Material>("Materials/VertexPicker");
					Shader shader = Shader.Find("Hidden/ProBuilder/VertexPicker");
					if (shader == null)
					{
						pb_Log.Error("pb_VertexPicker.shader not found! Re-import ProBuilder to fix.");
					}
					if (s_VertexPickerMaterial == null)
					{
						s_VertexPickerMaterial = new Material(shader);
					}
					else
					{
						s_VertexPickerMaterial.shader = shader;
					}
				}
				return s_VertexPickerMaterial;
			}
		}

		internal static Material EdgePickerMaterial
		{
			get
			{
				if (s_EdgePickerMaterial == null)
				{
					s_EdgePickerMaterial = Resources.Load<Material>("Materials/EdgePicker");
					Shader shader = Shader.Find("Hidden/ProBuilder/EdgePicker");
					if (shader == null)
					{
						pb_Log.Error("pb_EdgePicker.shader not found! Re-import ProBuilder to fix.");
					}
					if (s_EdgePickerMaterial == null)
					{
						s_EdgePickerMaterial = new Material(shader);
					}
					else
					{
						s_EdgePickerMaterial.shader = shader;
					}
				}
				return s_EdgePickerMaterial;
			}
		}

		internal static Material TriggerMaterial
		{
			get
			{
				return (Material)Resources.Load("Materials/Trigger", typeof(Material));
			}
		}

		internal static Material ColliderMaterial
		{
			get
			{
				return (Material)Resources.Load("Materials/Collider", typeof(Material));
			}
		}

		[Obsolete("NoDraw is no longer supported.")]
		internal static Material NoDrawMaterial
		{
			get
			{
				return (Material)Resources.Load("Materials/NoDraw", typeof(Material));
			}
		}

		internal static Material UnityDefaultDiffuse
		{
			get
			{
				if (s_UnityDefaultDiffuse == null)
				{
					MethodInfo method = typeof(Material).GetMethod("GetDefaultMaterial", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						s_UnityDefaultDiffuse = method.Invoke(null, null) as Material;
					}
					if (s_UnityDefaultDiffuse == null)
					{
						GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
						s_UnityDefaultDiffuse = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
						UnityEngine.Object.DestroyImmediate(gameObject);
					}
				}
				return s_UnityDefaultDiffuse;
			}
		}

		internal static Material UnlitVertexColor
		{
			get
			{
				if (s_UnlitVertexColorMaterial == null)
				{
					s_UnlitVertexColorMaterial = (Material)Resources.Load("Materials/UnlitVertexColor", typeof(Material));
				}
				return s_UnlitVertexColorMaterial;
			}
		}
	}
}
