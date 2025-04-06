using UnityEngine;

public class ArrowLogic : MonoBehaviour
{
    [SerializeField] PressurePlate pressurePlate;
    [SerializeField] float arrowSpeed = 10f;
    [SerializeField] ParticleSystem hitParticle;
    private void Start()
    {
        arrowDirection = new Vector2((pressurePlate.transform.position.x - transform.position.x), 0).normalized;
        initialPosition = transform.position;
    }
    Vector2 arrowDirection;
    Vector2 initialPosition;
    bool inFlight = false;
    void FixedUpdate()
    {
        if(pressurePlate.isActivated)
        {
            inFlight = true;
        }
        if(inFlight)
        {
            transform.position = new Vector2(transform.position.x + arrowDirection.x * Time.deltaTime * arrowSpeed, transform.position.y);
            hitParticle.transform.position = transform.position;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.name != "Shooter")
        {
            hitParticle.Play(true);
            inFlight = false;
            transform.position = new Vector3(initialPosition.x, initialPosition.y, 0.2f);
        }
    }
}
