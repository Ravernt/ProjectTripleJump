using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    AudioManager audioManager;
    [SerializeField] Health health;
    [SerializeField] private bool canReactivate = false;
    bool wasChecked = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!wasChecked || canReactivate)
        {
            audioManager.PlaySFX(audioManager.checkpoint);
            wasChecked = true;
            health.initialPosition = transform.position;
        }
    }
}
