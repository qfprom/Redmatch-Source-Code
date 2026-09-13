using Photon.Voice.Unity;

namespace Photon.Voice.DemoVoiceUI
{
	public struct MicRef
	{
		public Recorder.MicType MicType;

		public string Name;

		public int PhotonId;

		public MicRef(string name, int id)
		{
			MicType = Recorder.MicType.Photon;
			Name = name;
			PhotonId = id;
		}

		public MicRef(string name)
		{
			MicType = Recorder.MicType.Unity;
			Name = name;
			PhotonId = -1;
		}

		public override string ToString()
		{
			return string.Format("Mic reference: {0}", Name);
		}
	}
}
