using UnityEngine;

public class SafeAreaReceiver : MonoBehaviour
{
    [System.Serializable]
    public class Insets
    {
        public float top;
        public float bottom;
    }

    public static SafeAreaReceiver instance;
    public Insets safeArea;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnReceiveSafeArea(string json)
    {
        Insets insets = JsonUtility.FromJson<Insets>(json);
        safeArea = insets;
    }
}
