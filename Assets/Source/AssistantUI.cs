using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssistantUI : MonoBehaviour {

	[SerializeField] private Speech speech;

	[SerializeField] private Gaze gaze;

	// FIXME - There should be only one AudioSource but multiple mp3
	// TODO loading sound while querying the server
	[SerializeField] private AudioSource activationSound;
	
	[SerializeField] private AudioSource deactivationSound;
	
	[SerializeField] private GameObject userHeadInterface;
	
	[SerializeField] private RawImage _loadingIcon;
	
	[SerializeField] private RawImage _listeningIcon;
	
	[SerializeField] private TextMeshProUGUI _utterance;

	private FileLogger _vassCustomFileLogger;

	void Start() {
		_vassCustomFileLogger = new FileLogger();

		speech.OnDictationStart += OnDictationStart;
		speech.OnDictationEnd += (string utterance) => OnDictationEnd(utterance);
		
		_loadingIcon.enabled = false;
		_listeningIcon.enabled = false;
	}

	void OnDictationStart() {
		_listeningIcon.enabled = true;
		activationSound.Play();
	}
	
	void OnDictationEnd(string str) {
		_listeningIcon.enabled = false;
		_utterance.text = str;
		deactivationSound.Play();
	}
}
