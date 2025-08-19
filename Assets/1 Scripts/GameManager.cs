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
    // �Ŵ���
    public static GameManager instance;
    SFXManager sfxManager;

    // �߲�ġ
    public Transform[] grills;
    Yakitori[] yakitoris;
    List<Yakitori> yakitoriPool = new List<Yakitori>();
    public YakitoriDatabase yakitoriDatabase;

    // �ҽ�
    public SpriteRenderer sweetSR, hotSR;
    public Sprite[] sweetSprites, hotSprites;
    public SauceType curSauce;

    // �մ�
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

    // ������
    int maxOrder, money, tryYakitori, burnYakitori, wrongYakitori;
    int[] soldYakitori;
    public TextAsset reviewData;
    Dictionary<int, string> reviewDict;

    // �ִϸ��̼�
    public Animator adAnim, countAnim, endAnim, bgAnim;

    // ĳ���� ����
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

        // �մ� ���� ������
        for (int i = 0; i < customerList.Count; i++)
        {
            customerList[i].Bye();
        }
        customerList.Clear();

        // ���� ��� ����
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
        endText.text = totalYakitori + " ��\n" 
            + money.ToString("N0") + " ��\n" 
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


        // ����ǥ �ִϸ��̼�
        endAnim.SetTrigger("doEnd");
    }

    // �߲�ġ
    public void AddYakitori(int index)
    {
        // �׸� ���ڸ� ã�� �߲�ġ ��ġ
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
        // �ҽ� ����
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

    // �մ�
    void AddCustomer()
    {
        // �մ� ����
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

        // ���� �ֹ� ����
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
        // �ֹ� �ؽ�Ʈ �ʱ�ȭ
        for(int i = 0; i < orderTexts.Length; i++)
        {
            orderTexts[i].gameObject.SetActive(false);
        }

        // �ֹ� ����� �׸� ǥ��
        for(int i = 0; i < orderList.Count; i++)
        {
            Order order = orderList[i];
            order.text.text = yakitoriDatabase.GetYakitori(order.index).displayName + " " + order.amount + "��";
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
            // ��� �մ� ���� ��
            for (int i = 0; i < customerList.Count; i++)
            {
                customerList[i].transform.position -= new Vector3(0.5f, 0);
            }
            UpdateOrderText(customerList[0].orderList, customerList[0].transform.GetChild(0).gameObject);
        }
        else
        {
            // ��� �մ� ���� ��
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
        // ù��° �մ� �ֹ� ��� ��
        List<Order> curOrderList = customerList[0].orderList;
        for(int i = 0; i < curOrderList.Count; i++)
        {
            Order curOrder = curOrderList[i];
            // �ֹ� �߽߰� ����
            if (curOrder.index == yakitori.index)
            {
                curOrder.amount--;
                UpdateOrderText(curOrderList, customerList[0].transform.GetChild(0).gameObject);
                //curOrder.text.text = yakitoriDatabase.GetYakitori(curOrder.index).displayName + " " + curOrder.amount + "��";

                // ���� �Ǹŷ� ����
                money += yakitoriDatabase.GetYakitori(yakitori.index).price;
                moneyText.text = "���� : " + money.ToString("N0") + " ��";

                soldYakitori[yakitori.index]++;
                sfxManager.PlaySFX(SFXType.Coin);

                // �ܿ� ���� 0���� �Ǹ� �ֹ� Ŭ����
                if (curOrder.amount <= 0)
                {
                    //curOrder.text.text = "";
                    //curOrder.text.gameObject.SetActive(false);
                    customerList[0].orderList.RemoveAt(i);
                    UpdateOrderText(curOrderList, customerList[0].transform.GetChild(0).gameObject);

                    // �ֹ� ����Ʈ 0���� �Ǹ� �մ� Ŭ����
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
        return reviewDict.TryGetValue(key, out var review) ? review : "���� ����";
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
