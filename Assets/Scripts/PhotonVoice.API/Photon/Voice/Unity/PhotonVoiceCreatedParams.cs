namespace Photon.Voice.Unity
{
	public class PhotonVoiceCreatedParams
	{
		public LocalVoice Voice { get; internal set; }

		public IAudioDesc AudioDesc { get; internal set; }
	}
}
