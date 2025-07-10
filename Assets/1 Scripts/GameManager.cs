using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum SauceType
{
    None,
    Sweet,
    Hot
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    SFXManager sfxManager;

    public Transform[] grills;
    public Yakitori[] yakitoris;
    public List<Yakitori> yakitoriPool = new List<Yakitori>();

    public SpriteRenderer sweetSR, hotSR;
    public Sprite[] sweetSprites, hotSprites;
    public SauceType curSauce;

    public Customer customerPrefab;
    public List<Customer> customerList = new List<Customer>();
    YakitoriType[] yakitoriTypes;
    SauceType[] sauceTypes;

    public TextMeshProUGUI[] orderTexts;
    public TextMeshProUGUI moneyText, endText, reviewText;
    public Image timeBar;
    int maxOrder, money, tryYakitori, burnYakitori, wrongYakitori;
    int[] soldYakitori;
    public Animator endAnim;
    public TextAsset reviewData;
    Dictionary<int, string> reviewDict;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        sfxManager = GetComponentInChildren<SFXManager>();
        maxOrder = 1;
        tryYakitori = 0;
        soldYakitori = new int[9];
        burnYakitori = 0;
        yakitoris = new Yakitori[grills.Length];
        yakitoriTypes = (YakitoriType[])Enum.GetValues(typeof(YakitoriType));
        sauceTypes = (SauceType[])Enum.GetValues(typeof(SauceType));

        AddCustomer();
        StartCoroutine(Main());
    }

    IEnumerator Main()
    {
        float timer = 0;
        float maxTime = 90f;
        while (timer < maxTime)
        {
            yield return new WaitForSeconds(1f);
            timer++;
            timeBar.fillAmount = timer / maxTime;

            if(timer % 30 == 0)
            {
                Debug.Log("level up");
                maxOrder++;
            }
        }
        Debug.Log("end");
        CancelInvoke();
        Debug.Log("남은 손님 수 : " + customerList.Count);

        // 손님 전부 보내기
        for(int i = 0; i < customerList.Count; i++)
        {
            customerList[i].Bye();
        }
        customerList.Clear();

        // 게임 결과 정리
        int totalYakitori = 0;
        int bestYakitori = 0;
        int maxSold = 0;
        for(int i = 0; i < soldYakitori.Length; i++)
        {
            totalYakitori += soldYakitori[i];
            if(maxSold < soldYakitori[i])
            {
                maxSold = soldYakitori[i];
                bestYakitori = i;
            }
        }
        Debug.Log("total: " + totalYakitori + " / best: " + bestYakitori + " / burn: " + burnYakitori + " / try: " + tryYakitori + " / wrong: " + wrongYakitori);
        endText.text = totalYakitori + " 개\n" + money + " 원\n" + "100 %\n" + bestYakitori;
        int burnScore = ((float)burnYakitori / tryYakitori) < 0.1f ? 200 
            : ((float)burnYakitori / tryYakitori) < 0.3f ? 100
            : 0;
        int accuracyScore = ((float)wrongYakitori / (wrongYakitori + totalYakitori)) < 0.1f ? 20
            : ((float)wrongYakitori / (wrongYakitori + totalYakitori)) < 0.3f ? 10
            : 0;
        Debug.Log("burn ratio: " + ((float)burnYakitori / tryYakitori) + " / wrong ratio: " + ((float)wrongYakitori / (wrongYakitori + totalYakitori)));
        reviewText.text = GetReview(burnScore, accuracyScore, bestYakitori);

        // 매출표 애니메이션
        endAnim.SetTrigger("doEnd");
    }

    public void AddYakitori(Yakitori prefab)
    {
        // 그릴 빈자리 찾아 닭꼬치 배치
        for(int i = 0; i < yakitoris.Length; i++)
        {
            if (yakitoris[i] == null)
            {
                Yakitori newYakitori;
                if (yakitoriPool.Count > 0)
                {
                    newYakitori = yakitoriPool[0];
                    newYakitori.yakitoriType = prefab.yakitoriType;
                    newYakitori.InitYakitori(prefab.GetComponent<Animator>().runtimeAnimatorController);
                    newYakitori.gameObject.SetActive(true);
                    yakitoriPool.RemoveAt(0);
                }
                else
                {
                    newYakitori = Instantiate(prefab);
                }
                     
                yakitoris[i] = newYakitori;
                newYakitori.transform.position = grills[i].position;
                sfxManager.PlaySFX(SFXType.Fry);

                tryYakitori++;
                break;
            }
        }
    }
    
    public void ChangeSauce(SauceType type)
    {
        // 소스 선택
        if(curSauce != type)
        {
            curSauce = type;
            if(curSauce == SauceType.None)
            {
                sweetSR.sprite = sweetSprites[0];
                hotSR.sprite = hotSprites[0];
            }
            else if (curSauce == SauceType.Sweet)
            {
                sweetSR.sprite = sweetSprites[1];
                hotSR.sprite = hotSprites[0];
            }
            else if (curSauce == SauceType.Hot)
            {
                sweetSR.sprite = sweetSprites[0];
                hotSR.sprite = hotSprites[1];
            }
        }
    }

    public void DestroyYakitori(Yakitori yakitori)
    {
        yakitoris[Array.IndexOf(yakitoris, yakitori)] = null;
        yakitori.gameObject.SetActive(false);
        yakitoriPool.Add(yakitori);
    }

    void AddCustomer()
    {
        // 손님 생성
        Customer newCustomer = Instantiate(customerPrefab);

        // 랜덤 주문 생성
        for (int i = 0; i < maxOrder; i++)
        {
            Order newOrder = new Order();
            newOrder.yakitoriType = yakitoriTypes[Random.Range(0, yakitoriTypes.Length)];
            newOrder.sauceType = sauceTypes[Random.Range(0, sauceTypes.Length)];
            newOrder.amount = Random.Range(1, 4);

            if(newCustomer.AddOrder(newOrder))
            {
                string taste = newOrder.sauceType == SauceType.None ? ""
                     : newOrder.sauceType == SauceType.Sweet ? "달콤 "
                     : "매콤 ";
                string type = newOrder.yakitoriType == YakitoriType.Momo ? "닭꼬치 "
                     : newOrder.yakitoriType == YakitoriType.Negima ? "파닭꼬치"
                     : "염통꼬치 ";
                newOrder.name = taste + type;
                newOrder.text = orderTexts[i];
            }
        }

        newCustomer.transform.position += new Vector3(0.5f * customerList.Count, 0);
        customerList.Add(newCustomer);
        if(customerList.Count == 1)
        {
            UpdateOrderText(customerList[0].orderList);
        }
        Invoke("AddCustomer", 10f);
    }

    public void UpdateOrderText(List<Order> orderList)
    {
        for(int i = 0; i < orderTexts.Length; i++)
        {
            orderTexts[i].gameObject.SetActive(false);
        }

        for(int i = 0; i < orderList.Count; i++)
        {
            Order newOrder = orderList[i];
            newOrder.text.text = newOrder.name + " " + newOrder.amount + "개";
            newOrder.text.gameObject.SetActive(true);
        }
    }

    public void ByeCustomer()
    {
        Debug.Log("손님 나가요");
        customerList[0].Bye();
        customerList.RemoveAt(0);
        for(int i = 0; i <  customerList.Count; i++)
        {
            customerList[i].transform.position -= new Vector3(0.5f, 0);
        }
        if (customerList.Count > 0)
        {
            UpdateOrderText(customerList[0].orderList);
        }
    }
    

    public void GiveYakitori(Yakitori yakitori)
    {
        // 첫번째 손님 주문 목록 비교
        List<Order> curOrderList = customerList[0].orderList;
        for(int i = 0; i < curOrderList.Count; i++)
        {
            // 주문 발견시 제출
            if (curOrderList[i].yakitoriType == yakitori.yakitoriType
                && curOrderList[i].sauceType == yakitori.sauceType)
            {
                curOrderList[i].amount--;
                curOrderList[i].text.text = curOrderList[i].name + " " + curOrderList[i].amount + "개";

                // 계산과 판매량 관리
                var (index, price) = GetYakitoriInfo(yakitori.yakitoriType, yakitori.sauceType);
                money += price;
                moneyText.text = "매출 : " + money + " 원";

                soldYakitori[index]++;
                sfxManager.PlaySFX(SFXType.Coin);

                // 잔여 개수 0개가 되면 주문 클리어
                if (curOrderList[i].amount <= 0)
                {
                    curOrderList[i].text.text = "";
                    curOrderList[i].text.gameObject.SetActive(false);
                    customerList[0].orderList.RemoveAt(i);

                    // 주문 리스트 0개가 되면 손님 클리어
                    if (customerList[0].orderList.Count <= 0)
                    {
                        ByeCustomer();
                    }
                }
                DestroyYakitori(yakitori);
                return;
            }
        }

        sfxManager.PlaySFX(SFXType.Wrong);
        WrongYakitori();
    }

    private (int index, int price) GetYakitoriInfo(YakitoriType type, SauceType sauce)
    {
        int index = -1;
        int price = 0;

        if (type == YakitoriType.Hatsu)
        {
            index = (sauce == SauceType.Sweet) ? 2 :
                    (sauce == SauceType.Hot) ? 5 : 8;
            price = 3000;
        }
        else if (type == YakitoriType.Momo)
        {
            index = (sauce == SauceType.Sweet) ? 0 :
                    (sauce == SauceType.Hot) ? 3 : 6;
            price = 4000;
        }
        else if (type == YakitoriType.Negima)
        {
            index = (sauce == SauceType.Sweet) ? 1 :
                    (sauce == SauceType.Hot) ? 4 : 7;
            price = 4000;
        }

        if (sauce != SauceType.None)
            price += 500;

        return (index, price);
    }


    public void BurnYakitori()
    {
        burnYakitori++;
    }

    public void WrongYakitori()
    {
        wrongYakitori++;
    }

    public string GetReview(int burnScore, int accuracyScore, int menuIndex)
    {
        reviewDict = JsonConvert.DeserializeObject<Dictionary<int, string>>(reviewData.text);
        int key = burnScore + accuracyScore + menuIndex;
        return reviewDict.TryGetValue(key, out var review) ? review : "리뷰 없음";
    }
}
