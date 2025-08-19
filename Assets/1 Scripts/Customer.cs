using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Order
{
    public int index;
    public int amount;
    public TextMeshProUGUI text;
}

public class Customer : MonoBehaviour
{
    public Sprite[] sprites, outlines;
    public List<Order> orderList = new List<Order>();

    GameManager gameManager;
    SpriteRenderer sr, outlineSR;


    void Start()
    {
        gameManager = GameManager.instance;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitCo());
        sr = GetComponent<SpriteRenderer>();
        outlineSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
        int ran = Random.Range(0, sprites.Length);
        sr.sprite = sprites[ran];
        outlineSR.sprite = outlines[ran];
        outlineSR.gameObject.SetActive(false);
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
        sr.color = new Color(1, 1, 1, 1);
        transform.position = gameManager.firstLine;
        orderList.Clear();
        gameManager.ReturnCustomer(this);
    }

    public void AddOrder(Order order)
    {
        // 중복 확인
        for(int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].index == order.index)
                return;
        }

        orderList.Add(order);
    }

    public void TouchCustomer()
    {
        gameManager.UpdateOrderText(orderList, outlineSR.gameObject);
    }
}
