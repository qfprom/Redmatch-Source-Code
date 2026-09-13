using System;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	[Serializable]
	public class AppSettings
	{
		public string AppIdRealtime;

		public string AppIdChat;

		public string AppIdVoice;

		public string AppVersion;

		public bool UseNameServer = true;

		public string FixedRegion;

		public string Server;

		public int Port;

		public ConnectionProtocol Protocol;

		public bool EnableLobbyStatistics;

		public DebugLevel NetworkLogging = DebugLevel.ERROR;

		public bool IsMasterServerAddress
		{
			get
			{
				return !UseNameServer;
			}
		}

		public bool IsBestRegion
		{
			get
			{
				return UseNameServer && string.IsNullOrEmpty(FixedRegion);
			}
		}

		public bool IsDefaultNameServer
		{
			get
			{
				return UseNameServer && string.IsNullOrEmpty(Server);
			}
		}

		public bool IsDefaultPort
		{
			get
			{
				return Port <= 0;
			}
		}

		public string ToStringFull()
		{
			return string.Format("IsBestRegion: {0} IsDefaultNameServer: {1} IsDefaultPort: {2}", IsBestRegion, IsDefaultNameServer, IsDefaultPort);
		}
	}
}
