using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Health : MonoBehaviour
{
    /// <summary>
    /// Need to add a way to communicate that the player died or got hurt
    /// 
    /// current update frame counting way is inefficient and can lead to int overflow
    /// 
    /// The damage bounce is too fast
    /// 
    /// Need to respawn the player at a respawn position instead of the initial one
    /// </summary>
    /// 

    Rigidbody2D player;
    GameObject respawn;
    void Awake()
    {
        //gets the player information
        player = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
    }

    [SerializeField] private int health = 12;
    [SerializeField] private bool alwaysInvincible = false;
    [SerializeField] private int invincibilityFrameCount = 5;
    private int thrust = 10000;

    Vector2 initialPosition;
    private int updateCount = 0;
    private bool invincible = false;
    private int invincibilityStart;

    void FixedUpdate()
    {
        updateCount++;

        if (invincible && (updateCount - invincibilityStart) >= invincibilityFrameCount)
        {
            //removes invincibility once the invincibility frame count runs out
            invincible = false;
        }
        if (health <= 0 && !alwaysInvincible)
        {
            //respawns the player at a set position
            transform.position = initialPosition;
            
            health = 12;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Hurtful")
        {
            if(!invincible && !alwaysInvincible)
            {
                //removes health and starts invincibility frames
                health -= 4;
                invincible = true;
                invincibilityStart = updateCount;

                //adds a force away from the spike, so that the player doesn't rub against it
                float xValue = transform.position.x-collision.transform.position.x;
                float yValue = transform.position.y-collision.transform.position.y;
                Vector2 direction = new(xValue, yValue);
                player.AddForce(direction * thrust);
            }
            
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "InstaKill")
        {
            if (!invincible && !alwaysInvincible)
            {
                health = 0;
                invincible = true;
                invincibilityStart = updateCount;
            }
        }
    }

}
