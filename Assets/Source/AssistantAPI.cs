using System;
using System.Collections;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class AssistantAPI : MonoBehaviour {

	public event Action OnAskStart;

	public event Action<string> OnAskAnswer;

	public event Action<NetworkAvailability> OnStatusChanged;

	private NetworkAvailability status;

	public NetworkAvailability Status {
		get { return status; }
		set {
			status = value;
			OnStatusChanged?.Invoke(status);
		}
	}

	[SerializeField] public string remote = "ws://192.168.0.176:3000";

	[SerializeField] private Speech _speech;

	[SerializeField] private Gaze _gaze;


#if ENABLE_WINMD_SUPPORT
		
	private MessageWebSocket webSocket;
	
	private DataWriter messageWriter;
	
	async void Start() {
		await ConnectWebSocket();
		if (remote == null || remote == "") Debug.LogError("_remote URI was not set.");
	}
	
	private void OnDestroy() {
		webSocket?.Dispose();
		webSocket = null;
	}

	private async Task ConnectWebSocket() {
		try {
			Status =  NetworkAvailability.Connecting;

			webSocket = new MessageWebSocket();
			webSocket.Control.MessageType = SocketMessageType.Utf8;

			webSocket.MessageReceived += WebSocket_MessageReceived;
			webSocket.Closed += WebSocket_Closed;
			
			Uri serverUri = new Uri(remote);
			await webSocket.ConnectAsync(serverUri);
			messageWriter = new DataWriter(webSocket.OutputStream);
			Status =  NetworkAvailability.Connected;
			Debug.Log("Connected to distant VOICE server.");
			
			await SendMessage("Hello from HoloLens 2!"); // TODO - this is to be removed
		}
		catch (Exception e) {
			Status =  NetworkAvailability.Error;
			Debug.LogError($"WebSocket Error: {e.Message}");
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
				string receivedMessage = reader.ReadString(reader.UnconsumedBufferLength);
				Debug.Log("Message received: " + receivedMessage);
			}
		}
		catch (Exception ex) {
			Debug.LogError($"WebSocket Receive Error: {ex.Message}");
		}
	}

	private void WebSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args) {
		Debug.LogWarning($"WebSocket closed: Code={args.Code}, Reason={args.Reason}");
		Status =  NetworkAvailability.Closed;
	}


	public IEnumerator AskQuestion(Question question) {
		
		string wsMessageType = WebSocket_MessageType.Question;
		await SendMessage(question.ToJson());
		
		// TODO
		// if (request.result == UnityWebRequest.Result.Success) {
		// 	OnAskAnswer?.Invoke(request.downloadHandler.text);
		// 	Debug.Log("Response: " + request.downloadHandler.text);
		// } else {
		// 	OnAskAnswer?.Invoke("Error...");
		// 	Debug.LogError("Request failed: " + request.error);
		// };
	}
	
#endif
#if !ENABLE_WINMD_SUPPORT

	public IEnumerator AskQuestion(Question question) {
		throw new Exception("Not implemented");
	}

#endif

	[Serializable]
	public abstract class RestType {
		public string ToJson() {
			return JsonUtility.ToJson(this);
		}
	}

	[Serializable]
	public class Question : RestType {
		public string query;
		public string image;
		public StarePoint[] gaze;

		public Question(string query, string base64, StarePoint[] points, string mimeType = "image/jpeg") {
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

	// This class will change - This is temporary
	[Serializable]
	public class Answer : RestType {
		public string answer;
	}

	private static class WebSocket_MessageType {
		public const string Question = "question";
		public const string Monitor = "Monitor";
	}
}

public enum NetworkAvailability {
	Connected, Connecting, Error, Closed
}
