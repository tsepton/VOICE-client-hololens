using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;

public class Speech : MonoBehaviour {
	[SerializeField] private List<PhraseAction> _phraseActions;

	private DictationSubsystem _dictationSubsystem;

	public bool IsDictationSystemListening => _dictationSubsystem.running;

	public event Action OnDictationStart;

	public event Action<string> OnDictationEnd;

	private void Start() {
		Boot();
	}

	private void OnRecognized(DictationResultEventArgs arg) {
		Debug.Log($"Recognized: ''{arg.Result}'' with a confidence of {arg.Confidence}");
		_dictationSubsystem.StopDictation();
		OnDictationEnd?.Invoke(arg.Result);
	}

	private void OnRecognitionFinished(DictationSessionEventArgs arg) {
		if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Running) {
			PhraseRecognitionSystem.Restart();
		}
	}

	public void StartDictation() {
		if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running) {
			PhraseRecognitionSystem.Shutdown();
		}
		_dictationSubsystem.StartDictation();
		OnDictationStart?.Invoke();
	}

	private void Boot() {
		// Phrase Recognition System
		var phraseRecognitionSubsystem = XRSubsystemHelpers.KeywordRecognitionSubsystem;
		foreach (var phraseAction in _phraseActions) {
			if (!string.IsNullOrEmpty(phraseAction.Phrase) &&
				phraseAction.Action.GetPersistentEventCount() > 0) {
				phraseRecognitionSubsystem.
					CreateOrGetEventForKeyword(phraseAction.Phrase).
						AddListener(() => phraseAction.Action.Invoke());
			}
		}

		// Dictation System
		BootDictation();
	}

	private void BootDictation() {
		_dictationSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<DictationSubsystem>();
		if (_dictationSubsystem != null) {
			_dictationSubsystem.Recognized += OnRecognized;
			_dictationSubsystem.RecognitionFinished += OnRecognitionFinished;
			_dictationSubsystem.RecognitionFaulted += OnRecognitionFinished;
		}
	}

	public System.Collections.IEnumerator Restart() {
		if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running) {
			Debug.Log("PhraseRecognitionSystem.Shutdown");
			PhraseRecognitionSystem.Shutdown();
		}
		if (_dictationSubsystem.running) {
			Debug.Log("_dictationSubsystem.StopDictation");
			_dictationSubsystem.StopDictation();
			Debug.Log(_dictationSubsystem.running); // The doc mentions that it is true only when Stop is called 
			yield return new WaitForSeconds(1);
			Debug.Log(_dictationSubsystem.running);
			// TODO: Fill in a bug within the DictationSubsystem repository
			// FIXME ! StopDictation is not stopping the dictation subsystem
			// No error is thrown, but the dictation subsystem is still running
			// Makes the Phrase.RecognitionSystem.Restart() fail
			// _dictationSubsystem.StopDictation();
			if (_dictationSubsystem.running) {
				Debug.Log("Forcing the dictation subsystem to stop");
				_dictationSubsystem.Destroy();
				Debug.Log("_dictationSubsystem.Destroy success");
				BootDictation();
			}
		}
		Debug.Log("PhraseRecognitionSystem.Restart");
		PhraseRecognitionSystem.Restart();
	}

	void OnApplicationFocus(bool hasFocus) {
		if (!hasFocus) {
			PhraseRecognitionSystem.Shutdown();
			_dictationSubsystem.StopDictation();
		} else {
			PhraseRecognitionSystem.Restart();
		}
	}

	[Serializable]
	public struct PhraseAction {
		[SerializeField] private string _phrase;

		[SerializeField] private UnityEvent _action;

		public string Phrase => _phrase;

		public UnityEvent Action => _action;
	}
}
