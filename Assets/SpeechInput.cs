using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Events;

public class SpeechInput : MonoBehaviour
{
  [SerializeField] private List<PhraseAction> phraseActions;

  private void Start()
  {
    var phraseRecognitionSubsystem = XRSubsystemHelpers.KeywordRecognitionSubsystem;
    foreach (var phraseAction in phraseActions)
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
    [SerializeField] private string phrase;

    [SerializeField] private UnityEvent action;

    public string Phrase => phrase;

    public UnityEvent Action => action;
  }
}

