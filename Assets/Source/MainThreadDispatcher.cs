using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour {
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    private static MainThreadDispatcher _instance = null;

    public static MainThreadDispatcher Instance() {
        if (!_instance) {
            _instance = FindObjectOfType<MainThreadDispatcher>();
            if (!_instance) {
                var obj = new GameObject("MainThreadDispatcher");
                _instance = obj.AddComponent<MainThreadDispatcher>();
            }
        }
        return _instance;
    }

    public void Enqueue(Action action) {
        lock (_executionQueue) {
            _executionQueue.Enqueue(action);
        }
    }

    private void Update() {
        lock (_executionQueue) {
            while (_executionQueue.Count > 0) {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}