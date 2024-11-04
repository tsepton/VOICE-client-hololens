using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;


public class Screenshot : MonoBehaviour
{
    private static ILogger _logger = Debug.unityLogger;
    private static readonly string _tag = "Screenshot.cs";

    private PhotoCapture _photoCaptureObject = null;

    private Resolution _cameraResolution;

    private void Start()
    {
        _cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject)
        {
            _photoCaptureObject = captureObject;
            _logger.Log(_tag, "Photo capture initiliazed.");
        });
    }

    private void OnDestroy()
    {
        OnStoppedPhotoMode();
    }

    public void TakePhoto()
    {
        if (_photoCaptureObject == null) return;
        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = _cameraResolution.width;
        c.cameraResolutionHeight = _cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        _photoCaptureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (!result.success)
        {
            _logger.LogError(_tag, "Unable to start photo mode");
            return;
        }

        // TODO - base64 available property
        string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

        _photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, delegate (PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success) _logger.Log(_tag, "Saved Photo to disk");
            else _logger.Log(_tag, "Failed to save Photo to disk");
        });
    }

    private void OnStoppedPhotoMode()
    {
        if (_photoCaptureObject == null) return;
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
    }
}