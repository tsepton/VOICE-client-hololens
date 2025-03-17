using System;
using System.Collections;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif
using Answer = System.String;

public class AssistantAPI : MonoBehaviour {

	public event Action OnAskStart;

	public event Action<Answer> OnAskAnswer;

	public event Action<ConversationInfo> OnInfoReceived;

	public event Action<NetworkAvailability> OnStatusChanged;

	private NetworkAvailability status = NetworkAvailability.Closed;

	public NetworkAvailability Status {
		get { return status; }
		private set {
			status = value;
			OnStatusChanged?.Invoke(status);
			Debug.Log($"Websocket Status changed {status}");
		}
	}

	[SerializeField] public string remote = "0.0.0.0:3000";

	[SerializeField] private MainThreadDispatcher _mainThreadDispatcher;

#if ENABLE_WINMD_SUPPORT
		
	// TODO ? 
	// Maybe a wrapper around the websocket that has the conversation state could be useful 
	// This way, we could init multiple conversations at once with different chat history 
	// For instance, a chat history for a room, another for the living space
	// Think it through
	private MessageWebSocket webSocket;
	
	private DataWriter messageWriter;
	
	public async void InitChat(string? uuid) {
		if (remote == null || remote == "") Debug.LogError("Remote URI was not set.");
		else if (webSocket != null) Debug.LogError("Websocket connection already established."); 
		else await ConnectWebSocket(uuid);
	}
	
	private void OnDestroy() {
		webSocket?.Dispose();
		webSocket = null;
	}

	private async Task ConnectWebSocket(string? uuid) {
		try {
			Status =  NetworkAvailability.Connecting;

			webSocket = new MessageWebSocket();
			webSocket.Control.MessageType = SocketMessageType.Utf8;

			webSocket.MessageReceived += WebSocket_MessageReceived;
			webSocket.Closed += WebSocket_Closed;
			
			string uriParams = uuid != null ? $"uuid={uuid}" : "";
			Uri serverUri = new Uri($"ws://{remote}/chat?{uriParams}");
			messageWriter = new DataWriter(webSocket.OutputStream);
			await webSocket.ConnectAsync(serverUri);
			Status =  NetworkAvailability.Connected;
		}
		catch (Exception e) {
			Status =  NetworkAvailability.Error;
			Debug.LogError($"WebSocket Error: {e}");
			Debug.LogError(e.Message);
		}
	}

	private new async Task SendMessage(string message) {
		if (webSocket == null || messageWriter == null) {
			Debug.LogWarning("WebSocket not connected.");
			return;
		}

		messageWriter.WriteString(message);
		await messageWriter.StoreAsync();
		Debug.Log("Message sent: " + message);
	}

	private void WebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) {
		try {
			using (DataReader reader = args.GetDataReader()) {
				reader.UnicodeEncoding = UnicodeEncoding.Utf8;
				string serialized = reader.ReadString(reader.UnconsumedBufferLength);
				Debug.Log("Message received: " + serialized);
				_mainThreadDispatcher.Enqueue(() => HandleCallback(Deserialize(serialized)));
			}
		}
		catch (Exception ex) {
			Debug.LogError($"WebSocket Receive Error: {ex.Message}");
		}
	}

	private void WebSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args) {
		Status =  NetworkAvailability.Closed;
		Debug.LogWarning($"WebSocket closed: Code={args.Code}, Reason={args.Reason}");
	}


	public IEnumerator AskQuestion(Question question) {
		if (Status != NetworkAvailability.Connected) {
		    Debug.LogError("Cannot send Question to remote. Websocket connection is not established.");
			yield return null;
		}
		OnAskStart?.Invoke();
		SendMessage(question.ToJson()); 
		yield return null;
	}
	

	private Message Deserialize(string json) {
		Message message = JsonUtility.FromJson<Message>(json);
		switch (message.type) {
			case "answer":
				return JsonUtility.FromJson<Answer>(json);
			case "info":
				return JsonUtility.FromJson<ConversationInfo>(json);
			default:
				return message;
		};
	}

	private void HandleCallback(Message message) {
		switch (message){
			case Answer answer:
				OnAskAnswer?.Invoke(answer);
				break;
			case ConversationInfo info:
				OnInfoReceived?.Invoke(info);
				break;
			default:
				Debug.LogWarning("Unhandled message type.");
				break;
		}
	}
	
#endif
#if !ENABLE_WINMD_SUPPORT

	public IEnumerator AskQuestion(Question question) {
		throw new Exception("Not implemented");
	}

	public async void InitChat(string? uuid) {
		throw new Exception("Not implemented");
	}

#endif

	// UP
	[Serializable]
	public abstract class RestType {

		public string ToJson() {
			return JsonUtility.ToJson(this);
		}
	}

	[Serializable]
	public class Message : RestType {
		public string type;
	}

	[Serializable]
	public class Question : Message {
		public string query;
		public string image;
		public StarePoint[] gaze;

		public Question(string query, string base64, StarePoint[] points, string mimeType = "image/jpeg") {
			this.type = "question";
			this.query = query;
			this.image = $"data:{mimeType};base64,{base64}";
			this.gaze = points;
		}
	}

	[Serializable]
	public class StarePoint : RestType {
		public int x;
		public int y;

		public StarePoint(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public static StarePoint From(Vector2 vector) {
			return new StarePoint((int)vector.x, (int)vector.y);
		}
	}

	[Serializable]
	public class MonitoringData : Message {

		public MonitoringData() {
			this.type = "monitoring";
		}

		// TODO
	}


	[Serializable]
	public class ConversationInfo : Message {
		public string uuid;


		public ConversationInfo(string uuid) {
			this.type = "info";
			this.uuid = uuid;
		}
	}

	[Serializable]
	public class Answer : Message {
		public string text;

		public Answer(string text) {
			this.type = "answer";
			this.text = text;
		}
	}

}

public enum NetworkAvailability {
	Connected, Connecting, Error, Closed
}
