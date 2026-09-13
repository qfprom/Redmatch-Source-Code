using System;
using UnityEngine;
using UnityEngine.Events;

public class DiscordController : MonoBehaviour
{
	public bool enableRichPresence = true;

	public DiscordRpc.RichPresence presence;

	public string applicationId;

	public string optionalSteamId;

	public int callbackCalls;

	public int clickCounter;

	public DiscordRpc.JoinRequest joinRequest;

	public UnityEvent onConnect;

	public UnityEvent onDisconnect;

	public UnityEvent hasResponded;

	public DiscordJoinEvent onJoin;

	public DiscordJoinEvent onSpectate;

	public DiscordJoinRequestEvent onJoinRequest;

	private DiscordRpc.EventHandlers handlers;

	public static DiscordController dc;

	private void Awake()
	{
		if ((bool)dc)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		dc = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void OnClick(bool disconnect)
	{
		if (disconnect)
		{
			DiscordRpc.Shutdown();
			return;
		}
		clickCounter++;
		presence.details = string.Format("Button clicked {0} times", clickCounter);
		DiscordRpc.UpdatePresence(ref presence);
	}

	public void RequestRespondYes()
	{
		DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
		hasResponded.Invoke();
	}

	public void RequestRespondNo()
	{
		DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
		hasResponded.Invoke();
	}

	public void ReadyCallback()
	{
		callbackCalls++;
		presence.partyMax = 0;
		presence.partySize = 0;
		presence.state = "In Lobby";
		DiscordRpc.UpdatePresence(ref presence);
		onConnect.Invoke();
	}

	public void ForceDisconnect()
	{
		DisconnectedCallback(0, "Forcing disconnection by user");
	}

	public void DisconnectedCallback(int errorCode, string message)
	{
		callbackCalls++;
		onDisconnect.Invoke();
	}

	public void ErrorCallback(int errorCode, string message)
	{
		callbackCalls++;
	}

	public void JoinCallback(string secret)
	{
		callbackCalls++;
		onJoin.Invoke(secret);
	}

	public void SpectateCallback(string secret)
	{
		callbackCalls++;
		onSpectate.Invoke(secret);
	}

	public void RequestCallback(ref DiscordRpc.JoinRequest request)
	{
		callbackCalls++;
		joinRequest = request;
		onJoinRequest.Invoke(request);
	}

	private void Update()
	{
		if (!enableRichPresence)
		{
			return;
		}
		DiscordRpc.RunCallbacks();
	}

	private void OnEnable()
	{
		if (!enableRichPresence)
		{
			return;
		}
		callbackCalls = 0;
		handlers = default(DiscordRpc.EventHandlers);
		handlers.readyCallback = ReadyCallback;
		DiscordController discordController = this;
		discordController.handlers.disconnectedCallback = (DiscordRpc.DisconnectedCallback)Delegate.Combine(discordController.handlers.disconnectedCallback, new DiscordRpc.DisconnectedCallback(DisconnectedCallback));
		DiscordController discordController2 = this;
		discordController2.handlers.errorCallback = (DiscordRpc.ErrorCallback)Delegate.Combine(discordController2.handlers.errorCallback, new DiscordRpc.ErrorCallback(ErrorCallback));
		DiscordController discordController3 = this;
		discordController3.handlers.joinCallback = (DiscordRpc.JoinCallback)Delegate.Combine(discordController3.handlers.joinCallback, new DiscordRpc.JoinCallback(JoinCallback));
		DiscordController discordController4 = this;
		discordController4.handlers.spectateCallback = (DiscordRpc.SpectateCallback)Delegate.Combine(discordController4.handlers.spectateCallback, new DiscordRpc.SpectateCallback(SpectateCallback));
		DiscordController discordController5 = this;
		discordController5.handlers.requestCallback = (DiscordRpc.RequestCallback)Delegate.Combine(discordController5.handlers.requestCallback, new DiscordRpc.RequestCallback(RequestCallback));
		DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
	}

	private void OnDisable()
	{
		if (!enableRichPresence)
		{
			return;
		}
		DiscordRpc.ClearPresence();
		DiscordRpc.UpdatePresence(ref presence);
		DiscordRpc.Shutdown();
	}

	private void OnApplicationQuit()
	{
		if (!enableRichPresence)
		{
			return;
		}
		DiscordRpc.ClearPresence();
		DiscordRpc.UpdatePresence(ref presence);
		DiscordRpc.Shutdown();
	}
}
