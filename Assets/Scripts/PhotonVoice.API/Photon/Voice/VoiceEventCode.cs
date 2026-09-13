namespace Photon.Voice
{
	internal class VoiceEventCode
	{
		public const byte Code0 = 201;

		public static byte GetCode(int channelID)
		{
			return (byte)(201 + channelID);
		}

		public static bool TryGetChannelID(byte evCode, int maxChannels, out byte channelID)
		{
			if (evCode >= 201 && evCode < 201 + maxChannels)
			{
				channelID = (byte)(evCode - 201);
				return true;
			}
			channelID = 0;
			return false;
		}
	}
}
