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

    public Transform[] grills;
    public Yakitori[] yakitoris = new Yakitori[9];
    public List<Yakitori> yakitoriPool = new List<Yakitori>();

    public SauceType curSauce;

    public Customer customerPrefab;
    public List<Customer> customerList = new List<Customer>();
    YakitoriType[] yakitoriTypes;
    SauceType[] sauceTypes;

    public TextMeshProUGUI[] orderTexts;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        yakitoriTypes = (YakitoriType[])Enum.GetValues(typeof(YakitoriType));
        sauceTypes = (SauceType[])Enum.GetValues(typeof(SauceType));

        AddCustomer();
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
                    newYakitori.cookedSprite = prefab.cookedSprite;
                    newYakitori.sweetSprite = prefab.sweetSprite;
                    newYakitori.hotSprite = prefab.hotSprite;
                    newYakitori.InitYakitori(prefab.GetComponent<SpriteRenderer>().sprite);
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
        // �ҽ� ����
        if(curSauce != type)
        {
            curSauce = type;
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
        for (int i = 0; i < 3; i++)
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
            UpdateOrderText();
        }
        Invoke("AddCustomer", 15f);
    }

    void UpdateOrderText()
    {
        for(int i = 0; i < customerList[0].orderList.Count; i++)
        {
            Order newOrder = customerList[0].orderList[i];
            newOrder.text.text = newOrder.name + " " + newOrder.amount + "��";
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
                break;
            }
        }
    }
}
