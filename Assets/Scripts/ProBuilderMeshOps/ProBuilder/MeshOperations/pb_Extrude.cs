using System;
using System.Collections.Generic;
using ProBuilder.Core;
using UnityEngine;

namespace ProBuilder.MeshOperations
{
	public static class pb_Extrude
	{
		public static bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)
		{
			if (method == ExtrudeMethod.IndividualFaces)
			{
				return ExtrudePerFace(pb, faces, distance);
			}
			return ExtrudeAsGroups(pb, faces, method == ExtrudeMethod.FaceNormal, distance);
		}

		private static bool ExtrudePerFace(pb_Object pb, pb_Face[] faces, float distance)
		{
			if (faces == null || faces.Length < 1)
			{
				return false;
			}
			List<pb_Vertex> list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			int num = pb.sharedIndices.Length;
			int num2 = 0;
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> dictionary2 = pb.sharedIndicesUV.ToDictionary();
			List<pb_Face> list2 = new List<pb_Face>(pb.faces);
			Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
			foreach (pb_Face pb_Face2 in faces)
			{
				pb_Face2.smoothingGroup = 0;
				pb_Face2.textureGroup = -1;
				Vector3 vector = pb_Math.Normal(pb, pb_Face2) * distance;
				pb_Edge[] edges = pb_Face2.edges;
				dictionary3.Clear();
				for (int j = 0; j < edges.Length; j++)
				{
					int count = list.Count;
					int x = edges[j].x;
					int y = edges[j].y;
					if (!dictionary3.ContainsKey(x))
					{
						dictionary3.Add(x, dictionary[x]);
						dictionary[x] = num + num2++;
					}
					if (!dictionary3.ContainsKey(y))
					{
						dictionary3.Add(y, dictionary[y]);
						dictionary[y] = num + num2++;
					}
					dictionary.Add(count, dictionary3[x]);
					dictionary.Add(count + 1, dictionary3[y]);
					dictionary.Add(count + 2, dictionary[x]);
					dictionary.Add(count + 3, dictionary[y]);
					pb_Vertex pb_Vertex2 = new pb_Vertex(list[x]);
					pb_Vertex pb_Vertex3 = new pb_Vertex(list[y]);
					pb_Vertex2.position += vector;
					pb_Vertex3.position += vector;
					list.Add(new pb_Vertex(list[x]));
					list.Add(new pb_Vertex(list[y]));
					list.Add(pb_Vertex2);
					list.Add(pb_Vertex3);
					pb_Face item = new pb_Face(new int[6]
					{
						count,
						count + 1,
						count + 2,
						count + 1,
						count + 3,
						count + 2
					}, pb_Face2.material, new pb_UV(pb_Face2.uv), pb_Face2.smoothingGroup, -1, -1, false);
					list2.Add(item);
				}
				for (int k = 0; k < pb_Face2.distinctIndices.Length; k++)
				{
					list[pb_Face2.distinctIndices[k]].position.x += vector.x;
					list[pb_Face2.distinctIndices[k]].position.y += vector.y;
					list[pb_Face2.distinctIndices[k]].position.z += vector.z;
					if (dictionary2 != null && dictionary2.ContainsKey(pb_Face2.distinctIndices[k]))
					{
						dictionary2.Remove(pb_Face2.distinctIndices[k]);
					}
				}
			}
			pb.SetVertices(list);
			pb.SetFaces(list2.ToArray());
			pb.SetSharedIndices(dictionary);
			pb.SetSharedIndicesUV(dictionary2);
			return true;
		}

