using TMPro;
using UnityEngine;

public class ArrowLogic : MonoBehaviour
{
    AudioManager audioManager;
    [SerializeField] PressurePlate pressurePlate;
    [SerializeField] float arrowSpeed = 10f;
    [SerializeField] ParticleSystem hitParticle;
    Vector2 arrowDirection;
    Vector2 initialPosition;
    bool inFlight = false;

    bool canActivate = true;

    private void Start()
    {
        arrowDirection = new Vector2((pressurePlate.transform.position.x - transform.position.x), 0).normalized;
        initialPosition = transform.position;
    }

    void Awake()
    {
        var manager = GameObject.FindGameObjectWithTag("Audio");
        if (manager != null)
        {
            audioManager = manager.GetComponent<AudioManager>();
        }
    }

    void Update()
    {
        if(canActivate && pressurePlate.isActivated)
        {
            if(audioManager != null)
                audioManager.PlaySFX(audioManager.bullet);
            canActivate = false;
            inFlight = true;
        }
        if(inFlight)
        {
            transform.position = new Vector2(transform.position.x + arrowDirection.x * Time.deltaTime * arrowSpeed, transform.position.y);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.name != "Shooter")
        {
            hitParticle.Play(true);
            inFlight = false;
            Invoke(nameof(ResetPosition), 0.1f);
        }
    }

    void ResetPosition()
    {
        canActivate = true;
        transform.position = new Vector3(initialPosition.x, initialPosition.y, 0.2f);
    }
}
