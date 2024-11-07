using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {

	[SerializeField] private RectTransform trans;
	
	private Quaternion initialRotation;

	void Start() {
		trans = gameObject.GetComponent<RectTransform>();
		initialRotation = trans.rotation;
	}

	void FixedUpdate() {
		trans.Rotate(new Vector3(0,0,-180) * Time.deltaTime);
	}	
	
	void OnDisable() 
	{
		trans.rotation = initialRotation;
	}
}
