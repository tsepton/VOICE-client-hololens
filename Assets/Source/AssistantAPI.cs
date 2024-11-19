using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AssistantAPI : MonoBehaviour {

	[SerializeField] private string _remote = "http://192.168.0.113:3000";

	[SerializeField] private Speech _speech;

	[SerializeField] private Gaze _gaze;

	public event Action OnAskStart;
	public event Action<string> OnAskAnswer;

	public event Action<NetworkAvailability> OnPingAnswer;

	void Start() {
		if (_remote == null || _remote == "") Debug.LogError("_remote URI was not set.");
	}

	public IEnumerator CheckStatus() {
		var endpoint = $"{_remote}/status";
		UnityWebRequest request = new UnityWebRequest(endpoint, "GET");
		request.SetRequestHeader("Content-Type", "application/json");
		request.downloadHandler = new DownloadHandlerBuffer();

		Debug.Log("Sending request to " + _remote);
		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success) {
			OnPingAnswer?.Invoke(NetworkAvailability.Connected);
			Debug.Log("Response: " + request.downloadHandler.text);
		} else {
			OnPingAnswer?.Invoke(NetworkAvailability.Error);
			Debug.LogError("Request failed: " + request.error);
		}
	}


	public IEnumerator AskQuestion(Question question) {
		OnAskStart?.Invoke();

		var endpoint = $"{_remote}/question";
		UnityWebRequest request = new UnityWebRequest(endpoint, "GET");
		request.SetRequestHeader("Content-Type", "application/json");
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(question.ToJson());
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.timeout = 500;

		Debug.Log("Sending request to " + _remote);
		Debug.Log("Request payload: " + question.ToJson());

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success) {
			OnAskAnswer?.Invoke(request.downloadHandler.text);
			Debug.Log("Response: " + request.downloadHandler.text);
		} else {
			OnAskAnswer?.Invoke("Error...");
			Debug.LogError("Request failed: " + request.error);
		};
	}

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

}

public enum NetworkAvailability {
	Connected, Connecting, Error
}