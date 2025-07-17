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
            // ui 제한
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {

                // 오브젝트 타입에 따라 처리
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
        // 무입력 및 멀티 터치 제한
        if (Input.touchCount != 1)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        // ui 확인
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // 오브젝트 타입에 따라 처리
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
