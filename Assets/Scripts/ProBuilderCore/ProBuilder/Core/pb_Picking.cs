using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.Core
{
	internal static class pb_Picking
	{
		public static Dictionary<pb_Object, HashSet<int>> PickVerticesInRect(Camera cam, Rect rect, IList<pb_Object> selectable, pb_PickerOptions options, float pixelsPerPoint = 1f)
		{
			if (options.depthTest)
			{
				return pb_SelectionPicker.PickVerticesInRect(cam, rect, selectable, true, (int)((float)cam.pixelWidth / pixelsPerPoint), (int)((float)cam.pixelHeight / pixelsPerPoint));
			}
			Dictionary<pb_Object, HashSet<int>> dictionary = new Dictionary<pb_Object, HashSet<int>>();
			foreach (pb_Object item in selectable)
			{
				if (!item.isSelectable)
				{
					continue;
				}
				pb_IntArray[] sharedIndices = item.sharedIndices;
				HashSet<int> hashSet = new HashSet<int>();
				Vector3[] vertices = item.vertices;
				Transform transform = item.transform;
				float num = cam.pixelHeight;
				for (int i = 0; i < sharedIndices.Length; i++)
				{
					Vector3 position = transform.TransformPoint(vertices[sharedIndices[i][0]]);
					Vector3 point = cam.WorldToScreenPoint(position);
					if (!(point.z < cam.nearClipPlane))
					{
						point.x /= pixelsPerPoint;
						point.y = (num - point.y) / pixelsPerPoint;
						if (rect.Contains(point))
						{
							hashSet.Add(i);
						}
					}
				}
				dictionary.Add(item, hashSet);
			}
			return dictionary;
		}

		public static Dictionary<pb_Object, HashSet<pb_Face>> PickFacesInRect(Camera cam, Rect rect, IList<pb_Object> selectable, pb_PickerOptions options, float pixelsPerPoint = 1f)
		{
			if (options.depthTest && options.rectSelectMode == pb_RectSelectMode.Partial)
			{
				return pb_SelectionPicker.PickFacesInRect(cam, rect, selectable, (int)((float)cam.pixelWidth / pixelsPerPoint), (int)((float)cam.pixelHeight / pixelsPerPoint));
			}
			Dictionary<pb_Object, HashSet<pb_Face>> dictionary = new Dictionary<pb_Object, HashSet<pb_Face>>();
			foreach (pb_Object item in selectable)
			{
				if (!item.isSelectable)
				{
					continue;
				}
				HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
				Transform transform = item.transform;
				Vector3[] vertices = item.vertices;
				Vector3[] array = new Vector3[item.vertexCount];
				for (int i = 0; i < item.vertexCount; i++)
				{
					array[i] = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(transform.TransformPoint(vertices[i])), pixelsPerPoint);
				}
				for (int j = 0; j < item.faces.Length; j++)
				{
					pb_Face pb_Face2 = item.faces[j];
					if (options.rectSelectMode == pb_RectSelectMode.Complete)
					{
						if (array[pb_Face2.indices[0]].z < cam.nearClipPlane || !rect.Contains(array[pb_Face2.indices[0]]))
						{
							continue;
						}
						bool flag = false;
						for (int k = 1; k < pb_Face2.distinctIndices.Length; k++)
						{
							int num = pb_Face2.distinctIndices[k];
							if (array[num].z < cam.nearClipPlane || !rect.Contains(array[num]))
							{
								flag = true;
								break;
							}
						}
						if (!flag && (!options.depthTest || !pb_HandleUtility.PointIsOccluded(cam, item, transform.TransformPoint(pb_Math.Average(vertices, pb_Face2.distinctIndices)))))
						{
							hashSet.Add(pb_Face2);
						}
						continue;
					}
					pb_Bounds2D pb_Bounds2D2 = new pb_Bounds2D(array, pb_Face2.edges);
					bool flag2 = false;
					if (pb_Bounds2D2.Intersects(rect))
					{
						for (int l = 0; l < pb_Face2.distinctIndices.Length; l++)
						{
							if (flag2)
							{
								break;
							}
							Vector3 point = array[pb_Face2.distinctIndices[l]];
							flag2 = point.z > cam.nearClipPlane && rect.Contains(point);
						}
						if (!flag2)
						{
							Vector2 vector = new Vector2(rect.xMin, rect.yMax);
							Vector2 vector2 = new Vector2(rect.xMax, rect.yMax);
							Vector2 vector3 = new Vector2(rect.xMin, rect.yMin);
							Vector2 vector4 = new Vector2(rect.xMax, rect.yMin);
							flag2 = pb_Math.PointInPolygon(array, pb_Bounds2D2, pb_Face2.edges, vector);
							if (!flag2)
							{
								flag2 = pb_Math.PointInPolygon(array, pb_Bounds2D2, pb_Face2.edges, vector2);
							}
							if (!flag2)
							{
								flag2 = pb_Math.PointInPolygon(array, pb_Bounds2D2, pb_Face2.edges, vector4);
							}
							if (!flag2)
							{
								flag2 = pb_Math.PointInPolygon(array, pb_Bounds2D2, pb_Face2.edges, vector3);
							}
							for (int m = 0; m < pb_Face2.edges.Length; m++)
							{
								if (flag2)
								{
									break;
								}
								if (pb_Math.GetLineSegmentIntersect(vector2, vector, array[pb_Face2.edges[m].x], array[pb_Face2.edges[m].y]))
								{
									flag2 = true;
								}
								else if (pb_Math.GetLineSegmentIntersect(vector, vector3, array[pb_Face2.edges[m].x], array[pb_Face2.edges[m].y]))
								{
									flag2 = true;
								}
								else if (pb_Math.GetLineSegmentIntersect(vector3, vector4, array[pb_Face2.edges[m].x], array[pb_Face2.edges[m].y]))
								{
									flag2 = true;
								}
								else if (pb_Math.GetLineSegmentIntersect(vector4, vector, array[pb_Face2.edges[m].x], array[pb_Face2.edges[m].y]))
								{
									flag2 = true;
								}
							}
						}
					}
					if (flag2)
					{
						hashSet.Add(pb_Face2);
					}
				}
				dictionary.Add(item, hashSet);
			}
			return dictionary;
		}

		public static Dictionary<pb_Object, HashSet<pb_Edge>> PickEdgesInRect(Camera cam, Rect rect, IList<pb_Object> selectable, pb_PickerOptions options, float pixelsPerPoint = 1f)
		{
			if (options.depthTest && options.rectSelectMode == pb_RectSelectMode.Partial)
			{
				return pb_SelectionPicker.PickEdgesInRect(cam, rect, selectable, true, (int)((float)cam.pixelWidth / pixelsPerPoint), (int)((float)cam.pixelHeight / pixelsPerPoint));
			}
			Dictionary<pb_Object, HashSet<pb_Edge>> dictionary = new Dictionary<pb_Object, HashSet<pb_Edge>>();
			foreach (pb_Object item2 in selectable)
			{
				if (!item2.isSelectable)
				{
					continue;
				}
				Transform transform = item2.transform;
				HashSet<pb_Edge> hashSet = new HashSet<pb_Edge>();
				int i = 0;
				for (int faceCount = item2.faceCount; i < faceCount; i++)
				{
					pb_Edge[] edges = item2.faces[i].edges;
					int j = 0;
					for (int num = edges.Length; j < num; j++)
					{
						pb_Edge item = edges[j];
						Vector3 vector = transform.TransformPoint(item2.vertices[item.x]);
						Vector3 vector2 = transform.TransformPoint(item2.vertices[item.y]);
						Vector3 vector3 = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(vector), pixelsPerPoint);
						Vector3 vector4 = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(vector2), pixelsPerPoint);
						switch (options.rectSelectMode)
						{
						case pb_RectSelectMode.Complete:
							if (!(vector3.z < cam.nearClipPlane) && !(vector4.z < cam.nearClipPlane) && rect.Contains(vector3) && rect.Contains(vector4) && (!options.depthTest || !pb_HandleUtility.PointIsOccluded(cam, item2, (vector + vector2) * 0.5f)))
							{
								hashSet.Add(item);
							}
							break;
						case pb_RectSelectMode.Partial:
							if (pb_Math.RectIntersectsLineSegment(rect, vector3, vector4))
							{
								hashSet.Add(item);
							}
							break;
						}
					}
				}
				dictionary.Add(item2, hashSet);
			}
			return dictionary;
		}
	}
}
