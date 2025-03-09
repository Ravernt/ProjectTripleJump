using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.linearVelocity = new(0, -speed);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        ParticleSpawner.Instance.SpawnParticle("player_bullet_hit", transform.position);
        Destroy(gameObject);
    }
}
