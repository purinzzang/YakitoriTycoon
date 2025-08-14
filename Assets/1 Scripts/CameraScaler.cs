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
        float cameraSize = 2.247f * Mathf.Max(ratio, minRatio) + 0.01f;
        Camera.main.orthographicSize = cameraSize;
        canvas.matchWidthOrHeight = ratio < minRatio ? 1 : 0;
        return;
        
        float maxRatio = 2.222f; // 1440x3200
        if (ratio < minRatio)
        {
            canvas.matchWidthOrHeight = 1;
        }

        ratio = Mathf.Clamp(ratio, minRatio, maxRatio);

        cameraSize = Mathf.Lerp(4f, 5f, (ratio - minRatio) / (maxRatio - minRatio));
        Camera.main.orthographicSize = cameraSize;


    }
}
