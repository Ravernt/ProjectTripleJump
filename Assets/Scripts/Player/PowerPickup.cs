using DG.Tweening;
using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    [SerializeField] private AbilityType abilityToUnlock;
    [SerializeField] private Transform visuals;
    [SerializeField] private Transform background;
    AudioManager audioManager;

    Sequence mainAnimation;
    Sequence backgroundAnimation;
    
    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        //Pickup animations
        mainAnimation = DOTween.Sequence();
        mainAnimation.Append(visuals.DOLocalMoveY(0.3f, 2f));
        mainAnimation.Append(visuals.DOLocalMoveY(0f, 2f));
        mainAnimation.SetLoops(-1); //Infinite repeats

        backgroundAnimation = DOTween.Sequence();
        backgroundAnimation.Append(background.DORotate(new(0, 0, 360), 3f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
        backgroundAnimation.SetLoops(-1); //Infinite repeats
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerAbilities player))
        {
            audioManager.PlaySFX(audioManager.collect);
            player.UnlockAbility(abilityToUnlock);
            ParticleSpawner.Instance.SpawnParticle("powerup_pickup", transform.position);
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        mainAnimation.Kill();
        backgroundAnimation.Kill();
    }
}
