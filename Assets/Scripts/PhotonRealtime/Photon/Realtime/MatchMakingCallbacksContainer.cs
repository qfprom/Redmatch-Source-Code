using System.Collections.Generic;

namespace Photon.Realtime
{
	public class MatchMakingCallbacksContainer : List<IMatchmakingCallbacks>, IMatchmakingCallbacks
	{
		private HashSet<IMatchmakingCallbacks> targetsToAdd;

		private HashSet<IMatchmakingCallbacks> targetsToRemove;

		public MatchMakingCallbacksContainer()
		{
			targetsToAdd = new HashSet<IMatchmakingCallbacks>();
			targetsToRemove = new HashSet<IMatchmakingCallbacks>();
		}

		public void AddCallbackTarget(IMatchmakingCallbacks target)
		{
			targetsToAdd.Add(target);
			targetsToRemove.Remove(target);
		}

		public void RemoveCallbackTarget(IMatchmakingCallbacks target)
		{
			targetsToRemove.Add(target);
			targetsToAdd.Remove(target);
		}

		private void UpdateCallbackTargets()
		{
			if (targetsToAdd.Count != 0)
			{
				foreach (IMatchmakingCallbacks item in targetsToAdd)
				{
					Add(item);
				}
			}
			targetsToAdd.Clear();
			if (targetsToRemove.Count != 0)
			{
				foreach (IMatchmakingCallbacks item2 in targetsToRemove)
				{
					Remove(item2);
				}
			}
			targetsToRemove.Clear();
		}

		public void OnCreatedRoom()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnCreatedRoom();
				}
			}
		}

		public void OnJoinedRoom()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnJoinedRoom();
				}
			}
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnCreateRoomFailed(returnCode, message);
				}
			}
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnJoinRandomFailed(returnCode, message);
				}
			}
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnJoinRoomFailed(returnCode, message);
				}
			}
		}

		public void OnLeftRoom()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnLeftRoom();
				}
			}
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IMatchmakingCallbacks current = enumerator.Current;
					current.OnFriendListUpdate(friendList);
				}
			}
		}
	}
}
