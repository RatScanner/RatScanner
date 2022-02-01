using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Websocket.Client;
using Websocket.Client.Exceptions;

namespace RatTracking.TarkovTools
{
	public interface ITarkovToolsRemoteController : IDisposable
	{
		Task ConnectAsync(string sessionId);
		Task DisconnectAsync();
		Task OpenItemAsync(string itemId);
	}

	public class TarkovToolsRemoteController : ITarkovToolsRemoteController
	{
		private const string wssUri = "wss://tarkov-tools-live.herokuapp.com";
		private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			NullValueHandling = NullValueHandling.Ignore,
			Converters = new List<JsonConverter>
			{
				new StringEnumConverter(new CamelCaseNamingStrategy())
			}
		};
		private static readonly Lazy<PropertyInfo> WebsocketClientIsRunningProperty = new Lazy<PropertyInfo>(
			() => typeof(WebsocketClient).GetProperty(nameof(WebsocketClient.IsRunning)));

		private readonly IWebsocketClient _client;
		private string? _sessionId;

		public TarkovToolsRemoteController()
		{
			_client = new WebsocketClient(new Uri(wssUri));

			_client.MessageReceived
				.Where(p => p.MessageType == WebSocketMessageType.Text)
				.Where(p => JsonConvert.DeserializeObject<Payload>(p.Text, jsonSettings)?.Type == PayloadType.Ping)
				.Subscribe(_ => SendPong());
		}

		public Task ConnectAsync(string sessionId)
		{
			if (string.IsNullOrEmpty(sessionId))
				throw new ArgumentException("Invalid session ID", nameof(sessionId));

			_sessionId = sessionId;

			return ConnectInternalAsync();
		}

		private async Task ConnectInternalAsync()
		{
			try
			{
				await _client.StartOrFail().ConfigureAwait(false);
				Send(PayloadType.Connect);
			}
			catch (WebsocketException e)
			{
				await ForceDisconnectAsync().ConfigureAwait(false);
				throw new TarkovToolsRemoteControllerException("Unable to connect to Tarkov Tools", e);
			}
		}

		public Task DisconnectAsync()
		{
			_sessionId = string.Empty;
			return DisconnectInternalAsync();
		}

		private Task DisconnectInternalAsync() => _client.Stop(WebSocketCloseStatus.Empty, string.Empty);

		private Task ForceDisconnectAsync()
		{
			// If the WebsocketClient.Start method fails to open web socket then:
			// - any subsequent WebsocketClient.Start calls do nothing, because IsStarted is true
			// - any subsequent WebsocketClient.Stop calls do nothing, because IsRunning is false
			// Workaround: update the IsRunning property via reflection.
			WebsocketClientIsRunningProperty.Value.SetValue(_client, true);
			return DisconnectInternalAsync();
		}

		public async Task OpenItemAsync(string itemId)
		{
			if (string.IsNullOrEmpty(itemId))
				throw new ArgumentException("Invalid item ID", nameof(itemId));

			if (string.IsNullOrEmpty(_sessionId))
				throw new TarkovToolsRemoteControllerException("Controller is not connected");

			if (!_client.IsStarted)
				await ConnectInternalAsync().ConfigureAwait(false);

			Send(PayloadType.Command, new CommandData
			{
				Type = CommandType.Item,
				Value = itemId
			});
		}

		void IDisposable.Dispose() => _client.Dispose();

		private void SendPong() => Send(PayloadType.Pong);

		private void Send(PayloadType type, object? data = null)
		{
			var payload = new Payload
			{
				SessionID = _sessionId,
				Type = type,
				Data = data
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
			Pong
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
			Ammo
		}
	}
}
