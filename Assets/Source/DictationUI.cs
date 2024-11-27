using TMPro;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AssistantAPI;
using System.Linq;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit;


public class DictationUI : MonoBehaviour {
	[SerializeField] private AssistantAPI _api;

	[SerializeField] private AudioSource _audioSource;

	[SerializeField] private AudioClip _activationSound;

	[SerializeField] private AudioClip _deactivationSound;

	[SerializeField] private Speech _speech;

	[SerializeField] private Gaze _gaze;

	[SerializeField] private RawImage _listeningIcon;

	[SerializeField] private RawImage _loadingIcon;

	[SerializeField] private TextMeshProUGUI _utterance;

	[SerializeField] private TextMeshProUGUI _answer;

	private TextToSpeechSubsystem _textToSpeechSubsystem;
	
	private string DEFAULT_PLACEHOLDER = "Say `Hey Hololens!` and ask me anything!";

	void OnEnable() {
		_listeningIcon.enabled = false;
		_loadingIcon.enabled = false;
		_utterance.text = DEFAULT_PLACEHOLDER;
		
		_textToSpeechSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();

		_speech.OnDictationStart += OnDictationStart_UI;
		_speech.OnDictationEnd += OnDictationEnd_UI;

		_speech.OnDictationStart += _gaze.StartRecordingUserPov;
		_speech.OnDictationEnd += OnDictationEnd;

		_api.OnAskStart += OnAskStart_UI;
		_api.OnAskAnswer += OnAskAnswer_UI;
	}

	void OnDisable() {
		_speech.OnDictationStart -= OnDictationStart_UI;
		_speech.OnDictationEnd -= OnDictationEnd_UI;

		_speech.OnDictationStart -= _gaze.StartRecordingUserPov;
		_speech.OnDictationEnd -= OnDictationEnd;

		_api.OnAskStart -= OnAskStart_UI;
		_api.OnAskAnswer -= OnAskAnswer_UI;
	}

	private void OnDictationStart_UI() {
		_answer.text = "";
		_utterance.text = "";
		_listeningIcon.enabled = true;

		_audioSource.clip = _activationSound;
		_audioSource.Play();
	}

	private void OnDictationEnd_UI(string str) {
		_audioSource.clip = _deactivationSound;
		_audioSource.Play();

		_listeningIcon.enabled = false;
		_utterance.text = str;
	}

	private void OnAskStart_UI() {
		_loadingIcon.enabled = true;
	}

	private void OnAskAnswer_UI(string jsonString) {
		_loadingIcon.enabled = false;
		_utterance.text = DEFAULT_PLACEHOLDER;
		Answer json = JsonUtility.FromJson<Answer>(jsonString);
		// _answer.text = json.answer;
		_textToSpeechSubsystem.TrySpeak(json.answer, _audioSource);
	}

	private void OnDictationEnd(string utterance) {
		var (vectors, screenshot) = _gaze.StopRecordingUserPov();

		var question = new Question(
			utterance,
			screenshot,
			vectors.Select(vec => StarePoint.From(vec)).ToArray()
		);
		StartCoroutine(_api.AskQuestion(question));

	}

}
