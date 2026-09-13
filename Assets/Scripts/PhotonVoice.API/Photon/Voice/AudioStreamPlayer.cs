namespace Photon.Voice
{
	public class AudioStreamPlayer : IAudioOut
	{
		private const int maxPlayLagMs = 100;

		private int maxPlayLagSamples;

		private int playDelaySamples;

		private int frameSize;

		private int frameSamples;

		private int streamSamplePos;

		private ILogger logger;

		private int CurrentBufferLag;

		private int streamSamplePosAvg;

		private ISyncAudioOut audioOut;

		private bool audioOutStarted;

		private string logPrefix;

		private bool debugInfo;

		public int Lag
		{
			get
			{
				return CurrentBufferLag;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return audioOut.IsPlaying;
			}
		}

		public AudioStreamPlayer(ILogger logger, ISyncAudioOut audioOut, string logPrefix, bool debugInfo)
		{
			this.logger = logger;
			this.audioOut = audioOut;
			this.logPrefix = logPrefix;
			this.debugInfo = debugInfo;
		}

		public void Start(int frequency, int channels, int frameSamples, int playDelayMs)
		{
			this.frameSamples = frameSamples;
			frameSize = frameSamples * channels;
			maxPlayLagSamples = 100 * frequency / 1000 + this.frameSamples;
			playDelaySamples = playDelayMs * frequency / 1000 + this.frameSamples;
			CurrentBufferLag = playDelaySamples;
			streamSamplePosAvg = playDelaySamples;
			streamSamplePos = 0;
			audioOut.Start(frequency, channels, frameSamples, 100 + playDelayMs);
			audioOutStarted = true;
		}

		public void Service()
		{
			if (!audioOutStarted)
			{
				return;
			}
			audioOut.Service();
			int playSamplePos = audioOut.PlaySamplePos;
			CurrentBufferLag = (CurrentBufferLag * 39 + (streamSamplePos - playSamplePos)) / 40;
			streamSamplePosAvg = playSamplePos + CurrentBufferLag;
			if (streamSamplePosAvg > streamSamplePos)
			{
				streamSamplePosAvg = streamSamplePos;
			}
			if (playSamplePos < streamSamplePos - playDelaySamples && !audioOut.IsPlaying)
			{
				audioOut.UnPause();
			}
			if (playSamplePos > streamSamplePos - frameSamples && audioOut.IsPlaying)
			{
				if (debugInfo)
				{
					logger.LogWarning("{0} player overrun: {1}/{2}({3}) = {4}", logPrefix, playSamplePos, streamSamplePos, streamSamplePosAvg, streamSamplePos - playSamplePos);
				}
				audioOut.Pause();
				playSamplePos = streamSamplePos;
				audioOut.PlaySamplePos = playSamplePos;
				CurrentBufferLag = playDelaySamples;
			}
			if (!audioOut.IsPlaying)
			{
				return;
			}
			int num = streamSamplePos - playDelaySamples - maxPlayLagSamples;
			if (playSamplePos < num)
			{
				if (debugInfo)
				{
					logger.LogWarning("{0} player underrun: {1}/{2}({3}) = {4}", logPrefix, playSamplePos, streamSamplePos, streamSamplePosAvg, streamSamplePos - playSamplePos);
				}
				playSamplePos = streamSamplePos - playDelaySamples;
				audioOut.PlaySamplePos = playSamplePos;
				CurrentBufferLag = playDelaySamples;
			}
		}

		public void Push(float[] frame)
		{
			if (frame.Length != 0)
			{
				if (frame.Length != frameSize)
				{
					logger.LogError("{0} Audio frames are not of  size: {1} != {2}", logPrefix, frame.Length, frameSize);
				}
				else if (audioOutStarted)
				{
					audioOut.Push(frame);
					streamSamplePos += frameSamples;
				}
			}
		}

		public void Stop()
		{
			audioOut.Stop();
			audioOutStarted = false;
		}
	}
}
