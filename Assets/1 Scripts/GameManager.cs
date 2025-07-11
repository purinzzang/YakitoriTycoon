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
    List<Customer> customerList = new List<Customer>();
    List<Customer> customerPool = new List<Customer>();
    public Vector3 firstLine;

    // hud
    public TextMeshProUGUI[] orderTexts;
    public TextMeshProUGUI moneyText, endText, reviewText;
    public Image timeBar;

    // ������
    int maxOrder, money, tryYakitori, burnYakitori, wrongYakitori;
    int[] soldYakitori;
    public TextAsset reviewData;
    Dictionary<int, string> reviewDict;

    // �ִϸ��̼�
    public Animator endAnim, bgAnim;

    private void Awake()
    {
        instance = this;
        SetCameraSize();
    }

    void SetCameraSize()
    {
        float ratio = (float)Screen.height / Screen.width;

        float minRatio = 1.777f; // 720x1280
        float maxRatio = 2.222f; // 1440x3200

        ratio = Mathf.Clamp(ratio, minRatio, maxRatio);

        float cameraSize = Mathf.Lerp(4f, 5f, (ratio - minRatio) / (maxRatio - minRatio));
        Camera.main.orthographicSize = cameraSize;
    }


    private void Start()
    {
        sfxManager = GetComponentInChildren<SFXManager>();
        maxOrder = 1;
        tryYakitori = 0;
        soldYakitori = new int[9];
        burnYakitori = 0;
        yakitoris = new Yakitori[grills.Length];
        firstLine = customerPrefab.transform.position;
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
        CancelInvoke();

        // �մ� ���� ������
        for(int i = 0; i < customerList.Count; i++)
        {
            customerList[i].Bye();
        }
        customerList.Clear();

        // ���� ��� ����
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
        endText.text = totalYakitori + " ��\n" + money + " ��\n" + "100 %\n" + yakitoriDatabase.GetYakitori(bestYakitori).displayName;
        int burnScore = ((float)burnYakitori / tryYakitori) < 0.1f ? 200 
            : ((float)burnYakitori / tryYakitori) < 0.3f ? 100
            : 0;
        int accuracyScore = ((float)wrongYakitori / (wrongYakitori + totalYakitori)) < 0.1f ? 20
            : ((float)wrongYakitori / (wrongYakitori + totalYakitori)) < 0.3f ? 10
            : 0;
        Debug.Log("burn ratio: " + ((float)burnYakitori / tryYakitori) + " / wrong ratio: " + ((float)wrongYakitori / (wrongYakitori + totalYakitori)));
        reviewText.text = GetReview(burnScore, accuracyScore, bestYakitori);

        // ����ǥ �ִϸ��̼�
        endAnim.SetTrigger("doEnd");
    }

    // �߲�ġ
    public void AddYakitori(Yakitori prefab)
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
                    newYakitori.InitYakitori(prefab.GetComponent<Animator>().runtimeAnimatorController, prefab.index);
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
        // �ҽ� ����
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
            Debug.Log("��Ȱ���� �մ�");
        }
        else
        {
            newCustomer = Instantiate(customerPrefab);
            Debug.Log("���θ��� �մ�");
        }

        // ���� �ֹ� ����
        for (int i = 0; i < maxOrder; i++)
        {
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
            Order order = orderList[i];
            order.text.text = yakitoriDatabase.GetYakitori(order.index).displayName + " " + order.amount + "��";
            order.text.gameObject.SetActive(true);
        }
    }

    public void ByeCustomer()
    {
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
    
    public void ReturnCustomer(Customer customer)
    {
        customerPool.Add(customer);
    }

    public void GiveYakitori(Yakitori yakitori)
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
                curOrder.text.text = yakitoriDatabase.GetYakitori(curOrder.index).displayName + " " + curOrder.amount + "��";

                // ���� �Ǹŷ� ����
                //var (index, price) = GetYakitoriInfo(yakitori.yakitoriType, yakitori.sauceType);
                money += yakitoriDatabase.GetYakitori(yakitori.index).price;
                moneyText.text = "���� : " + money + " ��";

                soldYakitori[yakitori.index]++;
                sfxManager.PlaySFX(SFXType.Coin);

                // �ܿ� ���� 0���� �Ǹ� �ֹ� Ŭ����
                if (curOrder.amount <= 0)
                {
                    curOrder.text.text = "";
                    curOrder.text.gameObject.SetActive(false);
                    customerList[0].orderList.RemoveAt(i);

                    // �ֹ� ����Ʈ 0���� �Ǹ� �մ� Ŭ����
                    if (customerList[0].orderList.Count <= 0)
                    {
                        Invoke("ByeCustomer", 0.5f);
                    }
                }
                yakitori.MoveToCustomer(firstLine + Vector3.up * 0.5f);
                return;
            }
        }

        sfxManager.PlaySFX(SFXType.Wrong);
        WrongYakitori();
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
        SceneManager.LoadScene(0);
    }
}
