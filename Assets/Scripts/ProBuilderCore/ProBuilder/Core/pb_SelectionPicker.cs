using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProBuilder.Core
{
	internal static class pb_SelectionPicker
	{
		private const string k_FacePickerOcclusionTintUniform = "_Tint";

		private static readonly Color k_Blackf = new Color(0f, 0f, 0f, 1f);

		private static readonly Color k_Whitef = new Color(1f, 1f, 1f, 1f);

		private const uint k_PickerHashNone = 0u;

		private const uint k_PickerHashMin = 1u;

		private const uint k_PickerHashMax = 16777215u;

		private const uint k_MinEdgePixelsForValidSelection = 1u;

		private static bool s_Initialized = false;

		private static RenderTextureFormat s_RenderTextureFormat = RenderTextureFormat.Default;

		private static RenderTextureFormat[] s_PreferredFormats = new RenderTextureFormat[2]
		{
			RenderTextureFormat.ARGB32,
			RenderTextureFormat.ARGBFloat
		};

		private static RenderTextureFormat renderTextureFormat
		{
			get
			{
				if (s_Initialized)
				{
					return s_RenderTextureFormat;
				}
				s_Initialized = true;
				for (int i = 0; i < s_PreferredFormats.Length; i++)
				{
					if (SystemInfo.SupportsRenderTextureFormat(s_PreferredFormats[i]))
					{
						s_RenderTextureFormat = s_PreferredFormats[i];
						break;
					}
				}
				return s_RenderTextureFormat;
			}
		}

		private static TextureFormat textureFormat
		{
			get
			{
				return TextureFormat.ARGB32;
			}
		}

		public static Dictionary<pb_Object, HashSet<pb_Face>> PickFacesInRect(Camera camera, Rect pickerRect, IList<pb_Object> selection, int renderTextureWidth = -1, int renderTextureHeight = -1)
		{
			Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map;
			Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);
			Color32[] pixels = texture2D.GetPixels32();
			int num = Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int num2 = Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
			int width = texture2D.width;
			int height = texture2D.height;
			int num3 = Mathf.FloorToInt(pickerRect.width);
			int num4 = Mathf.FloorToInt(pickerRect.height);
			UnityEngine.Object.DestroyImmediate(texture2D);
			Dictionary<pb_Object, HashSet<pb_Face>> dictionary = new Dictionary<pb_Object, HashSet<pb_Face>>();
			HashSet<pb_Face> value = null;
			HashSet<uint> hashSet = new HashSet<uint>();
			for (int i = num2; i < Math.Min(num2 + num4, height); i++)
			{
				for (int j = num; j < Math.Min(num + num3, width); j++)
				{
					uint num5 = DecodeRGBA(pixels[i * width + j]);
					pb_Tuple<pb_Object, pb_Face> value2;
					if (hashSet.Add(num5) && map.TryGetValue(num5, out value2))
					{
						if (dictionary.TryGetValue(value2.Item1, out value))
						{
							value.Add(value2.Item2);
							continue;
						}
						dictionary.Add(value2.Item1, new HashSet<pb_Face> { value2.Item2 });
					}
				}
			}
			return dictionary;
		}

		public static Dictionary<pb_Object, HashSet<int>> PickVerticesInRect(Camera camera, Rect pickerRect, IList<pb_Object> selection, bool doDepthTest, int renderTextureWidth = -1, int renderTextureHeight = -1)
		{
			Dictionary<pb_Object, HashSet<int>> dictionary = new Dictionary<pb_Object, HashSet<int>>();
			Dictionary<uint, pb_Tuple<pb_Object, int>> map;
			Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, doDepthTest, out map, renderTextureWidth, renderTextureHeight);
			Color32[] pixels = texture2D.GetPixels32();
			int num = Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int num2 = Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
			int width = texture2D.width;
			int height = texture2D.height;
			int num3 = Mathf.FloorToInt(pickerRect.width);
			int num4 = Mathf.FloorToInt(pickerRect.height);
			UnityEngine.Object.DestroyImmediate(texture2D);
			HashSet<int> value = null;
			HashSet<uint> hashSet = new HashSet<uint>();
			for (int i = num2; i < Math.Min(num2 + num4, height); i++)
			{
				for (int j = num; j < Math.Min(num + num3, width); j++)
				{
					uint num5 = DecodeRGBA(pixels[i * width + j]);
					pb_Tuple<pb_Object, int> value2;
					if (hashSet.Add(num5) && map.TryGetValue(num5, out value2))
					{
						if (dictionary.TryGetValue(value2.Item1, out value))
						{
							value.Add(value2.Item2);
							continue;
						}
						dictionary.Add(value2.Item1, new HashSet<int> { value2.Item2 });
					}
				}
			}
			return dictionary;
		}

		public static Dictionary<pb_Object, HashSet<pb_Edge>> PickEdgesInRect(Camera camera, Rect pickerRect, IList<pb_Object> selection, bool doDepthTest, int renderTextureWidth = -1, int renderTextureHeight = -1)
		{
			Dictionary<pb_Object, HashSet<pb_Edge>> dictionary = new Dictionary<pb_Object, HashSet<pb_Edge>>();
			Dictionary<uint, pb_Tuple<pb_Object, pb_Edge>> map;
			Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, doDepthTest, out map, renderTextureWidth, renderTextureHeight);
			Color32[] pixels = texture2D.GetPixels32();
			int num = Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int num2 = Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
			int width = texture2D.width;
			int height = texture2D.height;
			int num3 = Mathf.FloorToInt(pickerRect.width);
			int num4 = Mathf.FloorToInt(pickerRect.height);
			UnityEngine.Object.DestroyImmediate(texture2D);
			Dictionary<uint, uint> dictionary2 = new Dictionary<uint, uint>();
			for (int i = num2; i < Math.Min(num2 + num4, height); i++)
			{
				for (int j = num; j < Math.Min(num + num3, width); j++)
				{
					uint num5 = DecodeRGBA(pixels[i * width + j]);
					if (num5 != 0 && num5 != 16777215)
					{
						if (!dictionary2.ContainsKey(num5))
						{
							dictionary2.Add(num5, 1u);
						}
						else
						{
							dictionary2[num5]++;
						}
					}
				}
			}
			foreach (KeyValuePair<uint, uint> item in dictionary2)
			{
				pb_Tuple<pb_Object, pb_Edge> value;
				if (item.Value > 1 && map.TryGetValue(item.Key, out value))
				{
					HashSet<pb_Edge> value2 = null;
					if (dictionary.TryGetValue(value.Item1, out value2))
					{
						value2.Add(value.Item2);
						continue;
					}
					dictionary.Add(value.Item1, new HashSet<pb_Edge> { value.Item2 });
				}
			}
			return dictionary;
		}

		private static Texture2D RenderSelectionPickerTexture(Camera camera, IList<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map, int width = -1, int height = -1)
		{
			GameObject[] array = GenerateFacePickingObjects(selection, out map);
			pb_Material.FacePickerMaterial.SetColor("_Tint", k_Whitef);
			Texture2D result = RenderWithReplacementShader(camera, pb_Material.SelectionPickerShader, "ProBuilderPicker", width, height);
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			return result;
		}

		private static Texture2D RenderSelectionPickerTexture(Camera camera, IList<pb_Object> selection, bool doDepthTest, out Dictionary<uint, pb_Tuple<pb_Object, int>> map, int width = -1, int height = -1)
		{
			GameObject[] depthObjects;
			GameObject[] pickerObjects;
			GenerateVertexPickingObjects(selection, doDepthTest, out map, out depthObjects, out pickerObjects);
			pb_Material.FacePickerMaterial.SetColor("_Tint", k_Blackf);
			Texture2D result = RenderWithReplacementShader(camera, pb_Material.SelectionPickerShader, "ProBuilderPicker", width, height);
			int i = 0;
			for (int num = pickerObjects.Length; i < num; i++)
			{
				UnityEngine.Object.DestroyImmediate(pickerObjects[i].GetComponent<MeshFilter>().sharedMesh);
				UnityEngine.Object.DestroyImmediate(pickerObjects[i]);
			}
			if (doDepthTest)
			{
				int j = 0;
				for (int num2 = depthObjects.Length; j < num2; j++)
				{
					UnityEngine.Object.DestroyImmediate(depthObjects[j]);
				}
			}
			return result;
		}

		private static Texture2D RenderSelectionPickerTexture(Camera camera, IList<pb_Object> selection, bool doDepthTest, out Dictionary<uint, pb_Tuple<pb_Object, pb_Edge>> map, int width = -1, int height = -1)
		{
			GameObject[] depthObjects;
			GameObject[] pickerObjects;
			GenerateEdgePickingObjects(selection, doDepthTest, out map, out depthObjects, out pickerObjects);
			pb_Material.FacePickerMaterial.SetColor("_Tint", k_Blackf);
			Texture2D result = RenderWithReplacementShader(camera, pb_Material.SelectionPickerShader, "ProBuilderPicker", width, height);
			int i = 0;
			for (int num = pickerObjects.Length; i < num; i++)
			{
				UnityEngine.Object.DestroyImmediate(pickerObjects[i].GetComponent<MeshFilter>().sharedMesh);
				UnityEngine.Object.DestroyImmediate(pickerObjects[i]);
			}
			if (doDepthTest)
			{
				int j = 0;
				for (int num2 = depthObjects.Length; j < num2; j++)
				{
					UnityEngine.Object.DestroyImmediate(depthObjects[j]);
				}
			}
			return result;
		}

		private static GameObject[] GenerateFacePickingObjects(IList<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map)
		{
			int count = selection.Count;
			GameObject[] array = new GameObject[count];
			map = new Dictionary<uint, pb_Tuple<pb_Object, pb_Face>>();
			uint num = 0u;
			for (int i = 0; i < count; i++)
			{
				pb_Object pb_Object2 = selection[i];
				GameObject gameObject = pb_Util.EmptyGameObjectWithTransform(pb_Object2.transform);
				gameObject.name = pb_Object2.name + " (Face Depth Test)";
				Mesh mesh = new Mesh();
				mesh.vertices = pb_Object2.vertices;
				mesh.triangles = pb_Object2.faces.SelectMany((pb_Face x) => x.indices).ToArray();
				Color32[] array2 = new Color32[mesh.vertexCount];
				pb_Face[] faces = pb_Object2.faces;
				foreach (pb_Face pb_Face2 in faces)
				{
					Color32 color = EncodeRGBA(num++);
					map.Add(DecodeRGBA(color), new pb_Tuple<pb_Object, pb_Face>(pb_Object2, pb_Face2));
					for (int num3 = 0; num3 < pb_Face2.distinctIndices.Length; num3++)
					{
						array2[pb_Face2.distinctIndices[num3]] = color;
					}
				}
				mesh.colors32 = array2;
				gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
				gameObject.AddComponent<MeshRenderer>().sharedMaterial = pb_Material.FacePickerMaterial;
				array[i] = gameObject;
			}
			return array;
		}

		private static void GenerateVertexPickingObjects(IList<pb_Object> selection, bool doDepthTest, out Dictionary<uint, pb_Tuple<pb_Object, int>> map, out GameObject[] depthObjects, out GameObject[] pickerObjects)
		{
			map = new Dictionary<uint, pb_Tuple<pb_Object, int>>();
			uint index = 2u;
			int count = selection.Count;
			pickerObjects = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				pb_Object pb_Object2 = selection[i];
				GameObject gameObject = pb_Util.EmptyGameObjectWithTransform(pb_Object2.transform);
				gameObject.name = pb_Object2.name + "  (Vertex Billboards)";
				gameObject.AddComponent<MeshFilter>().sharedMesh = BuildVertexMesh(pb_Object2, map, ref index);
				gameObject.AddComponent<MeshRenderer>().sharedMaterial = pb_Material.VertexPickerMaterial;
				pickerObjects[i] = gameObject;
			}
			if (doDepthTest)
			{
				depthObjects = new GameObject[count];
				for (int j = 0; j < count; j++)
				{
					pb_Object pb_Object3 = selection[j];
					GameObject gameObject2 = pb_Util.EmptyGameObjectWithTransform(pb_Object3.transform);
					gameObject2.name = pb_Object3.name + "  (Depth Mask)";
					gameObject2.AddComponent<MeshFilter>().sharedMesh = pb_Object3.msh;
					gameObject2.AddComponent<MeshRenderer>().sharedMaterial = pb_Material.FacePickerMaterial;
					depthObjects[j] = gameObject2;
				}
			}
			else
			{
				depthObjects = null;
			}
		}

		private static void GenerateEdgePickingObjects(IList<pb_Object> selection, bool doDepthTest, out Dictionary<uint, pb_Tuple<pb_Object, pb_Edge>> map, out GameObject[] depthObjects, out GameObject[] pickerObjects)
		{
			map = new Dictionary<uint, pb_Tuple<pb_Object, pb_Edge>>();
			uint index = 2u;
			int count = selection.Count;
			pickerObjects = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				pb_Object pb_Object2 = selection[i];
				GameObject gameObject = pb_Util.EmptyGameObjectWithTransform(pb_Object2.transform);
				gameObject.name = pb_Object2.name + "  (Edge Billboards)";
				gameObject.AddComponent<MeshFilter>().sharedMesh = BuildEdgeMesh(pb_Object2, map, ref index);
				gameObject.AddComponent<MeshRenderer>().sharedMaterial = pb_Material.EdgePickerMaterial;
				pickerObjects[i] = gameObject;
			}
			if (doDepthTest)
			{
				depthObjects = new GameObject[count];
				for (int j = 0; j < count; j++)
				{
					pb_Object pb_Object3 = selection[j];
					GameObject gameObject2 = pb_Util.EmptyGameObjectWithTransform(pb_Object3.transform);
					gameObject2.name = pb_Object3.name + "  (Depth Mask)";
					gameObject2.AddComponent<MeshFilter>().sharedMesh = pb_Object3.msh;
					gameObject2.AddComponent<MeshRenderer>().sharedMaterial = pb_Material.FacePickerMaterial;
					depthObjects[j] = gameObject2;
				}
			}
			else
			{
				depthObjects = null;
			}
		}

		private static Mesh BuildVertexMesh(pb_Object pb, Dictionary<uint, pb_Tuple<pb_Object, int>> map, ref uint index)
		{
			int num = Math.Min(pb.sharedIndices.Length, 16382);
			Vector3[] array = new Vector3[num * 4];
			Vector2[] array2 = new Vector2[num * 4];
			Vector2[] array3 = new Vector2[num * 4];
			Color[] array4 = new Color[num * 4];
			int[] array5 = new int[num * 6];
			int num2 = 0;
			int num3 = 0;
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = pb.vertices[pb.sharedIndices[i][0]];
				array[num3] = vector;
				array[num3 + 1] = vector;
				array[num3 + 2] = vector;
				array[num3 + 3] = vector;
				array2[num3] = Vector3.zero;
				array2[num3 + 1] = Vector3.right;
				array2[num3 + 2] = Vector3.up;
				array2[num3 + 3] = Vector3.one;
				array3[num3] = -up - right;
				array3[num3 + 1] = -up + right;
				array3[num3 + 2] = up - right;
				array3[num3 + 3] = up + right;
				array5[num2] = num3;
				array5[num2 + 1] = num3 + 1;
				array5[num2 + 2] = num3 + 2;
				array5[num2 + 3] = num3 + 1;
				array5[num2 + 4] = num3 + 3;
				array5[num2 + 5] = num3 + 2;
				Color32 color = EncodeRGBA(index);
				map.Add(index++, new pb_Tuple<pb_Object, int>(pb, i));
				array4[num3] = color;
				array4[num3 + 1] = color;
				array4[num3 + 2] = color;
				array4[num3 + 3] = color;
				num3 += 4;
				num2 += 6;
			}
			Mesh mesh = new Mesh();
			mesh.name = "Vertex Billboard";
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.uv2 = array3;
			mesh.colors = array4;
			mesh.triangles = array5;
			return mesh;
		}

		private static Mesh BuildEdgeMesh(pb_Object pb, Dictionary<uint, pb_Tuple<pb_Object, pb_Edge>> map, ref uint index)
		{
			int num = 0;
			int faceCount = pb.faceCount;
			for (int i = 0; i < faceCount; i++)
			{
				num += pb.faces[i].edges.Length;
			}
			int num2 = Math.Min(num, 32766);
			Vector3[] array = new Vector3[num2 * 2];
			Color32[] array2 = new Color32[num2 * 2];
			int[] array3 = new int[num2 * 2];
			int num3 = 0;
			for (int j = 0; j < faceCount; j++)
			{
				if (num3 >= num2)
				{
					break;
				}
				for (int k = 0; k < pb.faces[j].edges.Length; k++)
				{
					if (num3 >= num2)
					{
						break;
					}
					pb_Edge item = pb.faces[j].edges[k];
					Vector3 vector = pb.vertices[item.x];
					Vector3 vector2 = pb.vertices[item.y];
					int num4 = num3 * 2;
					array[num4] = vector;
					array[num4 + 1] = vector2;
					Color32 color = EncodeRGBA(index);
					map.Add(index++, new pb_Tuple<pb_Object, pb_Edge>(pb, item));
					array2[num4] = color;
					array2[num4 + 1] = color;
					array3[num4] = num4;
					array3[num4 + 1] = num4 + 1;
					num3++;
				}
			}
			Mesh mesh = new Mesh();
			mesh.name = "Edge Billboard";
			mesh.vertices = array;
			mesh.colors32 = array2;
			mesh.subMeshCount = 1;
			mesh.SetIndices(array3, MeshTopology.Lines, 0);
			return mesh;
		}

		public static uint DecodeRGBA(Color32 color)
		{
			uint r = color.r;
			uint g = color.g;
			uint b = color.b;
			if (BitConverter.IsLittleEndian)
			{
				return (r << 16) | (g << 8) | b;
			}
			return (r << 24) | (g << 16) | (b << 8);
		}

		public static Color32 EncodeRGBA(uint hash)
		{
			if (BitConverter.IsLittleEndian)
			{
				return new Color32((byte)((hash >> 16) & 0xFF), (byte)((hash >> 8) & 0xFF), (byte)(hash & 0xFF), byte.MaxValue);
			}
			return new Color32((byte)((hash >> 24) & 0xFF), (byte)((hash >> 16) & 0xFF), (byte)((hash >> 8) & 0xFF), byte.MaxValue);
		}

		private static Texture2D RenderWithReplacementShader(Camera camera, Shader shader, string tag, int width = -1, int height = -1)
		{
			bool flag = width < 0 || height < 0;
			int num = ((!flag) ? width : ((int)camera.pixelRect.width));
			int num2 = ((!flag) ? height : ((int)camera.pixelRect.height));
			GameObject gameObject = new GameObject();
			Camera camera2 = gameObject.AddComponent<Camera>();
			camera2.CopyFrom(camera);
			camera2.renderingPath = RenderingPath.Forward;
			camera2.enabled = false;
			camera2.clearFlags = CameraClearFlags.Color;
			camera2.backgroundColor = Color.white;
			camera2.allowHDR = false;
			camera2.allowMSAA = false;
			camera2.forceIntoRenderTexture = true;
			RenderTextureDescriptor desc = new RenderTextureDescriptor
			{
				width = num,
				height = num2,
				colorFormat = renderTextureFormat,
				autoGenerateMips = false,
				depthBufferBits = 16,
				dimension = TextureDimension.Tex2D,
				enableRandomWrite = false,
				memoryless = RenderTextureMemoryless.None,
				sRGB = false,
				useMipMap = false,
				volumeDepth = 1,
				msaaSamples = 1
			};
			RenderTexture temporary = RenderTexture.GetTemporary(desc);
			RenderTexture active = RenderTexture.active;
			camera2.targetTexture = temporary;
			RenderTexture.active = temporary;
			camera2.RenderWithShader(shader, tag);
			Texture2D texture2D = new Texture2D(num, num2, textureFormat, false, false);
			texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			UnityEngine.Object.DestroyImmediate(gameObject);
			return texture2D;
		}
	}
}
