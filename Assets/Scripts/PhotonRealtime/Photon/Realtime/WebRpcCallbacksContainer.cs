using System;
using System.Diagnostics;
using System.Threading;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	internal class WebRpcCallbacksContainer : IWebRpcCallback
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<OperationResponse> OnWebRpcResponseActions__BackingField;

		internal event Action<OperationResponse> OnWebRpcResponseActions
		{
			add
			{
				Action<OperationResponse> action = OnWebRpcResponseActions__BackingField;
				Action<OperationResponse> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnWebRpcResponseActions__BackingField, (Action<OperationResponse>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<OperationResponse> action = OnWebRpcResponseActions__BackingField;
				Action<OperationResponse> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnWebRpcResponseActions__BackingField, (Action<OperationResponse>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public void AddCallbackTarget(IWebRpcCallback target)
		{
			OnWebRpcResponseActions += target.OnWebRpcResponse;
		}

		public void RemoveCallbackTarget(IWebRpcCallback target)
		{
			OnWebRpcResponseActions -= target.OnWebRpcResponse;
		}

		public void OnWebRpcResponse(OperationResponse response)
		{
			if (OnWebRpcResponseActions__BackingField != null)
			{
				OnWebRpcResponseActions__BackingField(response);
			}
		}
	}
}
