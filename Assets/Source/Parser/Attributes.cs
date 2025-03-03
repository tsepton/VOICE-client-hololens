using System;
using UnityEngine;

// Forked from https://github.com/tsepton/ummi
[AttributeUsage(validOn: AttributeTargets.Method)]
public class UserAction : PropertyAttribute {
	public string[] Utterances { get; }

	public UserAction(string utterance) {
		Utterances = new[] { utterance };
	}

	public UserAction(string[] utterances) {
		Utterances = utterances;
	}
}