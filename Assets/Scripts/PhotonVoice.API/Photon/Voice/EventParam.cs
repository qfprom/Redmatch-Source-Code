namespace Photon.Voice
{
	internal enum EventParam : byte
	{
		VoiceId = 1,
		SamplingRate = 2,
		Channels = 3,
		FrameDurationUs = 4,
		Bitrate = 5,
		UserData = 10,
		EventNumber = 11,
		Codec = 12
	}
}
