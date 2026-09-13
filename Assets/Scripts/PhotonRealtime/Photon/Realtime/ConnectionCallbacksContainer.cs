using System.Collections.Generic;

namespace Photon.Realtime
{
	public class ConnectionCallbacksContainer : List<IConnectionCallbacks>, IConnectionCallbacks
	{
		private HashSet<IConnectionCallbacks> targetsToAdd;

		private HashSet<IConnectionCallbacks> targetsToRemove;

		public ConnectionCallbacksContainer()
		{
			targetsToAdd = new HashSet<IConnectionCallbacks>();
			targetsToRemove = new HashSet<IConnectionCallbacks>();
		}

		public void AddCallbackTarget(IConnectionCallbacks target)
		{
			targetsToAdd.Add(target);
			targetsToRemove.Remove(target);
		}

		public void RemoveCallbackTarget(IConnectionCallbacks target)
		{
			targetsToRemove.Add(target);
			targetsToAdd.Remove(target);
		}

		private void UpdateCallbackTargets()
		{
			if (targetsToAdd.Count != 0)
			{
				foreach (IConnectionCallbacks item in targetsToAdd)
				{
					Add(item);
				}
			}
			targetsToAdd.Clear();
			if (targetsToRemove.Count != 0)
			{
				foreach (IConnectionCallbacks item2 in targetsToRemove)
				{
					Remove(item2);
				}
			}
			targetsToRemove.Clear();
		}

		public void OnConnected()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IConnectionCallbacks current = enumerator.Current;
					current.OnConnected();
				}
			}
		}

		public void OnConnectedToMaster()
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IConnectionCallbacks current = enumerator.Current;
					current.OnConnectedToMaster();
				}
			}
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IConnectionCallbacks current = enumerator.Current;
					current.OnRegionListReceived(regionHandler);
				}
			}
		}

		public void OnDisconnected(DisconnectCause cause)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IConnectionCallbacks current = enumerator.Current;
					current.OnDisconnected(cause);
				}
			}
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IConnectionCallbacks current = enumerator.Current;
					current.OnCustomAuthenticationResponse(data);
				}
			}
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
			UpdateCallbackTargets();
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IConnectionCallbacks current = enumerator.Current;
					current.OnCustomAuthenticationFailed(debugMessage);
				}
			}
		}
	}
}
