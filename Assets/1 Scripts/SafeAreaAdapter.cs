using UnityEngine;
using UnityEngine.UI;

public class SafeAreaAdapter : MonoBehaviour
{
    enum Anchor { Top, Center, Bottom }
    [SerializeField] Anchor anchor;
    [SerializeField] CanvasScaler canvas;

    void Start()
    {
        Debug.Log("screen: " + Screen.width + ", " + Screen.height);
        Debug.Log("safe area: " + Screen.safeArea);
        RectTransform rt = GetComponent<RectTransform>();
        switch (anchor)
        {
            case Anchor.Top:
                rt.anchoredPosition -= new Vector2(0, (canvas.referenceResolution.x / Screen.width) * (Screen.height - Screen.safeArea.y - Screen.safeArea.height));
                break;
            case Anchor.Center:
                rt.anchoredPosition = new Vector2(0, Mathf.Clamp((canvas.referenceResolution.x / Screen.width) * (Screen.safeArea.height * -0.5f), -780f, -640f));
                break;
        }
        
    }
}
