using System.IO;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour {
	[SerializeField] private AssistantAPI _api;

	[SerializeField] private MRTKUGUIInputField _networkUrl;

	[SerializeField] private PressableButton _loadPreviousCheckbox;

	[SerializeField] private PressableButton _connectButton;

	[SerializeField] private GameObject _dictationGUI;

	[SerializeField] private GameObject _networkGUI;

	[SerializeField] private GameObject _modalities;

	private void Start() {
		_dictationGUI.SetActive(false);
		_modalities.SetActive(false);

		bool previousConvExists = RetrieveLastConversation() != null;
		_loadPreviousCheckbox.gameObject.SetActive(previousConvExists);
		_api.OnInfoReceived += SaveConversation;

		UpdateUiBasedOnNetworkStatus(_api.Status);
		_api.OnStatusChanged += UpdateUiBasedOnNetworkStatus;

		_networkUrl.text = _api.remote;
		_networkUrl.onEndEdit.AddListener((string address) => {
			_api.remote = address;
		});

		_connectButton.OnClicked.AddListener(() => {
			string uuid = null;
			if (_loadPreviousCheckbox.IsToggled) {
				uuid = RetrieveLastConversation()?.uuid;
			}
			_api.InitChat(uuid);
		});
	}

	private AssistantAPI.ConversationInfo? RetrieveLastConversation() {
		// FIXME - constante
		string fileName = "conversation_history.txt";
		return FileHelper.Load<AssistantAPI.ConversationInfo>(fileName);
	}

	private void SaveConversation(AssistantAPI.ConversationInfo conv) {
		// FIXME - constante
		string fileName = "conversation_history.txt";

		FileHelper.Save<AssistantAPI.ConversationInfo>(conv, fileName);
	}

	private void UpdateUiBasedOnNetworkStatus(NetworkAvailability status) {
		Debug.Log(status);
		switch (status) {
			case NetworkAvailability.Connected:
				_networkGUI.SetActive(false);
				_modalities.SetActive(true);
				_dictationGUI.SetActive(true);
				break;
			default:
				_networkGUI.SetActive(true);
				_dictationGUI.SetActive(false);
				_modalities.SetActive(false);
				break;
		}
	}

	public static class FileHelper {

		public static T? Load<T>(string fileName) {
			string path = Path.Combine(Application.persistentDataPath, fileName);
			if (File.Exists(path)) {
				string json = File.ReadAllText(path);
				return JsonUtility.FromJson<T>(json);
			} else {
				Debug.LogWarning($"File not found at: {path}");
				return default;
			}
		}

		public static void Save<T>(T obj, string fileName) {
			string path = Path.Combine(Application.persistentDataPath, fileName);
			string json = JsonUtility.ToJson(obj, true);

			try {
				File.WriteAllText(path, json);
				Debug.Log($"File saved to: {path}");
			} catch (IOException e) {
				Debug.LogError($"Failed to save file at: {path}\n{e.Message}");
			}
		}
	}

}
