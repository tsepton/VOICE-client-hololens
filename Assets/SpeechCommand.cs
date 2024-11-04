using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Events;

public class SpeechCommand : MonoBehaviour
{
  [SerializeField] private List<PhraseAction> _phraseActions;

  private void Start()
  {
    var phraseRecognitionSubsystem = XRSubsystemHelpers.KeywordRecognitionSubsystem;
    foreach (var phraseAction in _phraseActions)
    {
      if (!string.IsNullOrEmpty(phraseAction.Phrase) &&
        phraseAction.Action.GetPersistentEventCount() > 0)
      {
        phraseRecognitionSubsystem.
          CreateOrGetEventForKeyword(phraseAction.Phrase).
            AddListener(() => phraseAction.Action.Invoke());
      }
    }
  }

  [Serializable]
  public struct PhraseAction
  {
    [SerializeField] private string _phrase;

    [SerializeField] private UnityEvent _action;

    public string Phrase => _phrase;

    public UnityEvent Action => _action;
  }
}

