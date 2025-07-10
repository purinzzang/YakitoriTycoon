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

    Animator anim;

    private void Start()
    {
        gameManager = GameManager.instance;
        anim = GetComponent<Animator>();
  
    }

    private void OnEnable()
    {
        StartCoroutine(CookCo());
    }

    public void InitYakitori(RuntimeAnimatorController ac)
    {
        state = YakitoriState.Raw;
        sauceType = SauceType.None;
        anim.runtimeAnimatorController = ac;
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
                    anim.SetTrigger("doSweet");
                }
                else if (gameManager.curSauce == SauceType.Hot)
                {
                    sauceType = SauceType.Hot;
                    anim.SetTrigger("doHot");
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
        anim.SetTrigger("doCooked");
        state = YakitoriState.Cooked;

        yield return new WaitForSeconds(12f);
        anim.SetTrigger("doAlert");

        yield return new WaitForSeconds(3f);
        SFXManager.instance.PlaySFX(SFXType.Burn);
        anim.SetTrigger("doOvercooked");
        state = YakitoriState.OverCooked;
        gameManager.BurnYakitori();
    }
}
