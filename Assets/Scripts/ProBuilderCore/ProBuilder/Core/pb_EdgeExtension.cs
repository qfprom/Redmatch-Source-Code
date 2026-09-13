using System.Collections.Generic;

namespace ProBuilder.Core
{
	internal static class pb_EdgeExtension
	{
		public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, Dictionary<int, int> sharedIndicesLookup)
		{
			pb_Edge[] array = new pb_Edge[edges.Length];
			for (int i = 0; i < edges.Length; i++)
			{
				array[i] = new pb_Edge(sharedIndicesLookup[edges[i].x], sharedIndicesLookup[edges[i].y]);
			}
			return array;
		}

		public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, pb_IntArray[] sharedIndices)
		{
			return GetUniversalEdges(edges, sharedIndices.ToDictionary());
		}

		internal static pb_Edge GetLocalEdgeFast(pb_Edge edge, pb_IntArray[] sharedIndices)
		{
			return new pb_Edge(sharedIndices[edge.x][0], sharedIndices[edge.y][0]);
		}

		public static bool ValidateEdge(pb_Object pb, pb_Edge edge, out pb_Tuple<pb_Face, pb_Edge> validEdge)
		{
			pb_Face[] faces = pb.faces;
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_Edge pb_Edge2 = new pb_Edge(sharedIndices.IndexOf(edge.x), sharedIndices.IndexOf(edge.y));
			int index_a = -1;
			int index_a2 = -1;
			int index_b = -1;
			int index_b2 = -1;
			for (int i = 0; i < faces.Length; i++)
			{
				if (faces[i].distinctIndices.ContainsMatch(sharedIndices[pb_Edge2.x].array, out index_a, out index_b) && faces[i].distinctIndices.ContainsMatch(sharedIndices[pb_Edge2.y].array, out index_a2, out index_b2))
				{
					int x = faces[i].distinctIndices[index_a];
					int y = faces[i].distinctIndices[index_a2];
					validEdge = new pb_Tuple<pb_Face, pb_Edge>(faces[i], new pb_Edge(x, y));
					return true;
				}
			}
			validEdge = null;
			return false;
		}

		internal static pb_Edge[] AllEdges(pb_Face[] faces)
		{
			List<pb_Edge> list = new List<pb_Edge>();
			foreach (pb_Face pb_Face2 in faces)
			{
				list.AddRange(pb_Face2.edges);
			}
			return list.ToArray();
		}

		internal static bool Contains(this pb_Edge[] edges, pb_Edge edge)
		{
			for (int i = 0; i < edges.Length; i++)
			{
				if (edges[i].Equals(edge))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool Contains(this pb_Edge[] edges, int x, int y)
		{
			for (int i = 0; i < edges.Length; i++)
			{
				if ((x == edges[i].x && y == edges[i].y) || (x == edges[i].y && y == edges[i].x))
				{
					return true;
				}
			}
			return false;
		}

		internal static int IndexOf(this IList<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)
		{
			for (int i = 0; i < edges.Count; i++)
			{
				if (edges[i].Equals(edge, lookup))
				{
					return i;
				}
			}
			return -1;
		}

		internal static int[] AllTriangles(this pb_Edge[] edges)
		{
			int[] array = new int[edges.Length * 2];
			int num = 0;
			for (int i = 0; i < edges.Length; i++)
			{
				array[num++] = edges[i].x;
				array[num++] = edges[i].y;
			}
			return array;
		}

		internal static List<int> AllTriangles(this List<pb_Edge> edges)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < edges.Count; i++)
			{
				list.Add(edges[i].x);
				list.Add(edges[i].y);
			}
			return list;
		}
	}
}
