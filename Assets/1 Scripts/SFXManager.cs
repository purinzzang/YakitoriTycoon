using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;
    public AudioClip coinClip, burnClip, wrongClip;
    AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCoin()
    {
        audioSource.PlayOneShot(coinClip);
    }

    public void PlayBurn()
    {
        audioSource.PlayOneShot(burnClip);
    }

    public void PlayWrong()
    {
        audioSource.PlayOneShot(wrongClip);
    }
}
