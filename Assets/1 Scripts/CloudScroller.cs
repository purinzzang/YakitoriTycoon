using UnityEngine;

public class CloudScroller : MonoBehaviour
{
    float scrollSpeed;
    float resetX = -3f;
    float startX = 3f;

    private void Start()
    {
        scrollSpeed = Random.Range(0.1f, 0.2f);
    }

    void Update()
    {
        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;

        if (transform.position.x < resetX)
        {
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }
    }
}
