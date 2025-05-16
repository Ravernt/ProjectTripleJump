using System;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType
{
    Dash,
    DoubleJump,
    Glide,
    WallJump,
    TripleJump
}


public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] private bool unlockAllAbilitiesAtStart = false;
    [SerializeField] private bool activateCheats = false;

    Dictionary<AbilityType, bool> unlockedAbilities = new Dictionary<AbilityType, bool>
    {
        { AbilityType.Dash, false },
        { AbilityType.DoubleJump, false },
        { AbilityType.Glide, false },
        { AbilityType.WallJump, false },
        { AbilityType.TripleJump, false }
    };

    public bool HasAbility(AbilityType type) => unlockedAbilities.ContainsKey(type) && unlockedAbilities[type];

    void Awake()
    {
        //if (unlockAllAbilitiesAtStart)
        //{
            //foreach (AbilityType ability in Enum.GetValues(typeof(AbilityType)))
            //{
            //    UnlockAbility(ability);
            //}
        //}
    }

    void Update()
    {
        if (!activateCheats)
            return;

        // Press number keys 1–6 (top row) to toggle abilities
        //if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleAbility(AbilityType.Dash);
        //if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleAbility(AbilityType.DoubleJump);
        //if (Input.GetKeyDown(KeyCode.Alpha3)) ToggleAbility(AbilityType.Glide);
        //if (Input.GetKeyDown(KeyCode.Alpha4)) ToggleAbility(AbilityType.WallJump);
        //if (Input.GetKeyDown(KeyCode.Alpha5)) ToggleAbility(AbilityType.TripleJump);
    }

    public void UnlockAbility(AbilityType type)
    {
        if (unlockedAbilities.ContainsKey(type))
        {
            unlockedAbilities[type] = true;
            Debug.Log($"[PICKUP] Unlocked ability: {type}");
        }
    }

        /*
    public void ToggleAbility(AbilityType type)
    {
        if (unlockedAbilities.ContainsKey(type))
        {
            unlockedAbilities[type] = !unlockedAbilities[type];
            Debug.Log($"{type} is now {(unlockedAbilities[type] ? "Enabled" : "Disabled")}");
        }
    }
        */

}
