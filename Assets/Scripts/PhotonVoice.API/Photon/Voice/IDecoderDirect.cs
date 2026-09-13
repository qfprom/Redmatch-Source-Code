using System;

namespace Photon.Voice
{
	public interface IDecoderDirect : IDecoder, IDisposable
	{
		byte[] DecodeToByte(byte[] buf);

		float[] DecodeToFloat(byte[] buf);

		short[] DecodeToShort(byte[] buf);
	}
}
