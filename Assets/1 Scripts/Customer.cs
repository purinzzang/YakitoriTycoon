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

    GameManager gameManager;
    SpriteRenderer sr;

    void Start()
    {
        gameManager = GameManager.instance;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[Random.Range(0, sprites.Length)];
        StartCoroutine(WaitCo());
    }

    IEnumerator WaitCo()
    {
        yield return new WaitForSeconds(30);
        gameManager.ByeCustomer();

    }

    public void Bye()
    {
        StartCoroutine(ByeCo());
    }

    IEnumerator ByeCo()
    {
        int it = 0;
        sr.color = new Color(1, 1, 1, 0.5f);
        while (transform.position.x > -3)
        {
            yield return null;
            it++;
            transform.position += Vector3.left * Time.deltaTime;
            if (it > 1000)
            {
                Debug.Log("it 1000");
                break;
            }
        }
        gameObject.SetActive(false);
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

    public void TouchCustomer()
    {
        gameManager.UpdateOrderText(orderList);
    }
}
