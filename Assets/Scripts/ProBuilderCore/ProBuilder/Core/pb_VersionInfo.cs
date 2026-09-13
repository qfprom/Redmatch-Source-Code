using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProBuilder.Core
{
	[Serializable]
	public class pb_VersionInfo : IEquatable<pb_VersionInfo>, IComparable<pb_VersionInfo>, IComparable
	{
		[SerializeField]
		private int m_Major = -1;

		[SerializeField]
		private int m_Minor = -1;

		[SerializeField]
		private int m_Patch = -1;

		[SerializeField]
		private int m_Build = -1;

		[SerializeField]
		private VersionType m_Type = VersionType.Missing;

		[SerializeField]
		private string m_Metadata;

		[SerializeField]
		private string m_Date;

		public const string DefaultStringFormat = "M.m.p-t.b";

		public int major
		{
			get
			{
				return m_Major;
			}
		}

		public int minor
		{
			get
			{
				return m_Minor;
			}
		}

		public int patch
		{
			get
			{
				return m_Patch;
			}
		}

		public int build
		{
			get
			{
				return m_Build;
			}
		}

		public VersionType type
		{
			get
			{
				return m_Type;
			}
		}

		public string metadata
		{
			get
			{
				return m_Metadata;
			}
		}

		public string date
		{
			get
			{
				return m_Date;
			}
		}

		public pb_VersionInfo MajorMinorPatch
		{
			get
			{
				return new pb_VersionInfo(major, minor, patch);
			}
		}

		public pb_VersionInfo()
		{
		}

		public pb_VersionInfo(string formatted, string date = null)
		{
			m_Metadata = formatted;
			m_Date = date;
			pb_VersionInfo version;
			if (TryGetVersionInfo(formatted, out version))
			{
				m_Major = version.m_Major;
				m_Minor = version.m_Minor;
				m_Patch = version.m_Patch;
				m_Build = version.m_Build;
				m_Type = version.m_Type;
				m_Metadata = version.metadata;
			}
		}

		public pb_VersionInfo(int major, int minor, int patch, int build = -1, VersionType type = VersionType.Missing, string date = "", string metadata = "")
		{
			m_Major = major;
			m_Minor = minor;
			m_Patch = patch;
			m_Build = build;
			m_Type = type;
			m_Metadata = metadata;
			m_Date = ((!string.IsNullOrEmpty(date)) ? date : DateTime.Now.ToString("en-US: MM/dd/yyyy"));
		}

		public bool IsValid()
		{
			return major != -1 && minor != -1 && patch != -1;
		}

		public override bool Equals(object o)
		{
			return o is pb_VersionInfo && Equals((pb_VersionInfo)o);
		}

		public override int GetHashCode()
		{
			int num = 13;
			if (IsValid())
			{
				num = num * 7 + major.GetHashCode();
				num = num * 7 + minor.GetHashCode();
				num = num * 7 + patch.GetHashCode();
				num = num * 7 + build.GetHashCode();
				return num * 7 + type.GetHashCode();
			}
			return (!string.IsNullOrEmpty(m_Metadata)) ? base.GetHashCode() : m_Metadata.GetHashCode();
		}

		public bool Equals(pb_VersionInfo version)
		{
			if (object.ReferenceEquals(version, null))
			{
				return false;
			}
			if (IsValid() != version.IsValid())
			{
				return false;
			}
			if (IsValid())
			{
				return major == version.major && minor == version.minor && patch == version.patch && type == version.type && build == version.build;
			}
			if (string.IsNullOrEmpty(m_Metadata) || string.IsNullOrEmpty(version.m_Metadata))
			{
				return false;
			}
			return m_Metadata.Equals(version.m_Metadata);
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as pb_VersionInfo);
		}

		private static int WrapNoValue(int value)
		{
			return (value >= 0) ? value : int.MaxValue;
		}

		public int CompareTo(pb_VersionInfo version)
		{
			if (object.ReferenceEquals(version, null))
			{
				return 1;
			}
			if (Equals(version))
			{
				return 0;
			}
			if (major > version.major)
			{
				return 1;
			}
			if (major < version.major)
			{
				return -1;
			}
			if (minor > version.minor)
			{
				return 1;
			}
			if (minor < version.minor)
			{
				return -1;
			}
			if (WrapNoValue(patch) > WrapNoValue(version.patch))
			{
				return 1;
			}
			if (WrapNoValue(patch) < WrapNoValue(version.patch))
			{
				return -1;
			}
			if (WrapNoValue((int)type) > WrapNoValue((int)version.type))
			{
				return 1;
			}
			if (WrapNoValue((int)type) < WrapNoValue((int)version.type))
			{
				return -1;
			}
			if (WrapNoValue(build) > WrapNoValue(version.build))
			{
				return 1;
			}
			if (WrapNoValue(build) < WrapNoValue(version.build))
			{
				return -1;
			}
			return 0;
		}

		public static bool operator ==(pb_VersionInfo left, pb_VersionInfo right)
		{
			if (object.ReferenceEquals(left, null))
			{
				return object.ReferenceEquals(right, null);
			}
			return left.Equals(right);
		}

		public static bool operator !=(pb_VersionInfo left, pb_VersionInfo right)
		{
			return !(left == right);
		}

		public static bool operator <(pb_VersionInfo left, pb_VersionInfo right)
		{
			if (object.ReferenceEquals(left, null))
			{
				return !object.ReferenceEquals(right, null);
			}
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(pb_VersionInfo left, pb_VersionInfo right)
		{
			if (object.ReferenceEquals(left, null))
			{
				return false;
			}
			return left.CompareTo(right) > 0;
		}

		public string ToString(string format)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			char[] array = format.ToCharArray();
			foreach (char c in array)
			{
				if (flag)
				{
					stringBuilder.Append(c);
					flag = false;
					continue;
				}
				switch (c)
				{
				case '\\':
					flag = true;
					break;
				case 'M':
					stringBuilder.Append(major);
					break;
				case 'm':
					stringBuilder.Append(minor);
					break;
				case 'p':
					stringBuilder.Append(patch);
					break;
				case 'b':
					stringBuilder.Append(build);
					break;
				case 't':
					stringBuilder.Append(char.ToLower(type.ToString()[0]));
					break;
				case 'T':
					stringBuilder.Append(type);
					break;
				case 'd':
					stringBuilder.Append(date);
					break;
				case 'D':
					stringBuilder.Append(metadata);
					break;
				default:
					stringBuilder.Append(c);
					break;
				}
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return ToString("M.m.p-t.b");
		}

		public static bool TryGetVersionInfo(string input, out pb_VersionInfo version)
		{
			version = new pb_VersionInfo();
			bool flag = false;
			try
			{
				Match match = Regex.Match(input, "([0-9]+\\.[0-9]+\\.[0-9]+)");
				if (!match.Success)
				{
					return false;
				}
				string[] array = match.Value.Split('.');
				int.TryParse(array[0], out version.m_Major);
				int.TryParse(array[1], out version.m_Minor);
				int.TryParse(array[2], out version.m_Patch);
				flag = true;
				Match match2 = Regex.Match(input, "(?i)(?<=\\-)[a-z0-9\\-\\.]+");
				if (!match2.Success)
				{
					match2 = Regex.Match(input, "(?<=[0-9]+\\.[0-9]+\\.[0-9]+)[a-z0-9\\-\\.\\+]+");
				}
				if (match2.Success)
				{
					version.m_Type = GetVersionType(match2.Value);
					version.m_Build = GetBuildNumber(match2.Value);
				}
				Match match3 = Regex.Match(input, "(?<=\\+).+");
				if (match3.Success)
				{
					version.m_Metadata = match3.Value;
				}
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		private static VersionType GetVersionType(string input)
		{
			if (Regex.IsMatch(input, "(?i)^(alpha|a)(?=[^a-z]|\\Z)", RegexOptions.Multiline))
			{
				return VersionType.Alpha;
			}
			if (Regex.IsMatch(input, "(?i)^(beta|b)(?=[^a-z]|\\Z)", RegexOptions.Multiline))
			{
				return VersionType.Beta;
			}
			if (Regex.IsMatch(input, "(?i)^(patch|p)(?=[^a-z]|\\Z)", RegexOptions.Multiline))
			{
				return VersionType.Patch;
			}
			if (Regex.IsMatch(input, "(?i)^(final|f)(?=[^a-z]|\\Z)", RegexOptions.Multiline))
			{
				return VersionType.Final;
			}
			if (Regex.IsMatch(input, "(?i)^(dev|d|development)(?=[^a-z]|\\Z)", RegexOptions.Multiline))
			{
				return VersionType.Development;
			}
			return VersionType.Missing;
		}

		private static int GetBuildNumber(string input)
		{
			Match match = Regex.Match(input, "[0-9]+");
			int result = 0;
			if (match.Success && int.TryParse(match.Value, out result))
			{
				return result;
			}
			return 0;
		}
	}
}
