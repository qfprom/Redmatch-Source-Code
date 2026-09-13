namespace Photon.Voice
{
	public class LocalVoiceAudioShort : LocalVoiceAudio<short>
	{
		internal LocalVoiceAudioShort(VoiceClient voiceClient, IEncoderDataFlow<short> encoder, byte id, VoiceInfo voiceInfo, int channelId)
			: base(voiceClient, encoder, id, voiceInfo, channelId)
		{
			levelMeter = new AudioUtil.LevelMeterShort(info.SamplingRate, info.Channels);
			voiceDetector = new AudioUtil.VoiceDetectorShort(info.SamplingRate, info.Channels);
			initBuiltinProcessors();
		}
	}
}
