using System;

namespace Photon.Realtime
{
	public enum ClientState
	{
		PeerCreated = 0,
		Authenticating = 1,
		Authenticated = 2,
		JoiningLobby = 3,
		JoinedLobby = 4,
		DisconnectingFromMasterserver = 5,
		ConnectingToGameserver = 6,
		ConnectedToGameserver = 7,
		Joining = 8,
		Joined = 9,
		Leaving = 10,
		DisconnectingFromGameserver = 11,
		ConnectingToMasterserver = 12,
		Disconnecting = 13,
		Disconnected = 14,
		ConnectedToMasterserver = 15,
		[Obsolete("Renamed to ConnectedToMasterserver.")]
		ConnectedToMaster = ConnectedToMasterserver,
		ConnectingToNameServer = 16,
		ConnectedToNameServer = 17,
		DisconnectingFromNameServer = 18
	}
}
