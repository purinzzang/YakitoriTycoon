using UnityEngine;


public class YakitoriBox : MonoBehaviour
{
    GameManager gameManager;
    public Yakitori yakitoriPrefab;

    void Start()
    {
        gameManager = GameManager.instance;    
    }

    public void SetYakitori()
    {
        gameManager.AddYakitori(yakitoriPrefab);
    }
}
