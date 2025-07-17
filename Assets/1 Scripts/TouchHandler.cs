using UnityEngine;
using UnityEngine.EventSystems;

public class TouchHandler : MonoBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    void Update()
    {
        HandleMouseInput();

//#if UNITY_EDITOR || UNITY_STANDALONE
//        HandleMouseInput();
//#elif UNITY_ANDROID || UNITY_IOS
//        HandleTouchInput();
//#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ui ����
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {

                // ������Ʈ Ÿ�Կ� ���� ó��
                var yakitori = hit.collider.GetComponent<Yakitori>();
                if (yakitori != null)
                {
                    yakitori.TouchYakitori();
                }

                var box = hit.collider.GetComponent<YakitoriBox>();
                if (box != null)
                {
                    box.SetYakitori();
                }

                var sauce = hit.collider.GetComponent<Sauce>();
                if(sauce != null)
                {
                    sauce.ChangeSauce();
                }

                var customer = hit.collider.GetComponent<Customer>();
                if (customer != null)
                {
                    customer.TouchCustomer();
                }
            }
            else
            {
                gameManager.ChangeSauce(SauceType.None);
            }
        }

    }

    void HandleTouchInput()
    {
        // ���Է� �� ��Ƽ ��ġ ����
        if (Input.touchCount != 1)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        // ui Ȯ��
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // ������Ʈ Ÿ�Կ� ���� ó��
            var yakitori = hit.collider.GetComponent<Yakitori>();
            if (yakitori != null)
            {
                yakitori.TouchYakitori();
            }

            var box = hit.collider.GetComponent<YakitoriBox>();
            if (box != null)
            {
                box.SetYakitori();
            }
        }


    }
}
