using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void GameStart()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
