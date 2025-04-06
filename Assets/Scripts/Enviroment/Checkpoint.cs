using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] private bool canReactivate = false;
    bool wasChecked = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!wasChecked || canReactivate)
        {
            wasChecked = true;
            health.initialPosition = transform.position;
        }
    }
}
