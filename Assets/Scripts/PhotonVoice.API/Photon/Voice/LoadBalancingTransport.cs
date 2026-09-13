using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Voice
{
	public class LoadBalancingTransport : LoadBalancingClient, IVoiceTransport, IDisposable, ILogger
	{
		protected VoiceClient voiceClient;

		private object sendLock = new object();

		public VoiceClient VoiceClient
		{
			get
			{
				return voiceClient;
			}
		}

		[Obsolete("Use GlobalInterestGroup.")]
		public byte GlobalAudioGroup
		{
			get
			{
				return GlobalInterestGroup;
			}
			set
			{
				GlobalInterestGroup = value;
			}
		}

		public byte GlobalInterestGroup
		{
			get
			{
				return voiceClient.GlobalInterestGroup;
			}
			set
			{
				voiceClient.GlobalInterestGroup = value;
				if (base.State == ClientState.Joined)
				{
					if (voiceClient.GlobalInterestGroup != 0)
					{
						base.LoadBalancingPeer.OpChangeGroups(new byte[0], new byte[1] { voiceClient.GlobalInterestGroup });
					}
					else
					{
						base.LoadBalancingPeer.OpChangeGroups(new byte[0], null);
					}
				}
			}
		}

		public LoadBalancingTransport(ConnectionProtocol connectionProtocol = ConnectionProtocol.Udp)
			: base(connectionProtocol)
		{
			base.EventReceived += onEventActionVoiceClient;
			base.StateChanged += onStateChangeVoiceClient;
			voiceClient = new VoiceClient(this);
			int num = Enum.GetValues(typeof(Codec)).Length + 1;
			if (base.LoadBalancingPeer.ChannelCount < num)
			{
				base.LoadBalancingPeer.ChannelCount = (byte)num;
			}
		}

		public void LogError(string fmt, params object[] args)
		{
			DebugReturn(DebugLevel.ERROR, string.Format(fmt, args));
		}

		public void LogWarning(string fmt, params object[] args)
		{
			DebugReturn(DebugLevel.WARNING, string.Format(fmt, args));
		}

		public void LogInfo(string fmt, params object[] args)
		{
			DebugReturn(DebugLevel.INFO, string.Format(fmt, args));
		}

		public void LogDebug(string fmt, params object[] args)
		{
			DebugReturn(DebugLevel.ALL, string.Format(fmt, args));
		}

		public int AssignChannel(VoiceInfo v)
		{
			return 1 + Array.IndexOf(Enum.GetValues(typeof(Codec)), v.Codec);
		}

		public bool IsChannelJoined(int channelId)
		{
			return base.State == ClientState.Joined;
		}

		public void SetDebugEchoMode(LocalVoice v)
		{
			if (base.State == ClientState.Joined)
			{
				if (v.DebugEchoMode)
				{
					SendVoicesInfo(new List<LocalVoice> { v }, v.channelId, base.LocalPlayer.ActorNumber);
				}
				else
				{
					SendVoiceRemove(v, v.channelId, base.LocalPlayer.ActorNumber);
				}
			}
		}

		public new void Service()
		{
			base.Service();
			voiceClient.Service();
		}

		[Obsolete("Use LoadBalancingPeer::OpChangeGroups().")]
		public virtual bool ChangeAudioGroups(byte[] groupsToRemove, byte[] groupsToAdd)
		{
			return base.LoadBalancingPeer.OpChangeGroups(groupsToRemove, groupsToAdd);
		}

		public void SendVoicesInfo(IEnumerable<LocalVoice> voices, int channelId, int targetPlayerId)
		{
			object customEventContent = voiceClient.buildVoicesInfo(voices, true);
			SendOptions sendOptions = new SendOptions
			{
				Reliability = true,
				Channel = (byte)channelId
			};
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			if (targetPlayerId != 0)
			{
				raiseEventOptions.TargetActors = new int[1] { targetPlayerId };
			}
			lock (sendLock)
			{
				OpRaiseEvent(VoiceEventCode.GetCode(channelId), customEventContent, raiseEventOptions, sendOptions);
			}
			if (targetPlayerId == 0)
			{
				SendDebugEchoVoicesInfo(channelId);
			}
		}

		public void SendDebugEchoVoicesInfo(int channelId)
		{
			IEnumerable<LocalVoice> enumerable = voiceClient.LocalVoices.Where((LocalVoice x) => x.DebugEchoMode);
			if (enumerable.Count() > 0)
			{
				SendVoicesInfo(enumerable, channelId, base.LocalPlayer.ActorNumber);
			}
		}

		public void SendVoiceRemove(LocalVoice voice, int channelId, int targetPlayerId)
		{
			object customEventContent = voiceClient.buildVoiceRemoveMessage(voice);
			SendOptions sendOptions = new SendOptions
			{
				Reliability = true,
				Channel = (byte)channelId
			};
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			if (targetPlayerId != 0)
			{
				raiseEventOptions.TargetActors = new int[1] { targetPlayerId };
			}
			if (voice.DebugEchoMode)
			{
				raiseEventOptions.Receivers = ReceiverGroup.All;
			}
			lock (sendLock)
			{
				OpRaiseEvent(VoiceEventCode.GetCode(channelId), customEventContent, raiseEventOptions, sendOptions);
			}
		}

		public void SendFrame(ArraySegment<byte> data, byte evNumber, byte voiceId, int channelId, LocalVoice localVoice)
		{
			object[] customEventContent = new object[3] { voiceId, evNumber, data };
			SendOptions sendOptions = new SendOptions
			{
				Reliability = localVoice.Reliable,
				Channel = (byte)channelId,
				Encrypt = localVoice.Encrypt
			};
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			if (localVoice.DebugEchoMode)
			{
				raiseEventOptions.Receivers = ReceiverGroup.All;
			}
			raiseEventOptions.InterestGroup = localVoice.InterestGroup;
			lock (sendLock)
			{
				OpRaiseEvent(VoiceEventCode.GetCode(channelId), customEventContent, raiseEventOptions, sendOptions);
			}
			base.LoadBalancingPeer.SendOutgoingCommands();
		}

		public string ChannelIdStr(int channelId)
		{
			return null;
		}

		public string PlayerIdStr(int playerId)
		{
			return null;
		}

		private void onEventActionVoiceClient(EventData ev)
		{
			byte channelID;
			if (VoiceEventCode.TryGetChannelID(ev.Code, base.LoadBalancingPeer.ChannelCount, out channelID))
			{
				voiceClient.onVoiceEvent(ev[245], channelID, (int)ev[254], base.LocalPlayer.ActorNumber);
				return;
			}
			switch (ev.Code)
			{
			case byte.MaxValue:
			{
				int num = (int)ev[254];
				if (num != base.LocalPlayer.ActorNumber)
				{
					voiceClient.sendVoicesInfo(num);
				}
				break;
			}
			case 254:
			{
				int num = (int)ev[254];
				if (num == base.LocalPlayer.ActorNumber)
				{
					voiceClient.clearRemoteVoices();
				}
				else
				{
					onPlayerLeave(num);
				}
				break;
			}
			}
		}

		private void onStateChangeVoiceClient(ClientState fromState, ClientState state)
		{
			switch (state)
			{
			case ClientState.Joined:
				voiceClient.clearRemoteVoices();
				voiceClient.sendVoicesInfo(0);
				if (voiceClient.GlobalInterestGroup != 0)
				{
					base.LoadBalancingPeer.OpChangeGroups(new byte[0], new byte[1] { voiceClient.GlobalInterestGroup });
				}
				break;
			case ClientState.Disconnected:
				voiceClient.clearRemoteVoices();
				break;
			}
		}

		private void onPlayerLeave(int playerId)
		{
			if (voiceClient.removePlayerVoices(playerId))
			{
				DebugReturn(DebugLevel.INFO, "[PV] Player " + playerId + " voices removed on leave");
			}
			else
			{
				DebugReturn(DebugLevel.WARNING, "[PV] Voices of player " + playerId + " not found when trying to remove on player leave");
			}
		}

		public void Dispose()
		{
			voiceClient.Dispose();
		}
	}
}
