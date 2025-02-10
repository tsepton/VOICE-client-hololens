using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {

	[SerializeField] private RectTransform trans;

	void Start() {
		trans = gameObject.GetComponent<RectTransform>();
	}

	void FixedUpdate() {
		trans.Rotate(new Vector3(0, 0, -180) * Time.deltaTime);
	}
}
