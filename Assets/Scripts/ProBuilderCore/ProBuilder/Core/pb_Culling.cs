using System;

namespace ProBuilder.Core
{
	[Flags]
	public enum pb_Culling
	{
		None = 0,
		Back = 1,
		Front = 2,
		FrontBack = Back | Front
	}
}
