using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour {
	[SerializeField] private AssistantAPI _api;

	[SerializeField] private RawImage _loadingIcon;

	[SerializeField] private MRTKUGUIInputField _networkUrl;

	[SerializeField] private PressableButton _loadPreviousCheckbox;

	[SerializeField] private PressableButton _connectButton;

	[SerializeField] private GameObject _dictationGUI;

	[SerializeField] private GameObject _networkGUI;

	[SerializeField] private GameObject _modalities;

	private void Start() {
		_dictationGUI.SetActive(false);
		_modalities.SetActive(false);

		UpdateUiBasedOnNetworkStatus(_api.Status);
		_api.OnStatusChanged += UpdateUiBasedOnNetworkStatus;

		_networkUrl.text = _api.remote;
		_networkUrl.onEndEdit.AddListener((string address) => {
			_api.remote = address;
		});


		_connectButton.OnClicked.AddListener(() => {

			string uuid = null;
			if (_loadPreviousCheckbox.IsToggled) {
				uuid = RetrieveLastConversationUuid();
			}
			_api.InitChat(uuid);

		});

	}

	private string RetrieveLastConversationUuid() {
		throw new System.Exception("Not implemented");
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
				_loadingIcon.enabled = _api.remote != null && _api.remote != "";
				break;
		}
	}

}
