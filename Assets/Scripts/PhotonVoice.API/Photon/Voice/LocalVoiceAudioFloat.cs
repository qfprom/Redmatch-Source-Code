namespace Photon.Voice
{
	public class LocalVoiceAudioFloat : LocalVoiceAudio<float>
	{
		internal LocalVoiceAudioFloat(VoiceClient voiceClient, IEncoderDataFlow<float> encoder, byte id, VoiceInfo voiceInfo, int channelId)
			: base(voiceClient, encoder, id, voiceInfo, channelId)
		{
			levelMeter = new AudioUtil.LevelMeterFloat(info.SamplingRate, info.Channels);
			voiceDetector = new AudioUtil.VoiceDetectorFloat(info.SamplingRate, info.Channels);
			initBuiltinProcessors();
		}
	}
}
