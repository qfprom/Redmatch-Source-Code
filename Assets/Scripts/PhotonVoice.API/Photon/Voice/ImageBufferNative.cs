using System;

namespace Photon.Voice
{
	public class ImageBufferNative
	{
		public ImageBufferInfo Info { get; protected set; }

		public IntPtr[] Planes { get; protected set; }

		public ImageBufferNative(ImageBufferInfo info)
		{
			Info = info;
		}

		public virtual void Release()
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
