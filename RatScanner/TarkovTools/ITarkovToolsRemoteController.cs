﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Websocket.Client;
using Websocket.Client.Exceptions;
using Websocket.Client.Models;

namespace RatScanner.TarkovTools;

public interface ITarkovToolsRemoteController : IDisposable
{
	void Connect(string sessionId);
	void Disconnect();
	Task OpenItemAsync(string itemId);
	Task OpenAmmoChartAsync(string ammoType);
}

public class TarkovToolsRemoteController : ITarkovToolsRemoteController
{
	private const string wssUri = "wss://socket.tarkov.dev";

	private static readonly JsonSerializerSettings jsonSettings = new()
	{
		ContractResolver = new CamelCasePropertyNamesContractResolver(),
		NullValueHandling = NullValueHandling.Ignore,
		Converters = new List<JsonConverter>
		{
			new StringEnumConverter(new CamelCaseNamingStrategy()),
		},
	};

	private IWebsocketClient _client;

	private string _sessionId;

	private void InitWebsocketClient()
	{
		// Dispose (if necessary) and create new ws client
		_client?.Dispose();
		var uri = QueryHelpers.AddQueryString(wssUri, "sessionid", _sessionId);
		_client = new WebsocketClient(new Uri(uri));

		// Setup timeouts
		_client.ReconnectTimeout = TimeSpan.FromSeconds(5);
		_client.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);

		// Setup event handlers
		_client.MessageReceived.Subscribe(OnMessage);
		_client.DisconnectionHappened.Subscribe(OnDisconnect);
		_client.ReconnectionHappened.Subscribe(OnReconnect);
	}

	/// <summary>
	/// Connect the remote controller to Tarkov Tools
	/// </summary>
	/// <param name="sessionId">
	/// ID used to identify the remote control target.
	/// Throws if <paramref name="sessionId"/> is <see langword="null"/> or empty.
	/// </param>
	/// <returns>Task</returns>
	/// <exception cref="ArgumentException">Test</exception>
	public void Connect(string sessionId)
	{
		if (string.IsNullOrEmpty(sessionId))
			throw new ArgumentException("Invalid session ID", nameof(sessionId));

		_sessionId = sessionId;
	}

	public void Disconnect()
	{
		_sessionId = string.Empty;
	}

	private async Task ConnectInternalAsync()
	{
		if (_client == null)
			InitWebsocketClient();

		try
		{
			await _client.StartOrFail().ConfigureAwait(false);
			Send(PayloadType.Connect);
		}
		catch (WebsocketException e)
		{
			InitWebsocketClient();
			throw new TarkovToolsRemoteControllerException("Unable to connect to Tarkov Tools", e);
		}
	}

	private Task EnsureConnected()
	{
		if (string.IsNullOrEmpty(_sessionId))
			throw new TarkovToolsRemoteControllerException("Controller is not connected");

		return _client == null || !_client.IsStarted
			? ConnectInternalAsync()
			: Task.CompletedTask;
	}

	private void OnReconnect(ReconnectionInfo obj)
	{
		Send(PayloadType.Connect);
	}

	private void OnDisconnect(DisconnectionInfo obj)
	{
		// We should log disconnects once the logger utility of RatScanner
		// has been moved into its own project as we do not want to reference
		// RatScanner.csproj from RatTracking.csproj
	}

	private void OnMessage(ResponseMessage obj)
	{
		if (obj.MessageType != WebSocketMessageType.Text) return;
		var payloadType = JsonConvert.DeserializeObject<Payload>(obj.Text, jsonSettings)?.Type;
		if (payloadType is PayloadType.Ping) Send(PayloadType.Pong);
	}

	public async Task OpenItemAsync(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
			throw new ArgumentException("Invalid item ID", nameof(itemId));

		await EnsureConnected().ConfigureAwait(false);

		Send(PayloadType.Command, new CommandData
		{
			Type = CommandType.Item,
			Value = itemId,
		});
	}

	public async Task OpenAmmoChartAsync(string ammoType)
	{
		if (string.IsNullOrEmpty(ammoType))
			throw new ArgumentException("Invalid ammo type", nameof(ammoType));

		await EnsureConnected().ConfigureAwait(false);

		Send(PayloadType.Command, new CommandData
		{
			Type = CommandType.Ammo,
			Value = ammoType,
		});
	}

	void IDisposable.Dispose() => _client.Dispose();

	private void Send(PayloadType type, object? data = null)
	{
		var payload = new Payload
		{
			SessionID = _sessionId,
			Type = type,
			Data = data,
		};
		var json = JsonConvert.SerializeObject(payload, jsonSettings);
		_client.Send(json);
	}

	private class Payload
	{
		public string? SessionID { get; set; }
		public PayloadType Type { get; set; }
		public object? Data { get; set; }
	}

	private enum PayloadType
	{
		Command,
		Connect,
		Ping,
		Pong,
	}

	private class CommandData
	{
		public CommandType Type { get; set; }
		public string? Value { get; set; }
	}

	private enum CommandType
	{
		Item,
		Map,
		Ammo,
	}
}
