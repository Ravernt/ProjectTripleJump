using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    [SerializeField] private AbilityType abilityToUnlock;
    AudioManager audioManager;
    
    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerAbilities player))
        {
            audioManager.PlaySFX(audioManager.collect);
            player.UnlockAbility(abilityToUnlock);
            Destroy(gameObject);
        }
    }
}
