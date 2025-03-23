using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Health : MonoBehaviour
{
    /// <summary>
    /// 
    /// 
    /// 
    /// The damage bounce is too fast
    /// 
    /// </summary>
    /// 

    Rigidbody2D player;
    GameObject respawn;
    void Awake()
    {
        //gets the player information
        player = GetComponent<Rigidbody2D>();

        if (respawnPoint != null)
        {
            initialPosition = respawnPoint.position;
        }
        else
        {
            // usses current position as respawn point
            initialPosition = transform.position;
        }

        InitializeBlackoutPanel();
    }

    [SerializeField] PlayerController playerController;
    [SerializeField] private int health = 12;
    [SerializeField] private bool alwaysInvincible = false;
    [SerializeField] private int invincibilityFrameCount = 5;
    [SerializeField] Transform respawnPoint;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeSpeed = 2.0f;
    [SerializeField] ParticleSystem hitParticle;
    [SerializeField] ParticleSystem deathParticle;

    private float thrust = 15f;
    public Vector2 initialPosition;
    private int updateCount = 0;
    private bool invincible = false;
    private int invincibilityStart;
    private GameObject blackoutPanel;
    private CanvasGroup blackoutCanvasGroup;
    private bool isRespawning = false;

    bool dead = false;
    public event Action OnRespawn;
    public event Action OnDeath;

    void FixedUpdate()
    {
        updateCount++;
        if (updateCount >= 65536 && !invincible)
            updateCount = 0;
        if (!invincible)
        {
            spriteRenderer.color = Color.white;
        }
        if (invincible && (updateCount - invincibilityStart) >= invincibilityFrameCount)
        {
            //removes invincibility once the invincibility frame count runs out
            invincible = false;
        }
        if (!dead && health <= 0 && !alwaysInvincible)
        {
            dead = true;
            deathParticle.Play(true);
            OnDeath?.Invoke();
            spriteRenderer.color = Color.red;

            // Start the blackout and respawn process if not already in progress
            if (!isRespawning)
            {
                StartCoroutine(BlackoutAndRespawn());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!invincible && collision.collider.tag == "Hurtful")
        {
            //adds a force away from the spike, so that the player doesn't rub against it
            float xValue = transform.position.x - collision.transform.position.x;
            float yValue = transform.position.y - collision.transform.position.y;
            Vector2 direction = new(xValue, yValue);
            playerController.ApplyForce(direction.normalized * thrust, 0.1f);
            spriteRenderer.color = Color.red;
            hitParticle.Play(true);
            if (!alwaysInvincible)
            {
                //removes health and starts invincibility frames
                health -= 4;
                invincible = true;
                invincibilityStart = updateCount;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "InstaKill")
        {
            if (!dead && !invincible && !alwaysInvincible)
            {
                health = 0;
                invincible = true;
                invincibilityStart = updateCount;
                hitParticle.Play(true);
            }
        }
    }

    private void InitializeBlackoutPanel()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("BlackoutCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        blackoutPanel = new GameObject("BlackoutPanel");
        blackoutPanel.transform.SetParent(canvas.transform, false);

        UnityEngine.UI.Image image = blackoutPanel.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;

        RectTransform rectTransform = blackoutPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        blackoutCanvasGroup = blackoutPanel.AddComponent<CanvasGroup>();
        blackoutCanvasGroup.alpha = 0;
        blackoutCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator BlackoutAndRespawn()
    {
        isRespawning = true;

        // Fade to black
        while (blackoutCanvasGroup.alpha < 1)
        {
            blackoutCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // respawn the player
        transform.position = initialPosition;
        health = 12;
        dead = false;
        OnRespawn?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Fade back from black
        while (blackoutCanvasGroup.alpha > 0)
        {
            blackoutCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        isRespawning = false;
    }
}
