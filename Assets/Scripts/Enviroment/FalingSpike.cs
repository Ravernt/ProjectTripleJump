using Unity.VisualScripting;
using UnityEngine;

public class FalingSpike : MonoBehaviour
{
    [SerializeField] ParticleSystem hitParticle;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
    }
    Rigidbody2D rb;
    bool hasPlayed = false;
    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);
        if(hit.collider.tag == "Player")
        {
            if(!hasPlayed) { 
                hitParticle.Play();
                hasPlayed = true;
            }
            
            rb.simulated = true;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hitParticle.transform.position = transform.position;
        hitParticle.Play(true);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Invoke(nameof(DestroyThis), 0.05f);
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }
}
