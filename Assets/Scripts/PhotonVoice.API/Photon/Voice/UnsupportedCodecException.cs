using System;

namespace Photon.Voice
{
	internal class UnsupportedCodecException : Exception
	{
		public UnsupportedCodecException(string info, Codec codec, ILogger logger)
			: base("[PV] " + info + ": unsupported codec: " + codec)
		{
		}
	}
}
