using UnityEngine;
using UnityEngine.UI;

public class SafeAreaAdapter : MonoBehaviour
{
    enum Anchor { Top, Center, Bottom }
    [SerializeField] Anchor anchor;
    [SerializeField] CanvasScaler canvas;

    void Start()
    {
        float top, bottom;
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log("screen: " + Screen.width + ", " + Screen.height);
        Debug.Log("safe area: " + Screen.safeArea);
        top = (canvas.referenceResolution.x / Screen.width) * (Screen.height - Screen.safeArea.y - Screen.safeArea.height);
        bottom = Screen.safeArea.y;
        //RectTransform rt = GetComponent<RectTransform>();
        //switch (anchor)
        //{
        //    case Anchor.Top:
        //        rt.anchoredPosition -= new Vector2(0, (canvas.referenceResolution.x / Screen.width) * (Screen.height - Screen.safeArea.y - Screen.safeArea.height));
        //        break;
        //    case Anchor.Center:
        //        rt.anchoredPosition = new Vector2(0, Mathf.Clamp((canvas.referenceResolution.x / Screen.width) * (Screen.safeArea.height * -0.5f), -780f, -640f));
        //        break;
        //}
#else

        top = SafeAreaReceiver.instance.safeArea.top;
        bottom = SafeAreaReceiver.instance.safeArea.bottom;

#endif

        RectTransform rt = GetComponent<RectTransform>();
        switch (anchor)
        {
            case Anchor.Top:
                rt.anchoredPosition -= new Vector2(0, top);
                break;
            case Anchor.Center:
                float y = Mathf.Clamp((canvas.referenceResolution.x / Screen.width) * (Screen.height * -0.5f) + bottom, -780f, -640f);
                rt.anchoredPosition = new Vector2(0, y);
                break;
        }

    }
}
