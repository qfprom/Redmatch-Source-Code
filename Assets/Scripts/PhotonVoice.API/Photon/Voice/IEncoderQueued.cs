using System;
using System.Collections.Generic;

namespace Photon.Voice
{
	public interface IEncoderQueued : IEncoder, IDisposable
	{
		IEnumerable<ArraySegment<byte>> GetOutput();
	}
}
