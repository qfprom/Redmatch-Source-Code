using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity
{
	public abstract class VoiceComponent : MonoBehaviour, ILoggable
	{
		private VoiceLogger logger;

		[SerializeField]
		protected DebugLevel logLevel = DebugLevel.ERROR;

		public VoiceLogger Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new VoiceLogger(this, string.Format("{0}.{1}", base.name, GetType().Name), logLevel);
				}
				return logger;
			}
			protected set
			{
				logger = value;
			}
		}

		public DebugLevel LogLevel
		{
			get
			{
				if (Logger != null)
				{
					logLevel = Logger.LogLevel;
				}
				return logLevel;
			}
			set
			{
				logLevel = value;
				if (Logger != null)
				{
					Logger.LogLevel = logLevel;
				}
			}
		}

		protected virtual void Awake()
		{
			if (logger == null)
			{
				logger = new VoiceLogger(this, string.Format("{0}.{1}", base.name, GetType().Name), logLevel);
			}
		}
	}
}
