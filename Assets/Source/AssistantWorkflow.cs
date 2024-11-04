using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssistantWorkflow : MonoBehaviour {

	private FileLogger vassCustomFileLogger;

	void Start() {
		vassCustomFileLogger = new FileLogger();
	}
}
