using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;
using MixedReality.Toolkit.Input;
using System.Collections.Generic;
using System.Collections;


public class Screenshot : MonoBehaviour {
	private PhotoCapture _photoCaptureObject = null;

	private Resolution _cameraResolution;

	private List<UnityEngine.Vector2> _gazeCoordinates = new List<UnityEngine.Vector2>();

	public GazeInteractor gazeInteractor;


	private void Start() {
		_cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

		PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject) {
			_photoCaptureObject = captureObject;
			Debug.Log("Photo capture initiliazed.");
		});
	}

	private void OnDestroy() {
		OnStoppedPhotoMode();
	}

	public void TakePhoto() {
		if (_photoCaptureObject == null) return;
		CameraParameters c = new CameraParameters();
		c.hologramOpacity = 0.0f;
		c.cameraResolutionWidth = _cameraResolution.width;
		c.cameraResolutionHeight = _cameraResolution.height;
		c.pixelFormat = CapturePixelFormat.BGRA32;

		_photoCaptureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
		StartGazeLogging();
	}

	private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result) {
		if (!result.success) {
			Debug.LogError("Unable to start photo mode");
			return;
		}

		// TODO - base64 available property
		string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
		string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

		_photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, delegate (PhotoCapture.PhotoCaptureResult result) {
			if (result.success) Debug.Log("Saved Photo to disk");
			else Debug.Log("Failed to save Photo to disk");
		});
	}

	private void OnStoppedPhotoMode() {
		if (_photoCaptureObject == null) return;
		_photoCaptureObject.Dispose();
		_photoCaptureObject = null;
	}

	public void StartGazeLogging() {
		StartCoroutine(LogGazeCoordinatesAfterDelay());
	}

	private IEnumerator LogGazeCoordinatesAfterDelay() {

		float elapsedTime = 0f;
		_gazeCoordinates.Clear();

		while (elapsedTime < 5.0f) { // TODO - This should be as long as the voice command

			float gazeDistance = 1.0f; // TODO - Calculate it using intersection
			UnityEngine.Vector3 gazePointWorld = gazeInteractor.rayOriginTransform.position
				+ gazeInteractor.rayOriginTransform.forward * gazeDistance;

			UnityEngine.Vector3 gazePointScreen = Camera.main.WorldToScreenPoint(gazePointWorld);
			UnityEngine.Vector2 gazePointScreen2D = new UnityEngine.Vector2(gazePointScreen.x, gazePointScreen.y);
			_gazeCoordinates.Add(gazePointScreen2D);

			yield return new WaitForSeconds( 0.1f);
			elapsedTime +=  0.1f;
		}

		Debug.Log("Gaze logging complete. Total points logged: " + _gazeCoordinates.Count);
	}
}