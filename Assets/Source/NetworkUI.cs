using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour {
	[SerializeField] private AssistantAPI _api;

	[SerializeField] private RawImage _loadingIcon;

	[SerializeField] private MRTKUGUIInputField _networkUrl;

	[SerializeField] private GameObject _dictationUI;

	[SerializeField] private GameObject _modalities;

	private void Start() {
		_dictationUI.SetActive(false);
		_modalities.SetActive(false);

		UpdateUiBasedOnNetworkStatus(_api.Status);
		_api.OnStatusChanged += UpdateUiBasedOnNetworkStatus;

		_networkUrl.text = _api.remote;
		_networkUrl.onEndEdit.AddListener((string address) => {
			_api.remote = address;
		});

	}

	private void UpdateUiBasedOnNetworkStatus(NetworkAvailability status) {
		switch (status) {
			case NetworkAvailability.Connected:
				gameObject.SetActive(false);
				_modalities.SetActive(true);
				_dictationUI.SetActive(true);
				break;
			case NetworkAvailability.Error:
			case NetworkAvailability.Connecting:
			default:
				gameObject.SetActive(true);
				_dictationUI.SetActive(false);
				_modalities.SetActive(false);
				_loadingIcon.enabled = _api.remote != null && _api.remote != "";
				break;
		}
	}

}
