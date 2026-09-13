using System;

namespace Photon.Voice
{
	public interface IEncoderDataFlowDirect<T> : IEncoderDataFlow<T>, IEncoder, IDisposable
	{
		ArraySegment<byte> EncodeAndGetOutput(T[] buf);
	}
}
