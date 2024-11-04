using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;
using MixedReality.Toolkit.Input;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Assertions;


public class Gaze : MonoBehaviour {
	private PhotoCapture _photoCaptureObject = null;

	private Resolution _cameraResolution;

	private String _screenshotBase64 = null;

	private List<UnityEngine.Vector2> _gazeCoordinates = new List<UnityEngine.Vector2>();

	private Coroutine _gazeLoggingCoroutine;

	private GazeInteractor _gazeInteractor;

	public List<UnityEngine.Vector2> GazeCoordinates => _gazeCoordinates;

	public String ScreenshotBase64 => _screenshotBase64;


	private void Start() {
		_cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();

		PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject) {
			_photoCaptureObject = captureObject;
			Debug.Log("Photo capture initiliazed.");
		});
	}

	private void OnDestroy() {
		OnStoppedPhotoMode();
	}

	public void StartRecordingUserPov() {

		if (_photoCaptureObject == null) return;
		CameraParameters c = new CameraParameters();
		c.hologramOpacity = 0.0f;
		c.cameraResolutionWidth = _cameraResolution.width;
		c.cameraResolutionHeight = _cameraResolution.height;
		c.pixelFormat = CapturePixelFormat.BGRA32;

		_photoCaptureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
		StartGazeLogging();
	}

	public void StopRecordingUserPov() {
		StopGazeLogging();
		if (_photoCaptureObject != null) {
			_photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
		}
	}

	private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result) {
		if (!result.success) {
			Debug.LogError("Unable to start photo mode");
			return;
		}

		_screenshotBase64 = null;
		_photoCaptureObject.TakePhotoAsync(OnCapturedPhoto);
	}

	private void OnCapturedPhoto(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
		if (!result.success) {
			Debug.LogError("Failed to capture photo");
			return;
		}

		List<byte> imageData = new List<byte>();
		photoCaptureFrame.CopyRawImageDataIntoBuffer(imageData);

		Texture2D texture = new Texture2D(_cameraResolution.width, _cameraResolution.height, TextureFormat.BGRA32, false);
		texture.LoadRawTextureData(imageData.ToArray());
		texture.Apply();
		byte[] jpgBytes = texture.EncodeToJPG();
		Destroy(texture);
		string _screenshotBase64 = Convert.ToBase64String(jpgBytes);
		Debug.Log("Base64 Image: " + _screenshotBase64);
		
		// Release object for future use
		_photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
	}

	private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
		OnStoppedPhotoMode();
	}

	private void OnStoppedPhotoMode() {
		_photoCaptureObject?.Dispose();
		_photoCaptureObject = null;

		// Reinitialize photo capture
		PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject) {
			_photoCaptureObject = captureObject;
			Debug.Log("Photo capture reinitialized.");
		});
	}

	private void StartGazeLogging() {
		_gazeCoordinates.Clear();
		_gazeLoggingCoroutine = StartCoroutine(LogGazeCoordinates());
	}

	private void StopGazeLogging() {
		StopCoroutine(_gazeLoggingCoroutine);
		Debug.Log("Gaze logging complete. Total points logged: " + _gazeCoordinates.Count);
	}

	private IEnumerator LogGazeCoordinates() {

		float elapsedTime = 0f;
		Assert.IsTrue(_gazeCoordinates.Count == 0);

		while (true) {
			float gazeDistance = 1.0f; // TODO - Calculate it using intersection
			UnityEngine.Vector3 gazePointWorld = _gazeInteractor.rayOriginTransform.position
				+ _gazeInteractor.rayOriginTransform.forward * gazeDistance;

			UnityEngine.Vector3 gazePointScreen = Camera.main.WorldToScreenPoint(gazePointWorld);
			UnityEngine.Vector2 gazePointScreen2D = new UnityEngine.Vector2(gazePointScreen.x, gazePointScreen.y);
			_gazeCoordinates.Add(gazePointScreen2D);

			yield return new WaitForSeconds(0.1f);
			elapsedTime += 0.1f;
		}
	}
}