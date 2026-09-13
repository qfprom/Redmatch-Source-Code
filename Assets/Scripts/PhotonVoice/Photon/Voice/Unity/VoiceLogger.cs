using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity
{
	public class VoiceLogger : ILogger
	{
		private Object context;

		public string Tag { get; set; }

		public DebugLevel LogLevel { get; set; }

		public bool IsErrorEnabled
		{
			get
			{
				return (int)LogLevel >= 1;
			}
		}

		public bool IsWarningEnabled
		{
			get
			{
				return (int)LogLevel >= 2;
			}
		}

		public bool IsInfoEnabled
		{
			get
			{
				return (int)LogLevel >= 3;
			}
		}

		public bool IsDebugEnabled
		{
			get
			{
				return LogLevel == DebugLevel.ALL;
			}
		}

		public VoiceLogger(Object context, string tag, DebugLevel level = DebugLevel.ERROR)
		{
			this.context = context;
			Tag = tag;
			LogLevel = level;
		}

		public VoiceLogger(string tag, DebugLevel level = DebugLevel.ERROR)
		{
			Tag = tag;
			LogLevel = level;
		}

		public void LogError(string fmt, params object[] args)
		{
			if (IsErrorEnabled)
			{
				fmt = string.Format("[{0}] {1}", Tag, fmt);
				if (context == null)
				{
					Debug.LogErrorFormat(fmt, args);
				}
				else
				{
					Debug.LogErrorFormat(context, fmt, args);
				}
			}
		}

		public void LogWarning(string fmt, params object[] args)
		{
			if (IsWarningEnabled)
			{
				fmt = string.Format("[{0}] {1}", Tag, fmt);
				if (context == null)
				{
					Debug.LogWarningFormat(fmt, args);
				}
				else
				{
					Debug.LogWarningFormat(context, fmt, args);
				}
			}
		}

		public void LogInfo(string fmt, params object[] args)
		{
			if (IsInfoEnabled)
			{
				fmt = string.Format("[{0}] {1}", Tag, fmt);
				if (context == null)
				{
					Debug.LogFormat(fmt, args);
				}
				else
				{
					Debug.LogFormat(context, fmt, args);
				}
			}
		}

		public void LogDebug(string fmt, params object[] args)
		{
			if (IsDebugEnabled)
			{
				LogInfo(fmt, args);
			}
		}
	}
}
