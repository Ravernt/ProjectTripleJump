using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    AudioManager audioManager;
    SpriteRenderer spriteRenderer;
    [SerializeField] Health health;
    //[SerializeField] private bool canReactivate = true;
    bool isChecked = false;
    Checkpoint[] checkpoints;
    Color tempColor;

    void Awake()                      
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        health = FindAnyObjectByType<Health>();
        checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        /*
        if(health.initialPosition.x != transform.position.x || health.initialPosition.y != transform.position.y)
        {
            tempColor = spriteRenderer.color;
            tempColor.a = 0f;
            spriteRenderer.color = tempColor;
        }*/
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !isChecked)
        {
            audioManager.PlaySFX(audioManager.checkpoint);
            //set a new color
            tempColor.a = 250f;
            spriteRenderer.color = tempColor;
            health.initialPosition = transform.position;
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if(checkpoint.isChecked)
                {
                    //reset to old color, dont forget the checkoint. prefix
                    checkpoint.tempColor = checkpoint.spriteRenderer.color;
                    checkpoint.tempColor.a = 0f;
                    checkpoint.spriteRenderer.color = checkpoint.tempColor;
                    checkpoint.isChecked = false;
                }
            }
            isChecked = true;
        }
    }
}
