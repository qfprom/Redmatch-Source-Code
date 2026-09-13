namespace Photon.Voice
{
	public abstract class BufferReaderPushAdapterBase<T> : IServiceable
	{
		protected IDataReader<T> reader;

		public BufferReaderPushAdapterBase(IDataReader<T> reader)
		{
			this.reader = reader;
		}

		public abstract void Service(LocalVoice localVoice);

		public void Dispose()
		{
			reader.Dispose();
		}
	}
}
