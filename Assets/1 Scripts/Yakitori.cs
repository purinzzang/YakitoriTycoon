using System.Collections;
using UnityEngine;

public enum YakitoriState
{
    Raw,
    Cooked,
    OverCooked
}
public enum YakitoriType
{
    Momo,
    Negima,
    Hatsu
}


public class Yakitori : MonoBehaviour
{
    GameManager gameManager;
    public YakitoriType yakitoriType;
    public YakitoriState state;
    public SauceType sauceType;

    public Sprite cookedSprite, sweetSprite, hotSprite;
    SpriteRenderer sr;

    private void Start()
    {
        gameManager = GameManager.instance;
        sr = GetComponent<SpriteRenderer>();
  
    }

    private void OnEnable()
    {
        StartCoroutine(CookCo());
    }

    public void InitYakitori(Sprite sprite)
    {
        state = YakitoriState.Raw;
        sauceType = SauceType.None;
        if(sr != null)
        {
            sr.color = Color.white;
            sr.sprite = sprite;
        }
            

    }

    public void TouchYakitori()
    {
        if(state == YakitoriState.Cooked)
        {
            if(sauceType == SauceType.None)
            {
                if (gameManager.curSauce == SauceType.None)
                {
                    // 손님에게 제출
                    gameManager.GiveYakitori(this);
                }
                else if (gameManager.curSauce == SauceType.Sweet)
                {
                    sauceType = SauceType.Sweet;
                    sr.sprite = sweetSprite;
                }
                else if (gameManager.curSauce == SauceType.Hot)
                {
                    sauceType = SauceType.Hot;
                    sr.sprite = hotSprite;
                }
            }
            else
            {
                // 손님에게 제출
                gameManager.GiveYakitori(this);
            }
        }
        else if (state == YakitoriState.OverCooked)
        {
            // 버리기
            gameManager.DestroyYakitori(this);
        }
    }

    IEnumerator CookCo()
    {
        yield return new WaitForSeconds(5f);
        sr.sprite = cookedSprite;
        state = YakitoriState.Cooked;
        yield return new WaitForSeconds(10f);
        sr.color = new Color(0.3f, 0.3f, 0.3f);
        state = YakitoriState.OverCooked;
    }
}
