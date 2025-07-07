using UnityEngine;

public class Sauce : MonoBehaviour
{
    public SauceType type;
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void ChangeSauce()
    {
        gameManager.ChangeSauce(type);
    }
}
