using UnityEngine;
using UnityEngine.UI;

public class CameraScaler : MonoBehaviour
{
    public CanvasScaler canvas;

    void Start()
    {
        SetCameraSize();
    }

    void SetCameraSize()
    {
        float ratio = (float)Screen.height / Screen.width;

        float minRatio = 1.777f; // 720x1280
        float maxRatio = 2.222f; // 1440x3200
        if (ratio < minRatio)
        {
            canvas.matchWidthOrHeight = 1;
        }

        ratio = Mathf.Clamp(ratio, minRatio, maxRatio);

        float cameraSize = Mathf.Lerp(4f, 5f, (ratio - minRatio) / (maxRatio - minRatio));
        Camera.main.orthographicSize = cameraSize;


    }
}
