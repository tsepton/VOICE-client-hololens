using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour {
	[SerializeField] private AssistantAPI _api;

	[SerializeField] private RawImage _loadingIcon;

	[SerializeField] private TextMeshProUGUI _networkStatus;
	
	[SerializeField] private GameObject _dictationUI;

	private void Start() {
		_dictationUI.SetActive(false);
		
		_api.OnPingAnswer += UpdateUiBasedOnNetworkStatus;
		StartCoroutine(CheckConnectivityPeriodically());
	}

	private void UpdateUiBasedOnNetworkStatus(NetworkAvailability status) {
		switch (status) {
			case NetworkAvailability.Connected:
				_networkStatus.text = "";
				_loadingIcon.enabled = false;
				_dictationUI.SetActive(true);
				break;
			case NetworkAvailability.Error:
			case NetworkAvailability.Connecting:
			default:
				_dictationUI.SetActive(false);
				_networkStatus.text = "Contacting Server...";
				_loadingIcon.enabled = true;
				break;
		}
	}

	private IEnumerator CheckConnectivityPeriodically() {
		while (true) {
			StartCoroutine(_api.Ping());
			yield return new WaitForSeconds(30f);
		}
	}

}
