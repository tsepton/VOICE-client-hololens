using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssistantUI : MonoBehaviour {
	[SerializeField] private AssistantAPI _api;

	[SerializeField] private Speech _speech;

	[SerializeField] private Gaze _gaze;

	// FIXME - There should be only one AudioSource but multiple mp3
	// TODO loading sound while querying the server
	[SerializeField] private AudioSource _activationSound;

	[SerializeField] private AudioSource _deactivationSound;

	[SerializeField] private GameObject _userHeadInterface;

	[SerializeField] private RawImage _loadingIcon;

	[SerializeField] private RawImage _listeningIcon;

	[SerializeField] private TextMeshProUGUI _utterance;

	[SerializeField] private TextMeshProUGUI _answer;

	[SerializeField] private TextMeshProUGUI _networkStatus;

	private FileLogger _vassCustomFileLogger;

	void Start() {
		_vassCustomFileLogger = new FileLogger();

		_loadingIcon.enabled = false;
		_listeningIcon.enabled = false;

		_api.OnAskAnswer += (string str) => {
			_answer.text = str;
		};
		
		UpdateUiBasedOnNetworkStatus(_api.NetworkAvailability);
		_api.OnPingAnswer += UpdateUiBasedOnNetworkStatus;

		_speech.OnDictationEnd += OnDictationEnd;
		_speech.OnDictationStart += OnDictationStart;
	}

	private void OnDictationStart() {
		_utterance.text = "";
		_answer.text = "";
		_listeningIcon.enabled = true;
		_activationSound.Play();
	}

	private void OnDictationEnd(string str) {
		_deactivationSound.Play();
		_listeningIcon.enabled = false;
		_utterance.text = str;
	}

	private void UpdateUiBasedOnNetworkStatus(NetworkAvailability status) {
		switch (status) {
			case NetworkAvailability.Connected:
				_networkStatus.text = "Connected.";
				_networkStatus.gameObject.SetActive(false);
				_loadingIcon.enabled = false;
				
				_listeningIcon.enabled = true;
				_utterance.enabled = true;
				_answer.enabled = true;
				break;
			case NetworkAvailability.Error:
			case NetworkAvailability.Connecting:
			default:
				_listeningIcon.enabled = false;
				_utterance.text = "";
				_answer.text = "";
				
				_networkStatus.text = "Contacting Server...";
				_networkStatus.gameObject.SetActive(true);
				_loadingIcon.enabled = true;
				break;
		}
	}
}
