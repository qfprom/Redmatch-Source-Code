using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.Core
{
	internal static class pb_HandleUtility
	{
		private const float MAX_EDGE_SELECT_DISTANCE = 20f;

		public static Vector3 ScreenToGuiPoint(this Camera camera, Vector3 point, float pixelsPerPoint)
		{
			return new Vector3(point.x / pixelsPerPoint, ((float)camera.pixelHeight - point.y) / pixelsPerPoint, point.z);
		}

		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, HashSet<pb_Face> ignore = null)
		{
			return FaceRaycast(InWorldRay, mesh, out hit, float.PositiveInfinity, pb_Culling.Front, ignore);
		}

		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, float distance, pb_Culling cullingMode, HashSet<pb_Face> ignore = null)
		{
			InWorldRay.origin -= mesh.transform.position;
			InWorldRay.origin = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction = mesh.transform.worldToLocalMatrix * InWorldRay.direction;
			Vector3[] vertices = mesh.vertices;
			float OutDistance = 0f;
			Vector3 OutPoint = Vector3.zero;
			float num = float.PositiveInfinity;
			int num2 = -1;
			Vector3 inNormal = Vector3.zero;
			for (int i = 0; i < mesh.faces.Length; i++)
			{
				if (ignore != null && ignore.Contains(mesh.faces[i]))
				{
					continue;
				}
				int[] indices = mesh.faces[i].indices;
				for (int j = 0; j < indices.Length; j += 3)
				{
					Vector3 vector = vertices[indices[j]];
					Vector3 vector2 = vertices[indices[j + 1]];
					Vector3 vector3 = vertices[indices[j + 2]];
					Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
					float num3 = Vector3.Dot(InWorldRay.direction, vector4);
					bool flag = false;
					switch (cullingMode)
					{
					case pb_Culling.Front:
						if (num3 > 0f)
						{
							flag = true;
						}
						break;
					case pb_Culling.Back:
						if (num3 < 0f)
						{
							flag = true;
						}
						break;
					}
					if (!flag && pb_Math.RayIntersectsTriangle(InWorldRay, vector, vector2, vector3, out OutDistance, out OutPoint) && !(OutDistance > num) && !(OutDistance > distance))
					{
						inNormal = vector4;
						num2 = i;
						num = OutDistance;
					}
				}
			}
			hit = new pb_RaycastHit(num, InWorldRay.GetPoint(num), inNormal, num2);
			return num2 > -1;
		}

		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out List<pb_RaycastHit> hits, float distance, pb_Culling cullingMode, HashSet<pb_Face> ignore = null)
		{
			InWorldRay.origin -= mesh.transform.position;
			InWorldRay.origin = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction = mesh.transform.worldToLocalMatrix * InWorldRay.direction;
			Vector3[] vertices = mesh.vertices;
			float OutDistance = 0f;
			Vector3 OutPoint = Vector3.zero;
			hits = new List<pb_RaycastHit>();
			for (int i = 0; i < mesh.faces.Length; i++)
			{
				if (ignore != null && ignore.Contains(mesh.faces[i]))
				{
					continue;
				}
				int[] indices = mesh.faces[i].indices;
				for (int j = 0; j < indices.Length; j += 3)
				{
					Vector3 vector = vertices[indices[j]];
					Vector3 vector2 = vertices[indices[j + 1]];
					Vector3 vector3 = vertices[indices[j + 2]];
					if (!pb_Math.RayIntersectsTriangle(InWorldRay, vector, vector2, vector3, out OutDistance, out OutPoint))
					{
						continue;
					}
					Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
					if (cullingMode != pb_Culling.Front)
					{
						if (cullingMode != pb_Culling.Back)
						{
							if (cullingMode != pb_Culling.FrontBack)
							{
								continue;
							}
						}
						else
						{
							float num = Vector3.Dot(InWorldRay.direction, vector4);
							if (!(num > 0f))
							{
								continue;
							}
						}
					}
					else
					{
						float num = Vector3.Dot(InWorldRay.direction, -vector4);
						if (!(num > 0f))
						{
							continue;
						}
					}
					hits.Add(new pb_RaycastHit(OutDistance, InWorldRay.GetPoint(OutDistance), vector4, i));
				}
			}
			return hits.Count > 0;
		}

		public static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
		{
			Vector3 origin = InWorldRay.origin;
			origin -= transform.position;
			origin = transform.worldToLocalMatrix * origin;
			Vector3 direction = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
			return new Ray(origin, direction);
		}

		public static bool WorldRaycast(Ray InWorldRay, Transform transform, Vector3[] vertices, int[] triangles, out pb_RaycastHit hit, float distance = float.PositiveInfinity, pb_Culling cullingMode = pb_Culling.Front)
		{
			Ray inRay = transform.InverseTransformRay(InWorldRay);
			return MeshRaycast(inRay, vertices, triangles, out hit, distance, cullingMode);
		}

		public static bool MeshRaycast(Ray InRay, Vector3[] vertices, int[] triangles, out pb_RaycastHit hit, float distance = float.PositiveInfinity, pb_Culling cullingMode = pb_Culling.Front)
		{
			float num = float.PositiveInfinity;
			Vector3 normal = new Vector3(0f, 0f, 0f);
			int num2 = -1;
			Vector3 origin = InRay.origin;
			Vector3 direction = InRay.direction;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				Vector3 vert = vertices[triangles[i]];
				Vector3 vert2 = vertices[triangles[i + 1]];
				Vector3 vert3 = vertices[triangles[i + 2]];
				if (pb_Math.RayIntersectsTriangle2(origin, direction, vert, vert2, vert3, ref distance, ref normal))
				{
					num2 = i / 3;
					num = distance;
					break;
				}
			}
			hit = new pb_RaycastHit(num, InRay.GetPoint(num), normal, num2);
			return num2 > -1;
		}

		internal static bool PointIsOccluded(Camera cam, pb_Object pb, Vector3 worldPoint)
		{
			Vector3 normalized = (cam.transform.position - worldPoint).normalized;
			Ray inWorldRay = new Ray(worldPoint + normalized * 0.0001f, normalized);
			pb_RaycastHit hit;
			return FaceRaycast(inWorldRay, pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), pb_Culling.Back, (HashSet<pb_Face>)null);
		}

		internal static bool IsOccluded(Camera cam, pb_Object pb, pb_Face face)
		{
			Vector3 zero = Vector3.zero;
			int num = face.distinctIndices.Length;
			for (int i = 0; i < num; i++)
			{
				zero += pb.vertices[face.distinctIndices[i]];
			}
			zero *= 1f / (float)num;
			return PointIsOccluded(cam, pb, pb.transform.TransformPoint(zero));
		}
	}
}
