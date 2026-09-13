using System;

namespace Photon.Voice
{
	public interface IDecoderQueuedOutputImageNative : IDecoderQueued, IDecoder, IDisposable
	{
		ImageFormat OutputImageFormat { get; set; }

		Flip OutputImageFlip { get; set; }

		Func<int, int, IntPtr> OutputImageBufferGetter { get; set; }

		OnImageOutputNative OnOutputImage { get; set; }
	}
}
