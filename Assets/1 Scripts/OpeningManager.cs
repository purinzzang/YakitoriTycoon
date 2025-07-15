using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCameraSize();
    }

    void SetCameraSize()
    {
        float ratio = (float)Screen.height / Screen.width;

        float minRatio = 1.777f; // 720x1280
        float maxRatio = 2.222f; // 1440x3200

        ratio = Mathf.Clamp(ratio, minRatio, maxRatio);

        float cameraSize = Mathf.Lerp(4f, 5f, (ratio - minRatio) / (maxRatio - minRatio));
        Camera.main.orthographicSize = cameraSize;
    }

    public void GameStart()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
