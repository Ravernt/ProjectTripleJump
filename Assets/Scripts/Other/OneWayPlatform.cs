using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    Transform player;
    Collider2D platformCollider;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        platformCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        platformCollider.enabled = transform.position.y < player.position.y - 0.5f;
    }
}
