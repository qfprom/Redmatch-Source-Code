using UnityEngine;

namespace ProBuilder.Core
{
	internal abstract class pb_EntityBehaviour : MonoBehaviour
	{
		[Tooltip("Allow ProBuilder to automatically hide and show this object when entering or exiting play mode.")]
		public bool manageVisibility = true;

		public abstract void Initialize();

		public abstract void OnEnterPlayMode();

		protected void SetMaterial(Material material)
		{
			pb_Object component = GetComponent<pb_Object>();
			if (component != null)
			{
				component.SetFaceMaterial(component.faces, material);
				component.ToMesh();
				component.Refresh();
			}
			else if ((bool)GetComponent<Renderer>())
			{
				GetComponent<Renderer>().sharedMaterial = material;
			}
			else
			{
				base.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
			}
		}
	}
}
