namespace Photon.Voice
{
	public interface ISyncAudioOut : IAudioOut
	{
		int PlaySamplePos { get; set; }

		void Pause();

		void UnPause();
	}
}
