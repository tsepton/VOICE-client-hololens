using System;
using System.Collections;
using System.Linq;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AssistantAPI;


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

	[SerializeField] private PressableButton _resetBtn;

	private TextToSpeechSubsystem _textToSpeechSubsystem;

	private string DEFAULT_PLACEHOLDER = "Say \"Hey VOICE!\" and ask me anything!";

	void OnEnable() {
		_resetBtn.gameObject.SetActive(false);
		_loadingIcon.enabled = false;
		_listeningIcon.enabled = false;
		_utterance.text = DEFAULT_PLACEHOLDER;
		_answer.gameObject.SetActive(false);

		_textToSpeechSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();

		_speech.OnDictationStart += OnDictationStart_UI;
		_speech.OnDictationEnd += OnDictationEnd_UI;

		_speech.OnDictationStart += _gaze.StartRecordingUserPov;
		_speech.OnDictationEnd += OnDictationEnd;

		_api.OnAskStart += OnAskStart_UI;
		_api.OnAskAnswer += OnAskAnswer_UI;

		_resetBtn.OnClicked.AddListener(ResetDictation);
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
		// _answer.text = "";
		_utterance.text = "";
		_listeningIcon.enabled = true;
		_resetBtn.gameObject.SetActive(true);

		_audioSource.clip = _activationSound;
		_audioSource.Play();
		_resetBtn.gameObject.SetActive(true);
	}

	private void OnDictationEnd_UI(string str) {
		_audioSource.clip = _deactivationSound;
		_audioSource.Play();

		_utterance.text = str;
		_listeningIcon.enabled = false;
		_resetBtn.gameObject.SetActive(false);
	}

	private void OnAskStart_UI() {
		_loadingIcon.enabled = true;
	}

	private void OnAskAnswer_UI(Answer message) {
		_loadingIcon.enabled = false;
		_utterance.text = DEFAULT_PLACEHOLDER;
		_textToSpeechSubsystem.TrySpeak(message.text, _audioSource);
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

	private void ResetDictation() {
		StartCoroutine(ResetDictationCoroutine());
	}

	private IEnumerator ResetDictationCoroutine() {
		Debug.Log("Resetting dictation 1");
		yield return StartCoroutine(_speech.Restart());
		Debug.Log("Resetting dictation 2");
		_loadingIcon.enabled = false;
		Debug.Log("Resetting dictation 3");
		_utterance.text = DEFAULT_PLACEHOLDER;
		Debug.Log("Resetting dictation 4");
		_resetBtn.gameObject.SetActive(false);
	}
}
