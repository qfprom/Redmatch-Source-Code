using System;

public static class BuildConstants
{
	public enum ReleaseType
	{
		None = 0
	}

	public enum Platform
	{
		None = 0
	}

	public enum Architecture
	{
		None = 0
	}

	public enum Distribution
	{
		None = 0
	}

	public static readonly DateTime buildDate = DateTime.Now;

	public const string version = "";

	public const ReleaseType releaseType = ReleaseType.None;

	public const Platform platform = Platform.None;

	public const Architecture architecture = Architecture.None;

	public const Distribution distribution = Distribution.None;
}
