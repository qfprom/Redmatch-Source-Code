using System;

namespace Photon.Realtime
{
	public enum DisconnectCause
	{
		None = 0,
		ExceptionOnConnect = 1,
		Exception = 2,
		ServerTimeout = 3,
		[Obsolete("Replace with: ServerTimeout (same value).")]
		DisconnectByServer = ServerTimeout,
		ClientTimeout = 4,
		[Obsolete("Replace with: ClientTimeout (same value).")]
		TimeoutDisconnect = ClientTimeout,
		DisconnectByServerLogic = 5,
		DisconnectByServerReasonUnknown = 6,
		InvalidAuthentication = 7,
		CustomAuthenticationFailed = 8,
		AuthenticationTicketExpired = 9,
		MaxCcuReached = 10,
		[Obsolete("Replace with: MaxCcuReached (same value).")]
		DisconnectByServerUserLimit = MaxCcuReached,
		InvalidRegion = 11,
		OperationNotAllowedInCurrentState = 12,
		DisconnectByClientLogic = 13
	}
}
