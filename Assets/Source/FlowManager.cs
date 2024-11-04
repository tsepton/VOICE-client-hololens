using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowManager : MonoBehaviour {

	[SerializeField] Speech speech;
	[SerializeField] Gaze gaze;

	void Start() {

		speech.OnDictationStart += gaze.StartRecordingUserPov;
		speech.OnDictationEnd += (string result) => {
			gaze.StopRecordingUserPov();
			// TODO - do something with the utterance and retrieve screenshot and gaze
		};

	}

	void Update() {

	}
}
