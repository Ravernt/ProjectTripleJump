using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Transform endPosition;
    [SerializeField] float speed;

    bool returning = false;

    PlayerController controller;
    Rigidbody2D rb;
    Vector2 startPosition;

    Vector3 lastPosition;
    Vector2 velocity;

    void Start()
    {
        controller = FindAnyObjectByType<PlayerController>();
        rb = GetComponentInChildren<Rigidbody2D>();
        startPosition = transform.position;
        endPosition.SetParent(null);
    }

    void Update()
    {
        rb.linearVelocity = (returning ? -1 : 1) * speed *  (endPosition.position - transform.position).normalized;

        if (!returning && Vector2.Distance(transform.position, endPosition.position) <= 0.025f)
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

    void OnDrawGizmos()
    {
        if(endPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, endPosition.position);
        }
    }
}
