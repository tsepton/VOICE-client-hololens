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
			_dictationSubsystem.RecognitionFaulted += OnRecognitionFinished;
		}
	}

	private void OnRecognized(DictationResultEventArgs arg) {
		// TODO
		Debug.Log($"Recognized: ''{arg.Result}'' with a confidence of {arg.Confidence}");
		_dictationSubsystem.StopDictation();
		OnDictationEnd?.Invoke(arg.Result);
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
		OnDictationStart?.Invoke();
	}

	[Serializable]
	public struct PhraseAction {
		[SerializeField] private string _phrase;

		[SerializeField] private UnityEvent _action;

		public string Phrase => _phrase;

		public UnityEvent Action => _action;
	}
}

