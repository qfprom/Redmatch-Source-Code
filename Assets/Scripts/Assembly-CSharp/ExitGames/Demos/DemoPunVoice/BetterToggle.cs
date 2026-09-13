using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoPunVoice
{
	[RequireComponent(typeof(Toggle))]
	[DisallowMultipleComponent]
	public class BetterToggle : MonoBehaviour
	{
		public delegate void OnToggle(Toggle toggle);

		private Toggle toggle;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static OnToggle ToggleValueChanged__BackingField;

		public static event OnToggle ToggleValueChanged
		{
			add
			{
				OnToggle onToggle = ToggleValueChanged__BackingField;
				OnToggle onToggle2;
				do
				{
					onToggle2 = onToggle;
					onToggle = Interlocked.CompareExchange(ref ToggleValueChanged__BackingField, (OnToggle)Delegate.Combine(onToggle2, value), onToggle);
				}
				while ((object)onToggle != onToggle2);
			}
			remove
			{
				OnToggle onToggle = ToggleValueChanged__BackingField;
				OnToggle onToggle2;
				do
				{
					onToggle2 = onToggle;
					onToggle = Interlocked.CompareExchange(ref ToggleValueChanged__BackingField, (OnToggle)Delegate.Remove(onToggle2, value), onToggle);
				}
				while ((object)onToggle != onToggle2);
			}
		}

		private void Start()
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(delegate
			{
				OnToggleValueChanged();
			});
		}

		public void OnToggleValueChanged()
		{
			if (ToggleValueChanged__BackingField != null)
			{
				ToggleValueChanged__BackingField(toggle);
			}
		}
	}
}
