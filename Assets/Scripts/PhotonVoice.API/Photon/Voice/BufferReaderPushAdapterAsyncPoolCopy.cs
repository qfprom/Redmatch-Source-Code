using System;

namespace Photon.Voice
{
	public class BufferReaderPushAdapterAsyncPoolCopy<T> : BufferReaderPushAdapterBase<T>
	{
		protected T[] buffer;

		public BufferReaderPushAdapterAsyncPoolCopy(LocalVoice localVoice, IDataReader<T> reader)
			: base(reader)
		{
			buffer = new T[((LocalVoiceFramedBase)localVoice).FrameSize];
		}

		public override void Service(LocalVoice localVoice)
		{
			while (reader.Read(buffer))
			{
				LocalVoiceFramed<T> localVoiceFramed = (LocalVoiceFramed<T>)localVoice;
				T[] array = localVoiceFramed.BufferFactory.New();
				Array.Copy(buffer, array, buffer.Length);
				localVoiceFramed.PushDataAsync(array);
			}
		}
	}
}
