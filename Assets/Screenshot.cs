using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;


public class Screenshot : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;

    Resolution cameraResolution;

    void Start()
    {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;
            Debug.Log("Photo capture initiliazed.");
        });
    }

    public void TakePhoto()
    {
        Debug.Log("Taking a photo.");
        Debug.Log(cameraResolution);
        Debug.Log(photoCaptureObject);

        if (photoCaptureObject == null) return;
        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        photoCaptureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}