using System;
using UnityEngine;

namespace ProBuilder.Core
{
	[Serializable]
	internal class ChangelogEntry
	{
		[SerializeField]
		private pb_VersionInfo m_VersionInfo;

		[SerializeField]
		private string m_ReleaseNotes;

		public pb_VersionInfo versionInfo
		{
			get
			{
				return m_VersionInfo;
			}
		}

		public string releaseNotes
		{
			get
			{
				return m_ReleaseNotes;
			}
		}

		public ChangelogEntry(pb_VersionInfo version, string releaseNotes)
		{
			m_VersionInfo = version;
			m_ReleaseNotes = releaseNotes;
		}

		public override string ToString()
		{
			return m_VersionInfo.ToString() + "\n\n" + m_ReleaseNotes;
		}
	}
}
