using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] PlayerController controller;
    [SerializeField] Vector2 direction;
    [SerializeField] float speed;

    bool returning = false;

    Rigidbody2D rb;
    Vector2 startPosition;
    Vector2 endPosition;

    Vector3 lastPosition;
    Vector2 velocity;

    void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        startPosition = transform.position;
        endPosition = (Vector2)transform.position + direction;
    }

    void Update()
    {
        rb.linearVelocity = (returning ? -1 : 1) * speed * direction.normalized;

        if (!returning && Vector2.Distance(transform.position, endPosition) <= 0.025f)
        {
            returning = true;
        }

        if (returning && Vector2.Distance(transform.position, startPosition) <= 0.025f)
        {
            returning = false;
        }

        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            controller.CalculateVelocity += AddVelocity;
        }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            controller.CalculateVelocity -= AddVelocity;
        }
    }

    void AddVelocity(ref Vector2 velocity)
    {
        velocity += this.velocity;
    }
}
