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

    Vector2 direction;

    void Start()
    {
        controller = FindAnyObjectByType<PlayerController>();
        rb = GetComponentInChildren<Rigidbody2D>();
        startPosition = transform.position;
        endPosition.SetParent(null);

        direction = ((Vector2)transform.position - startPosition).normalized;
    }

    void Update()
    {
        rb.linearVelocity = (returning ? -1 : 1) * speed * ((Vector2)endPosition.position - startPosition).normalized;

        if (returning && !AreSimilarDirections(((Vector2)transform.position - startPosition).normalized, direction))
        {
            returning = false;
            direction = (transform.position - endPosition.position).normalized;
        }
        else if (!returning && !AreSimilarDirections((Vector2)(transform.position - endPosition.position).normalized, direction))
        {
            returning = true;
            direction = ((Vector2)transform.position - startPosition).normalized;
        }

        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    bool AreSimilarDirections(Vector2 direction1, Vector2 direction2)
    {
        return Mathf.Abs(direction1.x - direction2.x) < 0.001f && Mathf.Abs(direction1.y - direction2.y) < 0.001f;
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
