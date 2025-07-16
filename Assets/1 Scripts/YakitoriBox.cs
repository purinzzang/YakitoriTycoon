using UnityEngine;


public class YakitoriBox : MonoBehaviour
{
    GameManager gameManager;
    public int index;

    void Start()
    {
        gameManager = GameManager.instance;    
    }

    public void SetYakitori()
    {
        gameManager.AddYakitori(index);
    }
}
