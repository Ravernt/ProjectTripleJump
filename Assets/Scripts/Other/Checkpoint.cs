using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    AudioManager audioManager;
    SpriteRenderer spriteRenderer;
  //  [SerializeField] Health health;
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;
    bool isChecked = false;
    Checkpoint[] checkpoints;

    static Checkpoint lastCheckpoint;

    void Awake()                      
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        //health = FindAnyObjectByType<Health>();
        checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = inactiveColor;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !isChecked)
        {
            audioManager.PlaySFX(audioManager.checkpoint);
            spriteRenderer.color = activeColor;
            //health.initialPosition = transform.position;
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if(checkpoint.isChecked)
                {
                    checkpoint.spriteRenderer.color = inactiveColor;
                    checkpoint.isChecked = false;
                }
            }
            isChecked = true;
        }
    }
}
