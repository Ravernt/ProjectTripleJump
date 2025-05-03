using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    AudioManager audioManager;
    [SerializeField] Health health;
    [SerializeField] private bool canReactivate = false;
    bool wasChecked = false;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        health = FindAnyObjectByType<Health>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && (!wasChecked || canReactivate))
        {
            audioManager.PlaySFX(audioManager.checkpoint);
            wasChecked = true;
            //health.initialPosition = transform.position;
            health.initialPosition = transform.position;
        }
    }
}
