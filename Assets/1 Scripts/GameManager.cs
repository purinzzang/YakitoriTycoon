using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
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

    int maxOrder, money;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        sfxManager = GetComponentInChildren<SFXManager>();
        maxOrder = 1;
        yakitoris = new Yakitori[grills.Length];
        yakitoriTypes = (YakitoriType[])Enum.GetValues(typeof(YakitoriType));
        sauceTypes = (SauceType[])Enum.GetValues(typeof(SauceType));

        AddCustomer();
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {

        yield return new WaitForSeconds(30f);
        Debug.Log("1 money : " + money);
        maxOrder++;
        yield return new WaitForSeconds(30f);
        Debug.Log("2 money : " + money);
        maxOrder++;
        yield return new WaitForSeconds(30f);
        Debug.Log("3 money : " + money);
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
            UpdateOrderText();
        }
        Invoke("AddCustomer", 10f);
    }

    void UpdateOrderText()
    {
        for(int i = 0; i < customerList[0].orderList.Count; i++)
        {
            Order newOrder = customerList[0].orderList[i];
            newOrder.text.text = newOrder.name + " " + newOrder.amount + "개";
            newOrder.text.gameObject.SetActive(true);
        }
    }

    public void ByeCustomer()
    {
        customerList[0].gameObject.SetActive(false);
        customerList.RemoveAt(0);
        for(int i = 0; i <  customerList.Count; i++)
        {
            customerList[i].transform.position -= new Vector3(0.5f, 0);
        }
        if (customerList.Count > 0)
        {
            UpdateOrderText();
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

                // 계산
                int price = 0;
                if (yakitori.yakitoriType == YakitoriType.Hatsu)
                    price += 3000;
                else
                    price += 4000;

                if (yakitori.sauceType != SauceType.None)
                    price += 500;

                money += price;
                sfxManager.PlayCoin();

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

        sfxManager.PlayWrong();
    }
}
