using System;

namespace Photon.Voice
{
	public interface IDecoderQueued : IDecoder, IDisposable
	{
		void Decode(byte[] buf);
	}
}
