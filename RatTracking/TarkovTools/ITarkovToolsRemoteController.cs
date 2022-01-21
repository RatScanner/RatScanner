using System.Net.WebSockets;
using System.Reactive.Linq;
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
		void OpenItem(string itemId);
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

		public async Task ConnectAsync(string sessionId)
		{
			if (string.IsNullOrEmpty(sessionId))
				throw new TarkovToolsRemoteControllerException("Invalid ID for remote control");

			_sessionId = sessionId;

			try
			{
				await _client.StartOrFail().ConfigureAwait(false);
				Send(PayloadType.Connect);
			}
			catch (WebsocketException e)
			{
				throw new TarkovToolsRemoteControllerException("Unable to connect to Tarkov Tools", e);
			}
		}

		public Task DisconnectAsync() => _client.Stop(WebSocketCloseStatus.Empty, string.Empty);

		public void OpenItem(string itemId)
		{
			if (!_client.IsStarted)
				throw new TarkovToolsRemoteControllerException("Controller is not connected");

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
