using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    [SerializeField] private AbilityType abilityToUnlock;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerAbilities player))
        {
            player.UnlockAbility(abilityToUnlock);
            Destroy(gameObject);
        }
    }
}
