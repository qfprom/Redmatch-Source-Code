using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProBuilder.Core
{
	[Serializable]
	public class pb_Face
	{
		[SerializeField]
		private int[] _indices;

		[SerializeField]
		private int[] _distinctIndices;

		[SerializeField]
		private pb_Edge[] _edges;

		[SerializeField]
		private int _smoothingGroup;

		[SerializeField]
		private pb_UV _uv;

		[SerializeField]
		private Material _mat;

		public bool manualUV;

		internal int elementGroup;

		public int textureGroup = -1;

		public int[] indices
		{
			get
			{
				return _indices;
			}
		}

		public int[] distinctIndices
		{
			get
			{
				return (_distinctIndices != null) ? _distinctIndices : CacheDistinctIndices();
			}
		}

		public pb_Edge[] edges
		{
			get
			{
				return (_edges != null) ? _edges : CacheEdges();
			}
		}

		public int smoothingGroup
		{
			get
			{
				return _smoothingGroup;
			}
			set
			{
				_smoothingGroup = value;
			}
		}

		public Material material
		{
			get
			{
				return _mat;
			}
			set
			{
				_mat = value;
			}
		}

		public pb_UV uv
		{
			get
			{
				return _uv;
			}
			set
			{
				_uv = value;
			}
		}

		public int this[int i]
		{
			get
			{
				return indices[i];
			}
		}

		public pb_Face()
		{
		}

		public pb_Face(int[] i)
		{
			SetIndices(i);
			_uv = new pb_UV();
			_mat = pb_Material.DefaultMaterial;
			_smoothingGroup = 0;
			textureGroup = -1;
			elementGroup = 0;
		}

		public pb_Face(int[] i, Material m, pb_UV u, int smoothingGroup, int textureGroup, int elementGroup, bool manualUV)
		{
			SetIndices(i);
			_uv = new pb_UV(u);
			_mat = m;
			_smoothingGroup = smoothingGroup;
			this.textureGroup = textureGroup;
			this.elementGroup = elementGroup;
			this.manualUV = manualUV;
		}

		public pb_Face(pb_Face face)
		{
			_indices = new int[face.indices.Length];
			Array.Copy(face.indices, _indices, face.indices.Length);
			_uv = new pb_UV(face.uv);
			_mat = face.material;
			_smoothingGroup = face.smoothingGroup;
			textureGroup = face.textureGroup;
			elementGroup = face.elementGroup;
			manualUV = face.manualUV;
			RebuildCaches();
		}

		public void CopyFrom(pb_Face other)
		{
			int num = ((other.indices != null) ? other.indices.Length : 0);
			_indices = new int[num];
			Array.Copy(other.indices, _indices, num);
			_smoothingGroup = other.smoothingGroup;
			_uv = new pb_UV(other.uv);
			_mat = other.material;
			manualUV = other.manualUV;
			elementGroup = other.elementGroup;
			RebuildCaches();
		}

		public bool IsValid()
		{
			return indices.Length > 2;
		}

		public pb_Edge[] GetAllEdges()
		{
			pb_Edge[] array = new pb_Edge[indices.Length];
			for (int i = 0; i < indices.Length; i += 3)
			{
				array[i] = new pb_Edge(indices[i], indices[i + 1]);
				array[i + 1] = new pb_Edge(indices[i + 1], indices[i + 2]);
				array[i + 2] = new pb_Edge(indices[i + 2], indices[i]);
			}
			return array;
		}

		public void SetIndices(int[] i)
		{
			_indices = i;
			RebuildCaches();
		}

		public void ShiftIndices(int offset)
		{
			for (int i = 0; i < _indices.Length; i++)
			{
				_indices[i] += offset;
			}
		}

		public int SmallestIndexValue()
		{
			int num = _indices[0];
			for (int i = 0; i < _indices.Length; i++)
			{
				if (_indices[i] < num)
				{
					num = _indices[i];
				}
			}
			return num;
		}

		public void ShiftIndicesToZero()
		{
			int num = SmallestIndexValue();
			for (int i = 0; i < indices.Length; i++)
			{
				_indices[i] -= num;
			}
			for (int j = 0; j < _distinctIndices.Length; j++)
			{
				_distinctIndices[j] -= num;
			}
			for (int k = 0; k < _edges.Length; k++)
			{
				_edges[k].x -= num;
				_edges[k].y -= num;
			}
		}

		public void ReverseIndices()
		{
			Array.Reverse(_indices);
			RebuildCaches();
		}

		public void RebuildCaches()
		{
			CacheDistinctIndices();
			CacheEdges();
		}

		private pb_Edge[] CacheEdges()
		{
			if (_indices == null)
			{
				return null;
			}
			HashSet<pb_Edge> hashSet = new HashSet<pb_Edge>();
			List<pb_Edge> list = new List<pb_Edge>();
			for (int i = 0; i < indices.Length; i += 3)
			{
				pb_Edge item = new pb_Edge(indices[i], indices[i + 1]);
				pb_Edge item2 = new pb_Edge(indices[i + 1], indices[i + 2]);
				pb_Edge item3 = new pb_Edge(indices[i + 2], indices[i]);
				if (!hashSet.Add(item))
				{
					list.Add(item);
				}
				if (!hashSet.Add(item2))
				{
					list.Add(item2);
				}
				if (!hashSet.Add(item3))
				{
					list.Add(item3);
				}
			}
			hashSet.ExceptWith(list);
			_edges = hashSet.ToArray();
			return _edges;
		}

		private int[] CacheDistinctIndices()
		{
			if (_indices == null)
			{
				return null;
			}
			_distinctIndices = new HashSet<int>(_indices).ToArray();
			return distinctIndices;
		}

		public bool Contains(int[] triangle)
		{
			for (int i = 0; i < indices.Length; i += 3)
			{
				if (triangle.Contains(indices[i]) && triangle.Contains(indices[i + 1]) && triangle.Contains(indices[i + 2]))
				{
					return true;
				}
			}
			return false;
		}

		internal static int[] AllTriangles(pb_Face[] q)
		{
			List<int> list = new List<int>(q.Length * 6);
			foreach (pb_Face pb_Face2 in q)
			{
				list.AddRange(pb_Face2.indices);
			}
			return list.ToArray();
		}

		public MeshTopology ToQuadOrTriangles(out int[] quadOrTris)
		{
			if (ToQuad(out quadOrTris))
			{
				return MeshTopology.Quads;
			}
			int num = ((indices != null) ? Math.Max(0, indices.Length) : 0);
			quadOrTris = new int[num];
			Array.Copy(indices, quadOrTris, num);
			return MeshTopology.Triangles;
		}

		public int[] ToQuad()
		{
			int[] quad;
			ToQuad(out quad);
			return quad;
		}

		public bool ToQuad(out int[] quad)
		{
			if (indices == null || indices.Length != 6)
			{
				quad = null;
				return false;
			}
			quad = new int[4]
			{
				edges[0].x,
				edges[0].y,
				-1,
				-1
			};
			if (edges[1].x == quad[1])
			{
				quad[2] = edges[1].y;
			}
			else if (edges[2].x == quad[1])
			{
				quad[2] = edges[2].y;
			}
			else if (edges[3].x == quad[1])
			{
				quad[2] = edges[3].y;
			}
			if (edges[1].x == quad[2])
			{
				quad[3] = edges[1].y;
			}
			else if (edges[2].x == quad[2])
			{
				quad[3] = edges[2].y;
			}
			else if (edges[3].x == quad[2])
			{
				quad[3] = edges[3].y;
			}
			return true;
		}

		public static int GetMeshIndices(pb_Face[] faces, out pb_Submesh[] submeshes, MeshTopology preferredTopology = MeshTopology.Triangles)
		{
			if (preferredTopology != MeshTopology.Triangles && preferredTopology != MeshTopology.Quads)
			{
				throw new NotImplementedException("Currently only Quads and Triangles are supported.");
			}
			bool flag = preferredTopology == MeshTopology.Quads;
			Dictionary<Material, List<int>> dictionary = ((!flag) ? null : new Dictionary<Material, List<int>>());
			Dictionary<Material, List<int>> dictionary2 = new Dictionary<Material, List<int>>();
			int num = ((faces != null) ? faces.Length : 0);
			for (int i = 0; i < num; i++)
			{
				pb_Face pb_Face2 = faces[i];
				if (pb_Face2.indices == null || pb_Face2.indices.Length < 1)
				{
					continue;
				}
				Material key = pb_Face2.material ?? pb_Material.DefaultMaterial;
				List<int> value = null;
				int[] quad;
				if (flag && pb_Face2.ToQuad(out quad))
				{
					if (dictionary.TryGetValue(key, out value))
					{
						value.AddRange(quad);
					}
					else
					{
						dictionary.Add(key, new List<int>(quad));
					}
				}
				else if (dictionary2.TryGetValue(key, out value))
				{
					value.AddRange(pb_Face2.indices);
				}
				else
				{
					dictionary2.Add(key, new List<int>(pb_Face2.indices));
				}
			}
			int num2 = ((dictionary != null) ? dictionary.Count : 0) + dictionary2.Count;
			submeshes = new pb_Submesh[num2];
			int num3 = 0;
			if (dictionary != null)
			{
				foreach (KeyValuePair<Material, List<int>> item in dictionary)
				{
					submeshes[num3++] = new pb_Submesh(item.Key, MeshTopology.Quads, item.Value.ToArray());
				}
			}
			foreach (KeyValuePair<Material, List<int>> item2 in dictionary2)
			{
				submeshes[num3++] = new pb_Submesh(item2.Key, MeshTopology.Triangles, item2.Value.ToArray());
			}
			return num2;
		}

		public override string ToString()
		{
			if (indices.Length % 3 != 0)
			{
				return "Index count is not a multiple of 3.";
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < indices.Length; i += 3)
			{
				stringBuilder.Append("[");
				stringBuilder.Append(indices[i]);
				stringBuilder.Append(", ");
				stringBuilder.Append(indices[i + 1]);
				stringBuilder.Append(", ");
				stringBuilder.Append(indices[i + 2]);
				stringBuilder.Append("]");
				if (i < indices.Length - 3)
				{
					stringBuilder.Append(", ");
				}
			}
			return stringBuilder.ToString();
		}
	}
}
