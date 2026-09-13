namespace Photon.Voice
{
	internal static class VoiceCodec
	{
		internal static IDecoder CreateDefaultDecoder(int channelId, int playerId, byte voiceId, VoiceInfo info, ILogger logger)
		{
			Codec codec = info.Codec;
			if (codec == Codec.AudioOpus)
			{
				return new OpusCodec.Decoder(logger);
			}
			return null;
		}
	}
}
