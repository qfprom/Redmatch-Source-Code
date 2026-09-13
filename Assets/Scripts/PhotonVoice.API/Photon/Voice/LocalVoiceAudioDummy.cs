namespace Photon.Voice
{
	public class LocalVoiceAudioDummy : LocalVoice, ILocalVoiceAudio
	{
		private AudioUtil.VoiceDetectorDummy voiceDetector;

		private AudioUtil.LevelMeterDummy levelMeter;

		public static LocalVoiceAudioDummy Dummy = new LocalVoiceAudioDummy();

		public AudioUtil.IVoiceDetector VoiceDetector
		{
			get
			{
				return voiceDetector;
			}
		}

		public AudioUtil.ILevelMeter LevelMeter
		{
			get
			{
				return levelMeter;
			}
		}

		public bool VoiceDetectorCalibrating
		{
			get
			{
				return false;
			}
		}

		public LocalVoiceAudioDummy()
		{
			voiceDetector = new AudioUtil.VoiceDetectorDummy();
			levelMeter = new AudioUtil.LevelMeterDummy();
		}

		public void VoiceDetectorCalibrate(int durationMs)
		{
		}
	}
}
