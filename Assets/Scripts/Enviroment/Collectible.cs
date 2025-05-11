using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public enum PerkType
{
    SpeedBoost,
    JumpBoost,
    GlideSlowdown,
    DashCooldown,
    DashDuration,
    DashSpeed
}

public class Collectible : MonoBehaviour
{
    [SerializeField] private PerkType perkType;
    [SerializeField] private Transform visuals;
    [SerializeField] private Transform background;
    [SerializeField] private string messageOverride = "";
    [SerializeField] private TMP_Text perkTextUI;

    private AudioManager audioManager;
    private Sequence mainAnimation;
    private Sequence backgroundAnimation;

    bool collected = false;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();

        // Animate visuals
        if (visuals != null)
        {
            mainAnimation = DOTween.Sequence();
            mainAnimation.Append(visuals.DOLocalMoveY(0.3f, 2f));
            mainAnimation.Append(visuals.DOLocalMoveY(0f, 2f));
            mainAnimation.SetLoops(-1);
        }

        // Animate background rotation
        if (background != null)
        {
            backgroundAnimation = DOTween.Sequence();
            backgroundAnimation.Append(background.DORotate(new Vector3(0, 0, 360), 3f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            backgroundAnimation.SetLoops(-1);
        }
    }

    void Start()
    {
        if (perkTextUI == null)
            perkTextUI = Object.FindFirstObjectByType<TextMeshProUGUI>();

        CollectibleManager.Instance.RegisterCollectible();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collected && collision.TryGetComponent(out PlayerController player))
        {
            collected = true;
            CollectibleManager.Instance.CollectCollectible();
            ApplyPerkToPlayer(player);
            audioManager?.PlaySFX(audioManager.collect);

            string msg = string.IsNullOrEmpty(messageOverride) ? GetPerkMessage(perkType) : messageOverride;
            //perkTextUI.text = msg;

            var particle = ParticleSpawner.Instance?.SpawnParticle("collectible_pickup", transform.position);
            particle.GetComponentInChildren<TMP_Text>().text = msg;

            Destroy(gameObject);
        }
    }

    private void ApplyPerkToPlayer(PlayerController player)
    {
        switch (perkType)
        {
            case PerkType.SpeedBoost:
                player.Stats.MaxSpeed *= 1.1f;
                break;
            case PerkType.JumpBoost:
                player.Stats.JumpPower *= 1.1f;
                break;
            case PerkType.GlideSlowdown:
                player.Stats.glideGravityMultiplier *= 0.9f;
                break;
            case PerkType.DashSpeed:
                player.Stats.dashSpeed *= 1.1f;
                break;
            case PerkType.DashDuration:
                player.Stats.dashDuration *= 1.1f;
                break;
            case PerkType.DashCooldown:
                player.Stats.dashCooldown *= 0.9f;
                break;
        }
    }

    private string GetPerkMessage(PerkType perk)
    {
        return perk switch
        {
            PerkType.SpeedBoost => "Walk Speed +10%",
            PerkType.JumpBoost => "Jump Height +10%",
            PerkType.GlideSlowdown => "Glide Fall Speed -10%",
            PerkType.DashSpeed => "Dash Speed +10%",
            PerkType.DashDuration => "Dash Duration +10%",
            PerkType.DashCooldown => "Dash Cooldown -10%",
            _ => "Perk Collected!"
        };
    }

    private void OnDisable()
    {
        mainAnimation?.Kill();
        backgroundAnimation?.Kill();
    }
}
