using System;

namespace Photon.Voice
{
	public struct RemoteVoiceOptions
	{
		public Action<byte[]> OnDecodedFrameByteAction { get; set; }

		public Action<float[]> OnDecodedFrameFloatAction { get; set; }

		public Action<short[]> OnDecodedFrameShortAction { get; set; }

		public Action OnRemoteVoiceRemoveAction { get; set; }

		public IDecoder Decoder { get; set; }
	}
}
