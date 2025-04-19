using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool isActivated = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
            isActivated = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isActivated = false;
    }
}
