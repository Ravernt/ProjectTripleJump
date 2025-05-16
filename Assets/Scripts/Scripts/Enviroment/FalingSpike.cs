using Unity.VisualScripting;
using UnityEngine;

public class FalingSpike : MonoBehaviour
{
    [SerializeField] ParticleSystem hitParticle;
    AudioManager audioManager;
    Health health;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    public bool hasPlayed = false;
    Vector2 initialPosition;
    public bool Destroyed { get; private set; } = false;
    void Awake()
    {
        var source = GameObject.FindGameObjectWithTag("Audio");
        var playerState = GameObject.FindGameObjectWithTag("Player");
        initialPosition = transform.position;
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        //if (source != null)
        //{
        //    audioManager = source.GetComponent<AudioManager>();
        //}
        health = playerState.GetComponent<Health>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
    }
    // Update is called once per frame
    void Update()
    {
        //if (health.Dead)
        //{
        //    Invoke(nameof(Respawn), 1.5f);
        //}
        if (!Destroyed)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);
            if (hit.collider.tag == "Player")
            {
                if (!hasPlayed)
                {
                    hitParticle.Play();
                    hasPlayed = true;
                }

                rb.simulated = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if(audioManager != null)
        //{
        //    audioManager.PlaySFX(audioManager.spikeFalling);
        //}

        hitParticle.transform.position = transform.position;
        hitParticle.Play(true);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Invoke(nameof(DestroyThis), 0.05f);
    }

    void DestroyThis()
    {
        transform.position = new Vector2(transform.position.x+10000, transform.position.y+10000);
        rb.simulated = false;
        Destroyed = true;
        //Destroy(gameObject);
    }

    /*
    void Respawn()
    {
        transform.position = initialPosition;
        Destroyed = false;
    }
    */
}
