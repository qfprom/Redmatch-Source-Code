using System;

namespace ProBuilder.Core
{
	[Flags]
	public enum pb_LogLevel
	{
		None = 0,
		Error = 1,
		Warning = 2,
		Info = 4,
		Default = Error | Warning,
		All = 0xFF
	}
}
