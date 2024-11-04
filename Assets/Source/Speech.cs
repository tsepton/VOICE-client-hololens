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

	private void Start() {
		
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
		_dictationSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<DictationSubsystem>();
		if (_dictationSubsystem != null) {
			_dictationSubsystem.Recognized += OnRecognized;
			_dictationSubsystem.RecognitionFinished += OnRecognitionFinished;
		}
	}

	private void OnRecognized(DictationResultEventArgs arg) {
		Debug.Log(arg.Result);
		Debug.Log(arg.Confidence);
	}

	private void OnRecognitionFinished(DictationSessionEventArgs arg) {
		if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Stopped ||
			PhraseRecognitionSystem.Status == SpeechSystemStatus.Failed) {
			PhraseRecognitionSystem.Restart();
		}
	}

	public void StartDictation() {
		if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running) {
			PhraseRecognitionSystem.Shutdown();
		}
		_dictationSubsystem.StartDictation();
	}

	[Serializable]
	public struct PhraseAction {
		[SerializeField] private string _phrase;

		[SerializeField] private UnityEvent _action;

		public string Phrase => _phrase;

		public UnityEvent Action => _action;
	}
}

