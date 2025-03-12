using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] Transform player;

    Collider2D platformCollider;

    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        platformCollider.enabled = transform.position.y < player.position.y - 0.5f;
    }
}
