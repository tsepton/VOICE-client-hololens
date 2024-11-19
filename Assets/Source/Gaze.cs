using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;
using MixedReality.Toolkit.Input;
using System.Collections.Generic;
using System.Collections;
using System;


public class Gaze : MonoBehaviour {
	[SerializeField] private GazeInteractor _gazeInteractor;

	private PhotoCapture _photoCaptureObject = null;

	private CameraParameters _cameraParameters;

	private string _screenshotBase64 = null;

	private List<Vector2> _gazeCoordinates = new List<Vector2>();

	private Coroutine _gazeLoggingCoroutine;

	public List<Vector2> GazeCoordinates => _gazeCoordinates;

	public string ScreenshotBase64 => _screenshotBase64;


	private void Start() {
		_cameraParameters = new CameraParameters();
		Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();
		_cameraParameters.hologramOpacity = 0.0f;
		_cameraParameters.cameraResolutionWidth = cameraResolution.width;
		_cameraParameters.cameraResolutionHeight = cameraResolution.height;
		_cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

		PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject) {
			_photoCaptureObject = captureObject;
			Debug.Log("Photo capture initiliazed.");
			_photoCaptureObject.StartPhotoModeAsync(_cameraParameters, OnPhotoModeStarted);
		});
	}

	private void OnDestroy() {
		_photoCaptureObject.StopPhotoModeAsync((_) => {
			_photoCaptureObject?.Dispose();
			_photoCaptureObject = null;
		});
	}

	public void StartRecordingUserPov() {

		// Taking a picture 
		if (_photoCaptureObject == null) {
			Debug.LogError("Photo mode not started");
			return;
		};
		_screenshotBase64 = null;
		_photoCaptureObject.TakePhotoAsync(OnCapturedPhoto);

		// Recording gaze data
		_gazeCoordinates.Clear();
		_gazeLoggingCoroutine = StartCoroutine(LogGazeCoordinates());
	}

	public (List<Vector2>, string) StopRecordingUserPov() {
		if (_gazeLoggingCoroutine != null) StopCoroutine(_gazeLoggingCoroutine);
		// FIXME - We're making the assumption that the screenshot is ready... 
		return (_gazeCoordinates, _screenshotBase64);
	}

	private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result) {
		if (!result.success) Debug.LogError("Unable to start photo mode");
		else Debug.Log("Photo mode started successfully");
	}

	private void OnCapturedPhoto(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
		if (!result.success) {
			Debug.LogError("Failed to capture photo");
			return;
		}

		List<byte> imageData = new List<byte>();
		photoCaptureFrame.CopyRawImageDataIntoBuffer(imageData);

		Texture2D texture = new Texture2D(_cameraParameters.cameraResolutionWidth, _cameraParameters.cameraResolutionHeight, TextureFormat.BGRA32, false);
		texture.LoadRawTextureData(imageData.ToArray());
		texture.Apply();


		Texture2D flippedTexture = new Texture2D(texture.width, texture.height);
		for (int y = 0; y < texture.height; y++) {
			flippedTexture.SetPixels(0, texture.height - y - 1, texture.width, 1, texture.GetPixels(0, y, texture.width, 1));
		}
		flippedTexture.Apply();
		byte[] jpgBytes = flippedTexture.EncodeToJPG();
		Destroy(texture);
		Destroy(flippedTexture);

		_screenshotBase64 = Convert.ToBase64String(jpgBytes);
		Debug.Log("Captured photo successfully.");
	}

	private IEnumerator LogGazeCoordinates() {
		if (_gazeInteractor == null) Debug.LogError("No gaze interactor given.");

		float elapsedTime = 0f;

		var width = _cameraParameters.cameraResolutionWidth;
		var height = _cameraParameters.cameraResolutionHeight;

		while (true) {
			float gazeDistance = 0.75f; // TODO - Calculate it using intersection
			Vector3 gazePointWorld = _gazeInteractor.rayOriginTransform.position
				+ _gazeInteractor.rayOriginTransform.forward * gazeDistance;

			Vector3 gazePointScreen = Camera.main.WorldToViewportPoint(gazePointWorld);
			Vector2 gazePointScreen2D = new Vector2(gazePointScreen.x * width, gazePointScreen.y * height);
			_gazeCoordinates.Add(gazePointScreen2D);

			yield return new WaitForSeconds(0.05f);
			elapsedTime += 0.05f;
		}
	}
}