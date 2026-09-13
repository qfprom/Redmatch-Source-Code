using System;
using System.Collections.Generic;

namespace Photon.Voice
{
	internal interface IVoiceTransport : ILogger
	{
		int AssignChannel(VoiceInfo v);

		bool IsChannelJoined(int channelId);

		void SendVoicesInfo(IEnumerable<LocalVoice> voices, int channelId, int targetPlayerId);

		void SendVoiceRemove(LocalVoice voice, int channelId, int targetPlayerId);

		void SendFrame(ArraySegment<byte> data, byte evNumber, byte voiceId, int channelId, LocalVoice localVoice);

		string ChannelIdStr(int channelId);

		string PlayerIdStr(int playerId);

		void SetDebugEchoMode(LocalVoice v);
	}
}
