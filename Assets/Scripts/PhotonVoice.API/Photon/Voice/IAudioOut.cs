namespace Photon.Voice
{
	public interface IAudioOut
	{
		bool IsPlaying { get; }

		int Lag { get; }

		void Start(int frequency, int channels, int frameSamplesPerChannel, int playDelayMs);

		void Stop();

		void Push(float[] frame);

		void Service();
	}
}
