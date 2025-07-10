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
        Debug.Log("���� �մ� �� : " + customerList.Count);

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
        endText.text = totalYakitori + " ��\n" + money + " ��\n" + "100 %\n" + bestYakitori;
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

    public void DestroyYakitori(Yakitori yakitori)
    {
        yakitoris[Array.IndexOf(yakitoris, yakitori)] = null;
        yakitori.gameObject.SetActive(false);
        yakitoriPool.Add(yakitori);
    }

    void AddCustomer()
    {
        // �մ� ����
        Customer newCustomer = Instantiate(customerPrefab);

        // ���� �ֹ� ����
        for (int i = 0; i < maxOrder; i++)
        {
            Order newOrder = new Order();
            newOrder.yakitoriType = yakitoriTypes[Random.Range(0, yakitoriTypes.Length)];
            newOrder.sauceType = sauceTypes[Random.Range(0, sauceTypes.Length)];
            newOrder.amount = Random.Range(1, 4);

            if(newCustomer.AddOrder(newOrder))
            {
                string taste = newOrder.sauceType == SauceType.None ? ""
                     : newOrder.sauceType == SauceType.Sweet ? "���� "
                     : "���� ";
                string type = newOrder.yakitoriType == YakitoriType.Momo ? "�߲�ġ "
                     : newOrder.yakitoriType == YakitoriType.Negima ? "�Ĵ߲�ġ"
                     : "���벿ġ ";
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
            newOrder.text.text = newOrder.name + " " + newOrder.amount + "��";
            newOrder.text.gameObject.SetActive(true);
        }
    }

    public void ByeCustomer()
    {
        Debug.Log("�մ� ������");
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
        // ù��° �մ� �ֹ� ��� ��
        List<Order> curOrderList = customerList[0].orderList;
        for(int i = 0; i < curOrderList.Count; i++)
        {
            // �ֹ� �߽߰� ����
            if (curOrderList[i].yakitoriType == yakitori.yakitoriType
                && curOrderList[i].sauceType == yakitori.sauceType)
            {
                curOrderList[i].amount--;
                curOrderList[i].text.text = curOrderList[i].name + " " + curOrderList[i].amount + "��";

                // ���� �Ǹŷ� ����
                var (index, price) = GetYakitoriInfo(yakitori.yakitoriType, yakitori.sauceType);
                money += price;
                moneyText.text = "���� : " + money + " ��";

                soldYakitori[index]++;
                sfxManager.PlaySFX(SFXType.Coin);

                // �ܿ� ���� 0���� �Ǹ� �ֹ� Ŭ����
                if (curOrderList[i].amount <= 0)
                {
                    curOrderList[i].text.text = "";
                    curOrderList[i].text.gameObject.SetActive(false);
                    customerList[0].orderList.RemoveAt(i);

                    // �ֹ� ����Ʈ 0���� �Ǹ� �մ� Ŭ����
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
        return reviewDict.TryGetValue(key, out var review) ? review : "���� ����";
    }
}
