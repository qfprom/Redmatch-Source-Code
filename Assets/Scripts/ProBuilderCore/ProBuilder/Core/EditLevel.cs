using System;

namespace ProBuilder.Core
{
	[Flags]
	public enum EditLevel
	{
		Top = 0,
		Geometry = 1,
		Texture = 2,
		Plugin = 4
	}
}
