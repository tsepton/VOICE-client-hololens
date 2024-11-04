using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssistantUI : MonoBehaviour {

	[SerializeField] public Speech speech;

	[SerializeField] public Gaze gaze;

	[SerializeField] public AudioSource activationSound;
	
	[SerializeField] public AudioSource deactivationSound;
	
	// TODO loading sound while querying the server

	[SerializeField] public GameObject userHeadInterface;

	private FileLogger _vassCustomFileLogger;
	
	private RawImage _loadingIcon;
	
	private TextMeshProUGUI _utterance;

	void Start() {
		_vassCustomFileLogger = new FileLogger();

		speech.OnDictationStart += OnDictationStart;
		speech.OnDictationEnd += (string utterance) => OnDictationEnd(utterance);
		
		_loadingIcon = userHeadInterface.GetComponentInChildren<RawImage>();
		_loadingIcon.enabled = false;
		_utterance = userHeadInterface.GetComponentInChildren<TextMeshProUGUI>();
	}

	void OnDictationStart() {
		_loadingIcon.enabled = true;
		activationSound.Play();
	}
	
	void OnDictationEnd(string str) {
		_loadingIcon.enabled = false;
		_utterance.text = str;
		deactivationSound.Play();
	}
}
