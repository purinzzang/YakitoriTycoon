using System.Collections;
using UnityEngine;

public enum YakitoriState
{
    Raw,
    Cooked,
    OverCooked
}

public class Yakitori : MonoBehaviour
{
    GameManager gameManager;
    SFXManager sfxManager;

    public YakitoriState state;
    public SauceType sauceType;
    public int index;

    Animator anim;

    private void Start()
    {
        gameManager = GameManager.instance;
        sfxManager = SFXManager.instance;
        anim = GetComponent<Animator>();
  
    }

    private void OnEnable()
    {
        StartCoroutine(CookCo());
    }

    public void InitYakitori(RuntimeAnimatorController ac, int index)
    {
        state = YakitoriState.Raw;
        sauceType = SauceType.None;
        anim.runtimeAnimatorController = ac;
        this.index = index;
        transform.localScale = Vector3.one;
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
                    index += 3;
                }
                else if (gameManager.curSauce == SauceType.Hot)
                {
                    sauceType = SauceType.Hot;
                    anim.SetTrigger("doHot");
                    index += 6;
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
            gameManager.ReturnYakitori(this);
            gameObject.SetActive(false);
        }
    }

    IEnumerator CookCo()
    {
        yield return new WaitForSeconds(5f);
        anim.SetTrigger("doCooked");
        state = YakitoriState.Cooked;

        yield return new WaitForSeconds(12f);
  
        float elapsed = 0f;

        while (elapsed < 3f)
        {
            float pulse = Mathf.Sin(elapsed * Mathf.PI * 2f); // 0 → 1 → 0 → -1 → 0 (주기 1초)
            float t = (pulse + 1f) / 2f; // 0 → 1 → 0 → 0 → 1 (0~1로 변환)
            float scaleValue = Mathf.Lerp(1f, 1.1f, t);
            transform.localScale = Vector3.one * scaleValue;

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;

        anim.SetTrigger("doOvercooked");
        state = YakitoriState.OverCooked;
        gameManager.BurnYakitori();
        sfxManager.PlaySFX(SFXType.Burn);
    }

    public void MoveToCustomer(Vector3 target)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToCustomerCo(target));
    }

    IEnumerator MoveToCustomerCo(Vector3 to)
    {
        Vector3 from = transform.position;
        float elapsed = 0f;
        float duration = 0.5f;

        transform.localScale = Vector3.one;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(from, to, t);

            // 스케일 변화 계산: 0~0.5 구간에서 커졌다가, 0.5~1.0 구간에서 작아짐
            float scaleT = t < 0.5f
                ? Mathf.Lerp(1f, 1.2f, t * 2f)    // 0~0.5 구간: 1 → 1.1
                : Mathf.Lerp(1.2f, 0f, (t - 0.5f) * 2f); // 0.5~1 구간: 1.1 → 0

            transform.localScale = Vector3.one * scaleT;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
        gameManager.ReturnYakitori(this);
    }

}
