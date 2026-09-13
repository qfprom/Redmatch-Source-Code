using System;
using System.Collections.Generic;

namespace Photon.Voice
{
	public interface IEncoderNativeImageDirect : IEncoder, IDisposable
	{
		IEnumerable<ArraySegment<byte>> EncodeAndGetOutput(IntPtr[] buf, int width, int height, int[] stride, ImageFormat imageFormat, Rotation rotation, Flip flip);
	}
}
