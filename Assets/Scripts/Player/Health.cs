using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System;

public class Health : MonoBehaviour
{
    AudioManager audioManager;

    Rigidbody2D player;
    GameObject respawn;
    void Awake()
    {
        var source = GameObject.FindGameObjectWithTag("Audio");

        if(source != null)
        {
            audioManager = source.GetComponent<AudioManager>();
        }

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
        transform.position = initialPosition;
        InitializeBlackoutPanel();
    }

    [SerializeField] PlayerController playerController;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool alwaysInvincible = false;
    [SerializeField] private float invincibilityTime;
    [SerializeField] Transform respawnPoint;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeSpeed = 2.0f;
    [SerializeField] ParticleSystem hitParticle;
    [SerializeField] ParticleSystem deathParticle;

    private float thrust = 15f;
    public Vector2 initialPosition;
    private GameObject blackoutPanel;
    private CanvasGroup blackoutCanvasGroup;

    public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    public int CurrentHealth { get; set; }
    public bool Dead { get; set; } = false;
    public bool Invincible { get; set; } = false;
    public float InvincibilityTime { get { return invincibilityTime; } set { invincibilityTime = value; } }
    public event Action OnRespawn;
    public event Action OnDeath;
    Coroutine invincibilityCoroutine;
    Coroutine respawnCoroutine;

    void Start()
    {
        CurrentHealth = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Hurtful")
        {
            float xValue = transform.position.x - collision.transform.position.x;
            float yValue = transform.position.y - collision.transform.position.y;
            Vector2 direction = new(xValue, yValue);
            playerController.ApplyForce(direction.normalized * thrust, 0.1f);

            TakeDamage(1);
        }

        if (collision.collider.tag == "InstaKill")
        {
            Death();
        }
    }

    public void TakeDamage(int amount)
    {
        if (Dead || alwaysInvincible || Invincible)
            return;

        CurrentHealth -= amount;

        if(CurrentHealth <= 0)
        {
            Death();
        }
        else
        {
            if (audioManager != null)
                audioManager.PlaySFX(audioManager.hurt);
            hitParticle.Play(true);

            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
            }

            invincibilityCoroutine = StartCoroutine(Invincibility());
        }
    }

    public void Death()
    {
        if (Dead || alwaysInvincible)
            return;

        Dead = true;
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.dead);
        deathParticle.Play(true);
        OnDeath?.Invoke();
        spriteRenderer.color = Color.red;

        if(respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(BlackoutAndRespawn());
    }

    public IEnumerator Invincibility()
    {
        Invincible = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(invincibilityTime);
        spriteRenderer.color = Color.white;
        Invincible = false;
        invincibilityCoroutine = null;
    }

    void InitializeBlackoutPanel()
    {
        GameObject canvasObject = new GameObject("BlackoutCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        canvas.transform.position = Camera.main.transform.position;
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

    public IEnumerator BlackoutAndRespawn()
    {
        // Fade to black
        while (blackoutCanvasGroup.alpha < 1)
        {
            blackoutCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        // respawn the player
        transform.position = initialPosition;
        CurrentHealth = maxHealth;
        spriteRenderer.color = Color.white;
        Dead = false;
        OnRespawn?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Fade back from black
        while (blackoutCanvasGroup.alpha > 0)
        {
            blackoutCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        respawnCoroutine = null;

        
    }
}
