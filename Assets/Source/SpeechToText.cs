using System;
using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;

public class SpeechToText : MonoBehaviour
{
    private DictationSubsystem _dictationSubsystem;

    private static ILogger _logger = Debug.unityLogger;

    private static readonly string _tag = "SpeechToText.cs";


    private void Start()
    {
        _dictationSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<DictationSubsystem>();

        if (_dictationSubsystem != null)
        {
            // dictationSubsystem.Recognizing += OnRecognizing;
            _dictationSubsystem.Recognized += OnRecognized;
            // dictationSubsystem.RecognitionFinished += OnRecognitionFinished;
            // dictationSubsystem.RecognitionFaulted += OnRecognitionFaulted;

            // And start dictation
            // dictationSubsystem.StartDictation();
        }
    }


    // private void OnRecognizing(DictationResultEventArgs arg) { }

    private void OnRecognized(DictationResultEventArgs arg)
    {
        _logger.Log(_tag, arg.Result);
        _logger.Log(_tag, arg.Confidence);
    }

    // private void OnRecognitionFinished(DictationSessionEventArgs arg) { }

    // private void OnRecognitionFaulted(DictationSessionEventArgs arg) { }

    public void StartListening()
    {
        _dictationSubsystem.StartDictation();
    }

}
