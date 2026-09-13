using UnityEngine;

namespace ProBuilder.Core
{
	[DisallowMultipleComponent]
	internal class pb_ColliderBehaviour : pb_EntityBehaviour
	{
		public override void Initialize()
		{
			Collider collider = base.gameObject.GetComponent<Collider>();
			if (!collider)
			{
				collider = base.gameObject.AddComponent<MeshCollider>();
			}
			collider.isTrigger = false;
			SetMaterial(pb_Material.ColliderMaterial);
		}

		public override void OnEnterPlayMode()
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}
}
