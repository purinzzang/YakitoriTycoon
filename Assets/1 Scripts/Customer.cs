using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Order
{
    public YakitoriType yakitoriType;
    public SauceType sauceType;
    public int amount;
    public string name;
    public TextMeshProUGUI text;
}

public class Customer : MonoBehaviour
{
    public Sprite[] sprites;
    public List<Order> orderList = new List<Order>();

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[Random.Range(0, sprites.Length)];
        StartCoroutine(WaitCo());
    }

    IEnumerator WaitCo()
    {
        yield return new WaitForSeconds(30);
        GameManager.instance.ByeCustomer();
    }

    public bool AddOrder(Order order)
    {
        // 중복 확인
        for(int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].yakitoriType == order.yakitoriType
                && orderList[i].sauceType == order.sauceType)
                return false;
            
        }

        orderList.Add(order);
        return true;
    }
}
