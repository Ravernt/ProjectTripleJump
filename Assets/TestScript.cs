using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPos = transform.position;
    }
    Vector2 initialPos;
    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
       transform.position = initialPos;
    }
}
