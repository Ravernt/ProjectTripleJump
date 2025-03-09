using UnityEngine;
using UnityEngine.Pool;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] Bullet prefab;
    [SerializeField] Transform firePoint1;
    [SerializeField] Transform firePoint2;

    void Start()
    {
        GetComponent<PlayerController>().OnStateChange += Shoot;
    }

    void Shoot(PlayerState state)
    {
        if (state != PlayerState.DoubleJumping)
            return;

        Instantiate(prefab, firePoint1.position, firePoint1.rotation);
        Instantiate(prefab, firePoint2.position, firePoint2.rotation);
    }
}
