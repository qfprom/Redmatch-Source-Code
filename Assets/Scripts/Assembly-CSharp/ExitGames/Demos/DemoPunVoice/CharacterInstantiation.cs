using System;
using System.Diagnostics;
using System.Threading;
using Photon.Pun;
using UnityEngine;

namespace ExitGames.Demos.DemoPunVoice
{
	public class CharacterInstantiation : MonoBehaviourPunCallbacks
	{
		public delegate void OnCharacterInstantiated(GameObject character);

		public Transform SpawnPosition;

		public float PositionOffset = 2f;

		public GameObject[] PrefabsToInstantiate;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static OnCharacterInstantiated CharacterInstantiated__BackingField;

		public static event OnCharacterInstantiated CharacterInstantiated
		{
			add
			{
				OnCharacterInstantiated onCharacterInstantiated = CharacterInstantiated__BackingField;
				OnCharacterInstantiated onCharacterInstantiated2;
				do
				{
					onCharacterInstantiated2 = onCharacterInstantiated;
					onCharacterInstantiated = Interlocked.CompareExchange(ref CharacterInstantiated__BackingField, (OnCharacterInstantiated)Delegate.Combine(onCharacterInstantiated2, value), onCharacterInstantiated);
				}
				while ((object)onCharacterInstantiated != onCharacterInstantiated2);
			}
			remove
			{
				OnCharacterInstantiated onCharacterInstantiated = CharacterInstantiated__BackingField;
				OnCharacterInstantiated onCharacterInstantiated2;
				do
				{
					onCharacterInstantiated2 = onCharacterInstantiated;
					onCharacterInstantiated = Interlocked.CompareExchange(ref CharacterInstantiated__BackingField, (OnCharacterInstantiated)Delegate.Remove(onCharacterInstantiated2, value), onCharacterInstantiated);
				}
				while ((object)onCharacterInstantiated != onCharacterInstantiated2);
			}
		}

		public override void OnJoinedRoom()
		{
			if (PrefabsToInstantiate != null)
			{
				GameObject gameObject = PrefabsToInstantiate[(PhotonNetwork.LocalPlayer.ActorNumber - 1) % 4];
				Vector3 vector = Vector3.zero;
				if (SpawnPosition != null)
				{
					vector = SpawnPosition.position;
				}
				Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
				insideUnitSphere = PositionOffset * insideUnitSphere.normalized;
				vector += insideUnitSphere;
				vector.y = 0f;
				Camera.main.transform.position += vector;
				gameObject = PhotonNetwork.Instantiate(gameObject.name, vector, Quaternion.identity, 0);
				if (CharacterInstantiated__BackingField != null)
				{
					CharacterInstantiated__BackingField(gameObject);
				}
			}
		}
	}
}
