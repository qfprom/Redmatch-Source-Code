using System;

namespace ProBuilder.Core
{
	[Flags]
	public enum pb_LogOutput
	{
		None = 0,
		Console = 1,
		File = 2
	}
}
