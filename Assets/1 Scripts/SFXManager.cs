using UnityEngine;
using System.Collections.Generic;

public enum SFXType
{
    Coin,
    Burn,
    Wrong,
    Fry
}


public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioClip buttonClip, coinClip, burnClip, wrongClip, fryClip;
    private AudioSource audioSource;

    private Dictionary<SFXType, AudioClip> sfxDict;

    private void Awake()
    {
        instance = this;

        sfxDict = new Dictionary<SFXType, AudioClip>
        {
            { SFXType.Coin, coinClip },
            { SFXType.Burn, burnClip },
            { SFXType.Wrong, wrongClip },
            { SFXType.Fry, fryClip }
        };
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDict.TryGetValue(type, out var clip) && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFXManager: 클립을 찾을 수 없음 {type}");
        }
    }

    public void PlayButton()
    {
        audioSource.PlayOneShot(buttonClip);
    }
}
