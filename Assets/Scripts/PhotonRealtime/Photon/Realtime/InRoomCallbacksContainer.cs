using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	internal class InRoomCallbacksContainer : List<IInRoomCallbacks>, IInRoomCallbacks
	{
		private HashSet<IInRoomCallbacks> targetsToAdd;

		private HashSet<IInRoomCallbacks> targetsToRemove;

		public InRoomCallbacksContainer()
		{
			targetsToAdd = new HashSet<IInRoomCallbacks>();
			targetsToRemove = new HashSet<IInRoomCallbacks>();
		}

		public void AddCallbackTarget(IInRoomCallbacks target)
		{
			targetsToAdd.Add(target);
			targetsToRemove.Remove(target);
		}

		public void RemoveCallbackTarget(IInRoomCallbacks target)
		{
			targetsToRemove.Add(target);
			targetsToAdd.Remove(target);
		}

		private void UpdateCallbackTargets()
		{
			if (targetsToAdd.Count != 0)
			{
				foreach (IInRoomCallbacks item in targetsToAdd)
				{
					Add(item);
				}
			}
			targetsToAdd.Clear();
			if (targetsToRemove.Count != 0)
			{
				foreach (IInRoomCallbacks item2 in targetsToRemove)
				{
					Remove(item2);
				}
			}
			targetsToRemove.Clear();
		}

		public void OnPlayerEnteredRoom(Player newPlayer)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IInRoomCallbacks current = enumerator.Current;
					current.OnPlayerEnteredRoom(newPlayer);
				}
			}
		}

		public void OnPlayerLeftRoom(Player otherPlayer)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IInRoomCallbacks current = enumerator.Current;
					current.OnPlayerLeftRoom(otherPlayer);
				}
			}
		}

		public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IInRoomCallbacks current = enumerator.Current;
					current.OnRoomPropertiesUpdate(propertiesThatChanged);
				}
			}
		}

		public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProp)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IInRoomCallbacks current = enumerator.Current;
					current.OnPlayerPropertiesUpdate(targetPlayer, changedProp);
				}
			}
		}

		public void OnMasterClientSwitched(Player newMasterClient)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IInRoomCallbacks current = enumerator.Current;
					current.OnMasterClientSwitched(newMasterClient);
				}
			}
		}
	}
}