		private static bool ExtrudeAsGroups(pb_Object pb, pb_Face[] faces, bool compensateAngleVertexDistance, float distance)
		{
			if (faces == null || faces.Length < 1)
			{
				return false;
			}
			List<pb_Vertex> list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			int num = pb.sharedIndices.Length;
			int num2 = 0;
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> dictionary2 = pb.sharedIndicesUV.ToDictionary();
			List<pb_Face> list2 = new List<pb_Face>(pb.faces);
			Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
			Dictionary<int, int> dictionary4 = new Dictionary<int, int>();
			Dictionary<int, int> dictionary5 = new Dictionary<int, int>();
			Dictionary<int, pb_Tuple<Vector3, Vector3, List<int>>> dictionary6 = new Dictionary<int, pb_Tuple<Vector3, Vector3, List<int>>>();
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb, faces, true, dictionary);
			List<HashSet<pb_Face>> faceGroups = GetFaceGroups(wingedEdges);
			foreach (HashSet<pb_Face> item2 in faceGroups)
			{
				Dictionary<pb_EdgeLookup, pb_Face> perimeterEdges = GetPerimeterEdges(item2, dictionary);
				dictionary4.Clear();
				dictionary3.Clear();
				foreach (KeyValuePair<pb_EdgeLookup, pb_Face> item3 in perimeterEdges)
				{
					pb_EdgeLookup key = item3.Key;
					pb_Face value = item3.Value;
					int count = list.Count;
					int x = key.local.x;
					int y = key.local.y;
					if (!dictionary3.ContainsKey(x))
					{
						dictionary3.Add(x, dictionary[x]);
						int value2 = -1;
						if (dictionary4.TryGetValue(dictionary[x], out value2))
						{
							dictionary[x] = value2;
						}
						else
						{
							value2 = num + num2++;
							dictionary4.Add(dictionary[x], value2);
							dictionary[x] = value2;
						}
					}
					if (!dictionary3.ContainsKey(y))
					{
						dictionary3.Add(y, dictionary[y]);
						int value3 = -1;
						if (dictionary4.TryGetValue(dictionary[y], out value3))
						{
							dictionary[y] = value3;
						}
						else
						{
							value3 = num + num2++;
							dictionary4.Add(dictionary[y], value3);
							dictionary[y] = value3;
						}
					}
					dictionary.Add(count, dictionary3[x]);
					dictionary.Add(count + 1, dictionary3[y]);
					dictionary.Add(count + 2, dictionary[x]);
					dictionary.Add(count + 3, dictionary[y]);
					dictionary5.Add(count + 2, x);
					dictionary5.Add(count + 3, y);
					list.Add(new pb_Vertex(list[x]));
					list.Add(new pb_Vertex(list[y]));
					list.Add(null);
					list.Add(null);
					pb_Face item = new pb_Face(new int[6]
					{
						count,
						count + 1,
						count + 2,
						count + 1,
						count + 3,
						count + 2
					}, value.material, new pb_UV(value.uv), value.smoothingGroup, -1, -1, false);
					list2.Add(item);
				}
				foreach (pb_Face item4 in item2)
				{
					item4.textureGroup = -1;
					Vector3 vector = pb_Math.Normal(pb, item4);
					for (int i = 0; i < item4.distinctIndices.Length; i++)
					{
						int num3 = item4.distinctIndices[i];
						if (!dictionary3.ContainsKey(num3) && dictionary4.ContainsKey(dictionary[num3]))
						{
							dictionary[num3] = dictionary4[dictionary[num3]];
						}
						int key2 = dictionary[num3];
						if (dictionary2 != null && dictionary2.ContainsKey(item4.distinctIndices[i]))
						{
							dictionary2.Remove(item4.distinctIndices[i]);
						}
						pb_Tuple<Vector3, Vector3, List<int>> value4 = null;
						if (dictionary6.TryGetValue(key2, out value4))
						{
							value4.Item1.x += vector.x;
							value4.Item1.y += vector.y;
							value4.Item1.z += vector.z;
							value4.Item3.Add(num3);
						}
						else
						{
							dictionary6.Add(key2, new pb_Tuple<Vector3, Vector3, List<int>>(vector, vector, new List<int> { num3 }));
						}
					}
				}
			}
			foreach (KeyValuePair<int, pb_Tuple<Vector3, Vector3, List<int>>> item5 in dictionary6)
			{
				Vector3 vector2 = item5.Value.Item1 / item5.Value.Item3.Count;
				vector2.Normalize();
				float num4 = ((!compensateAngleVertexDistance) ? 1f : pb_Math.Secant(Vector3.Angle(vector2, item5.Value.Item2) * ((float)Math.PI / 180f)));
				vector2.x *= distance * num4;
				vector2.y *= distance * num4;
				vector2.z *= distance * num4;
				foreach (int item6 in item5.Value.Item3)
				{
					list[item6].position.x += vector2.x;
					list[item6].position.y += vector2.y;
					list[item6].position.z += vector2.z;
				}
			}
			foreach (KeyValuePair<int, int> item7 in dictionary5)
			{
				list[item7.Key] = new pb_Vertex(list[item7.Value]);
			}
			pb.SetVertices(list);
			pb.SetFaces(list2.ToArray());
			pb.SetSharedIndices(dictionary);
			pb.SetSharedIndicesUV(dictionary2);
			return true;
		}

		private static List<HashSet<pb_Face>> GetFaceGroups(List<pb_WingedEdge> wings)
		{
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
			List<HashSet<pb_Face>> list = new List<HashSet<pb_Face>>();
			foreach (pb_WingedEdge wing in wings)
			{
				if (!hashSet.Add(wing.face))
				{
					continue;
				}
				HashSet<pb_Face> hashSet2 = new HashSet<pb_Face>();
				hashSet2.Add(wing.face);
				HashSet<pb_Face> hashSet3 = hashSet2;
				pb_GrowShrink.Flood(wing, hashSet3);
				foreach (pb_Face item in hashSet3)
				{
					hashSet.Add(item);
				}
				list.Add(hashSet3);
			}
			return list;
		}

		private static Dictionary<pb_EdgeLookup, pb_Face> GetPerimeterEdges(HashSet<pb_Face> faces, Dictionary<int, int> lookup)
		{
			Dictionary<pb_EdgeLookup, pb_Face> dictionary = new Dictionary<pb_EdgeLookup, pb_Face>();
			HashSet<pb_EdgeLookup> hashSet = new HashSet<pb_EdgeLookup>();
			foreach (pb_Face face in faces)
			{
				pb_Edge[] edges = face.edges;
				for (int i = 0; i < edges.Length; i++)
				{
					pb_Edge pb_Edge2 = edges[i];
					pb_EdgeLookup pb_EdgeLookup2 = new pb_EdgeLookup(lookup[pb_Edge2.x], lookup[pb_Edge2.y], pb_Edge2.x, pb_Edge2.y);
					if (!hashSet.Add(pb_EdgeLookup2))
					{
						if (dictionary.ContainsKey(pb_EdgeLookup2))
						{
							dictionary.Remove(pb_EdgeLookup2);
						}
					}
					else
					{
						dictionary.Add(pb_EdgeLookup2, face);
					}
				}
			}
			return dictionary;
		}

		public static bool Extrude(this pb_Object pb, pb_Edge[] edges, float extrudeDistance, bool extrudeAsGroup, bool enableManifoldExtrude, out pb_Edge[] extrudedEdges)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> dictionary = sharedIndices.ToDictionary();
			List<pb_Edge> list = new List<pb_Edge>();
			List<pb_Face> list2 = new List<pb_Face>();
			foreach (pb_Edge pb_Edge2 in edges)
			{
				int num = 0;
				pb_Face item = null;
				pb_Face[] faces = pb.faces;
				foreach (pb_Face pb_Face2 in faces)
				{
					if (pb_Face2.edges.IndexOf(pb_Edge2, dictionary) > -1)
					{
						item = pb_Face2;
						if (++num > 1)
						{
							break;
						}
					}
				}
				if (enableManifoldExtrude || num < 2)
				{
					list.Add(pb_Edge2);
					list2.Add(item);
				}
			}
			if (list.Count < 1)
			{
				extrudedEdges = null;
				return false;
			}
			Vector3[] vertices = pb.vertices;
			Vector3[] normals = pb.msh.normals;
			int[] array = new int[list.Count * 2];
			int num2 = 0;
			for (int k = 0; k < list.Count; k++)
			{
				array[num2++] = list[k].x;
				array[num2++] = list[k].y;
			}
			List<pb_Edge> list3 = new List<pb_Edge>();
			List<pb_Edge> list4 = new List<pb_Edge>();
			for (int l = 0; l < list.Count; l++)
			{
				pb_Edge pb_Edge3 = list[l];
				pb_Face pb_Face3 = list2[l];
				Vector3 vector = ((!extrudeAsGroup) ? pb_Math.Normal(pb, pb_Face3) : pb_MeshOps.AverageNormalWithIndices(sharedIndices[dictionary[pb_Edge3.x]], array, normals));
				Vector3 vector2 = ((!extrudeAsGroup) ? pb_Math.Normal(pb, pb_Face3) : pb_MeshOps.AverageNormalWithIndices(sharedIndices[dictionary[pb_Edge3.y]], array, normals));
				int num3 = dictionary[pb_Edge3.x];
				int num4 = dictionary[pb_Edge3.y];
				pb_Face pb_Face4 = pb.AppendFace(new Vector3[4]
				{
					vertices[pb_Edge3.x],
					vertices[pb_Edge3.y],
					vertices[pb_Edge3.x] + vector.normalized * extrudeDistance,
					vertices[pb_Edge3.y] + vector2.normalized * extrudeDistance
				}, new Color[4]
				{
					pb.colors[pb_Edge3.x],
					pb.colors[pb_Edge3.y],
					pb.colors[pb_Edge3.x],
					pb.colors[pb_Edge3.y]
				}, new Vector2[4], new pb_Face(new int[6] { 2, 1, 0, 2, 3, 1 }, pb_Face3.material, new pb_UV(), 0, -1, -1, false), new int[4] { num3, num4, -1, -1 });
				list4.Add(new pb_Edge(pb_Face4.indices[3], pb_Face4.indices[4]));
				list3.Add(new pb_Edge(num3, pb_Face4.indices[3]));
				list3.Add(new pb_Edge(num4, pb_Face4.indices[4]));
			}
			sharedIndices = pb.sharedIndices;
			if (extrudeAsGroup)
			{
				for (int m = 0; m < list3.Count; m++)
				{
					int x = list3[m].x;
					for (int n = 0; n < list3.Count; n++)
					{
						if (n != m && list3[n].x == x)
						{
							pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, list3[n].y, list3[m].y);
							break;
						}
					}
				}
			}
			pb.SetSharedIndices(sharedIndices);
			pb_Face[] faces2 = pb.faces;
			foreach (pb_Face pb_Face5 in faces2)
			{
				pb_Face5.RebuildCaches();
			}
			extrudedEdges = list4.ToArray();
			return true;
		}
	}
}
