using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
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
    // 매니저
    public static GameManager instance;
    SFXManager sfxManager;

    // 닭꼬치
    public Transform[] grills;
    Yakitori[] yakitoris;
    List<Yakitori> yakitoriPool = new List<Yakitori>();
    public YakitoriDatabase yakitoriDatabase;

    // 소스
    public SpriteRenderer sweetSR, hotSR;
    public Sprite[] sweetSprites, hotSprites;
    public SauceType curSauce;

    // 손님
    public Customer customerPrefab;
    public List<Customer> customerList = new List<Customer>();
    public List<Customer> customerPool = new List<Customer>();
    public Vector3 firstLine;
    public SpriteRenderer playerSR;
    public Sprite[] playerSprites;

    // hud
    public TextMeshProUGUI[] orderTexts;
    public TextMeshProUGUI moneyText, endText, reviewText;
    public Image timeBar;
    public GameObject countDown, curOutline;

    // 데이터
    int maxOrder, money, tryYakitori, burnYakitori, wrongYakitori;
    int[] soldYakitori;
    public TextAsset reviewData;
    Dictionary<int, string> reviewDict;

    // 애니메이션
    public Animator adAnim, countAnim, endAnim, bgAnim;

    // 캐릭터 선택
    bool isMan;
    public GameObject[] characters;
    public float grillTime, burnTime;

    // for test
    public int spawnDelay, minSpawnDelay, maxSpawnDelay;
    public bool isRandomSpawn;

    public void RandomSpawnToggle(bool b)
    {
        isRandomSpawn = b;
    }

    public void ChangeSpawnDelay(string v)
    {
        if (v != "")
        {
            spawnDelay = int.Parse(v);
        }
    }

    public void ChangeMinSpawnDelay(string v)
    {
        if (v != "")
        {
            minSpawnDelay = int.Parse(v);
        }
    }

    public void ChangeMaxSpawnDelay(string v)
    {
        if (v != "")
        {
            maxSpawnDelay = int.Parse(v);
        }
    }

    private void Awake()
    {
        Time.timeScale = 1;
        instance = this;
    }


    private void Start()
    {
        sfxManager = GetComponentInChildren<SFXManager>();

        isMan = true;
        maxOrder = 1;
        tryYakitori = 0;
        soldYakitori = new int[9];
        burnYakitori = 0;
        yakitoris = new Yakitori[grills.Length];
        firstLine = customerPrefab.transform.position;

        // for test
        spawnDelay = 7;
        minSpawnDelay = 5;
        maxSpawnDelay = 7;
    }

    public void ChangeCharacter()
    {
        isMan = !isMan;
        characters[0].SetActive(isMan);
        characters[1].SetActive(!isMan);
    }

    public void GameStart()
    {
        playerSR.sprite = playerSprites[isMan ? 0 : 1];
        grillTime = isMan ? 4f : 5f;
        burnTime = isMan ? 3f : 5f;
        GetComponent<AudioSource>().Play();
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

            if(timer % 30 == 0 && maxOrder < 3)
            {
                Debug.Log("level up");
                maxOrder++;
                bgAnim.SetTrigger("doTimePass");
                if (maxOrder == 3)
                {
                    bgAnim.transform.GetChild(0).gameObject.SetActive(false);
                    bgAnim.transform.GetChild(1).gameObject.SetActive(false);
                    bgAnim.transform.GetChild(2).gameObject.SetActive(true);
                }
            }
        }
        Debug.Log("end");

        adAnim.SetTrigger("doShow");
        Time.timeScale = 0;
    }

    public void WatchAdd()
    {
        //countAnim.SetTrigger("doCount");
        StartCoroutine(ExtraTimeCo());
        //CancelInvoke();
    }

    public void ExtraTime()
    {
        StartCoroutine(ExtraTimeCo());
    }

    IEnumerator ExtraTimeCo()
    {
        countDown.SetActive(true);
        TextMeshProUGUI cText = countDown.GetComponentInChildren<TextMeshProUGUI>();
        if (cText == null)
            Debug.Log("null");
        cText.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        cText.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        cText.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        countDown.SetActive(false);
        Time.timeScale = 1;

        float timer = 60f;
        float maxTime = 90f;
        timeBar.fillAmount = timer / maxTime;
        while (timer < maxTime)
        {
            yield return new WaitForSeconds(1f);
            timer++;
            timeBar.fillAmount = timer / maxTime;

        }
        Debug.Log("end");
        GameEnd();
    }

    public void GameEnd()
    {
        Time.timeScale = 1;

        CancelInvoke();

        // 손님 전부 보내기
        for (int i = 0; i < customerList.Count; i++)
        {
            customerList[i].Bye();
        }
        customerList.Clear();

        // 게임 결과 정리
        int totalYakitori = 0;
        int bestYakitori = 0;
        int maxSold = 0;
        for (int i = 0; i < soldYakitori.Length; i++)
        {
            totalYakitori += soldYakitori[i];
            if (maxSold < soldYakitori[i])
            {
                maxSold = soldYakitori[i];
                bestYakitori = i;
            }
        }
        //Debug.Log("total: " + totalYakitori + " / best: " + bestYakitori + " / burn: " + burnYakitori + " / try: " + tryYakitori + " / wrong: " + wrongYakitori);
        float wrongRatio = ((float)wrongYakitori / (wrongYakitori + totalYakitori));
        float burnRatio = ((float)burnYakitori / tryYakitori);
        endText.text = totalYakitori + " 개\n" 
            + money.ToString("N0") + " 원\n" 
            + (totalYakitori + wrongYakitori > 0 ? (int)((1f - wrongRatio) * 100f) : "-") + " %\n\n" 
            + (totalYakitori > 0 ? yakitoriDatabase.GetYakitori(bestYakitori).displayName : "-");

        if(totalYakitori > 0)
        {
            int burnScore = burnRatio < 0.1f ? 200
                : burnRatio < 0.3f ? 100
                : 0;
            int accuracyScore = wrongRatio < 0.1f ? 20
                : wrongRatio < 0.3f ? 10
                : 0;
            //Debug.Log("burn ratio: " + burnRatio + " / wrong ratio: " + wrongRatio);
            reviewText.text = GetReview(burnScore, accuracyScore, bestYakitori);
        }


        // 매출표 애니메이션
        endAnim.SetTrigger("doEnd");
    }

    // 닭꼬치
    public void AddYakitori(int index)
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
                    newYakitori.InitYakitori(yakitoriDatabase.GetYakitori(index).ac, index);

                    yakitoriPool.RemoveAt(0);
                }
                else
                {
                    newYakitori = Instantiate(yakitoriDatabase.GetYakitori(index).prefab);
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
                sweetSR.transform.localScale = Vector3.one;
                hotSR.sprite = hotSprites[0];
                hotSR.transform.localScale = Vector3.one;
            }
            else if (curSauce == SauceType.Sweet)
            {
                sweetSR.sprite = sweetSprites[1];
                sweetSR.transform.localScale = Vector3.one * 1.2f;
                hotSR.sprite = hotSprites[0];
                hotSR.transform.localScale = Vector3.one;
                sfxManager.PlaySFX(SFXType.Sauce);
            }
            else if (curSauce == SauceType.Hot)
            {
                sweetSR.sprite = sweetSprites[0];
                sweetSR.transform.localScale = Vector3.one;
                hotSR.sprite = hotSprites[1];
                hotSR.transform.localScale = Vector3.one * 1.2f;
                sfxManager.PlaySFX(SFXType.Sauce);
            }
        }
    }

    public void ReturnYakitori(Yakitori yakitori)
    {
        yakitoris[Array.IndexOf(yakitoris, yakitori)] = null;
        yakitoriPool.Add(yakitori);
    }

    // 손님
    void AddCustomer()
    {
        // 손님 생성
        Customer newCustomer;
        if (customerPool.Count > 0)
        {
            newCustomer = customerPool[0];
            customerPool.RemoveAt(0);
            newCustomer.gameObject.SetActive(true);
        }
        else
        {
            newCustomer = Instantiate(customerPrefab);
        }

        // 랜덤 주문 생성
        for (int i = 0; i < maxOrder; i++)
        {
            Debug.Log(i);
            Order newOrder = new Order();
            int ran = Random.Range(0, yakitoriDatabase.yakitoriList.Length);
            newOrder.amount = Random.Range(1, 4);
            newOrder.index = ran;
            newOrder.text = orderTexts[i];
            newCustomer.AddOrder(newOrder);
        }

        newCustomer.transform.position += new Vector3(0.5f * customerList.Count, 0);
        customerList.Add(newCustomer);
        if(customerList.Count == 1)
        {
            UpdateOrderText(customerList[0].orderList, customerList[0].transform.GetChild(0).gameObject);
        }
        float delay = isRandomSpawn ? Random.Range((float)minSpawnDelay, (float)maxSpawnDelay) : spawnDelay;
        Debug.Log(delay);
        Invoke("AddCustomer", delay);
    }

    public void UpdateOrderText(List<Order> orderList, GameObject outline)
    {
        // 주문 텍스트 초기화
        for(int i = 0; i < orderTexts.Length; i++)
        {
            orderTexts[i].gameObject.SetActive(false);
        }

        // 주문 목록의 항목 표시
        for(int i = 0; i < orderList.Count; i++)
        {
            Order order = orderList[i];
            order.text.text = yakitoriDatabase.GetYakitori(order.index).displayName + " " + order.amount + "개";
            order.text.gameObject.SetActive(true);
        }

        if (curOutline != null)
            curOutline.SetActive(false);
        curOutline = outline;
        curOutline.SetActive(true);
    }

    public void ByeCustomer()
    {
        if (customerList.Count == 0)
            return;

        customerList[0].Bye();
        customerList.RemoveAt(0);

        if (customerList.Count > 0)
        {
            // 대기 손님 있을 때
            for (int i = 0; i < customerList.Count; i++)
            {
                customerList[i].transform.position -= new Vector3(0.5f, 0);
            }
            UpdateOrderText(customerList[0].orderList, customerList[0].transform.GetChild(0).gameObject);
        }
        else
        {
            // 대기 손님 없을 때
            CancelInvoke();
            AddCustomer();
        }
    }
    
    public void ReturnCustomer(Customer customer)
    {
        customerPool.Add(customer);
    }

    public bool GiveYakitori(Yakitori yakitori)
    {
        // 첫번째 손님 주문 목록 비교
        List<Order> curOrderList = customerList[0].orderList;
        for(int i = 0; i < curOrderList.Count; i++)
        {
            Order curOrder = curOrderList[i];
            // 주문 발견시 제출
            if (curOrder.index == yakitori.index)
            {
                curOrder.amount--;
                UpdateOrderText(curOrderList, customerList[0].transform.GetChild(0).gameObject);
                //curOrder.text.text = yakitoriDatabase.GetYakitori(curOrder.index).displayName + " " + curOrder.amount + "개";

                // 계산과 판매량 관리
                money += yakitoriDatabase.GetYakitori(yakitori.index).price;
                moneyText.text = "매출 : " + money.ToString("N0") + " 원";

                soldYakitori[yakitori.index]++;
                sfxManager.PlaySFX(SFXType.Coin);

                // 잔여 개수 0개가 되면 주문 클리어
                if (curOrder.amount <= 0)
                {
                    //curOrder.text.text = "";
                    //curOrder.text.gameObject.SetActive(false);
                    customerList[0].orderList.RemoveAt(i);
                    UpdateOrderText(curOrderList, customerList[0].transform.GetChild(0).gameObject);

                    // 주문 리스트 0개가 되면 손님 클리어
                    if (customerList[0].orderList.Count <= 0)
                    {
                        Invoke("ByeCustomer", 0.5f);
                    }
                }
                yakitori.MoveToCustomer(firstLine + Vector3.up * 0.5f);
                return true;
            }
        }

        sfxManager.PlaySFX(SFXType.Wrong);
        WrongYakitori();
        return false;
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

    public void RetryGame()
    {
        SceneManager.LoadScene(1);
    }

    public void PauseGame(bool b)
    {
        Time.timeScale = b ? 0 : 1;
    }

}
