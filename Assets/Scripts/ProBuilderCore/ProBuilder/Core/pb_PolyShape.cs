using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.Core
{
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[ProGridsConditionalSnap]
	public class pb_PolyShape : MonoBehaviour
	{
		public enum PolyEditMode
		{
			None = 0,
			Path = 1,
			Height = 2,
			Edit = 3
		}

		private pb_Object m_Mesh;

		public List<Vector3> points = new List<Vector3>();

		public float extrude = 0f;

		public PolyEditMode polyEditMode = PolyEditMode.None;

		public bool flipNormals = false;

		public bool isOnGrid = true;

		public Material material;

		public pb_Object mesh
		{
			get
			{
				if (m_Mesh == null)
				{
					m_Mesh = GetComponent<pb_Object>();
				}
				return m_Mesh;
			}
			set
			{
				m_Mesh = value;
			}
		}

		private bool IsSnapEnabled()
		{
			return isOnGrid;
		}
	}
}
