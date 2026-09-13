using UnityEngine;

namespace ProBuilder.Core
{
	[DisallowMultipleComponent]
	internal class pb_TriggerBehaviour : pb_EntityBehaviour
	{
		public override void Initialize()
		{
			Collider collider = base.gameObject.GetComponent<Collider>();
			if (!collider)
			{
				collider = base.gameObject.AddComponent<MeshCollider>();
			}
			MeshCollider meshCollider = collider as MeshCollider;
			if ((bool)meshCollider)
			{
				meshCollider.convex = true;
			}
			collider.isTrigger = true;
			SetMaterial(pb_Material.TriggerMaterial);
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
