using System.Collections.Generic;

namespace Photon.Realtime
{
	internal class LobbyCallbacksContainer : List<ILobbyCallbacks>, ILobbyCallbacks
	{
		private HashSet<ILobbyCallbacks> targetsToAdd;

		private HashSet<ILobbyCallbacks> targetsToRemove;

		public LobbyCallbacksContainer()
		{
			targetsToAdd = new HashSet<ILobbyCallbacks>();
			targetsToRemove = new HashSet<ILobbyCallbacks>();
		}

		public void AddCallbackTarget(ILobbyCallbacks target)
		{
			targetsToAdd.Add(target);
			targetsToRemove.Remove(target);
		}

		public void RemoveCallbackTarget(ILobbyCallbacks target)
		{
			targetsToRemove.Add(target);
			targetsToAdd.Remove(target);
		}

		private void UpdateCallbackTargets()
		{
			if (targetsToAdd.Count != 0)
			{
				foreach (ILobbyCallbacks item in targetsToAdd)
				{
					Add(item);
				}
			}
			targetsToAdd.Clear();
			if (targetsToRemove.Count != 0)
			{
				foreach (ILobbyCallbacks item2 in targetsToRemove)
				{
					Remove(item2);
				}
			}
			targetsToRemove.Clear();
		}

		public void OnJoinedLobby()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ILobbyCallbacks current = enumerator.Current;
					current.OnJoinedLobby();
				}
			}
		}

		public void OnLeftLobby()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ILobbyCallbacks current = enumerator.Current;
					current.OnLeftLobby();
				}
			}
		}

		public void OnRoomListUpdate(List<RoomInfo> roomList)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ILobbyCallbacks current = enumerator.Current;
					current.OnRoomListUpdate(roomList);
				}
			}
		}

		public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ILobbyCallbacks current = enumerator.Current;
					current.OnLobbyStatisticsUpdate(lobbyStatistics);
				}
			}
		}
	}
}
