namespace Photon.Chat
{
	public class ChannelCreationOptions
	{
		public static ChannelCreationOptions Default = new ChannelCreationOptions();

		public bool PublishSubscribers { get; set; }

		public int MaxSubscribers { get; set; }
	}
}
