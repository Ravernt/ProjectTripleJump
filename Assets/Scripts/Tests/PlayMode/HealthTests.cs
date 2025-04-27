using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class HealthTests
{
    PlayerController player;
    Health health;
    ScriptableStats stats;
    GameObject floor;

    [UnitySetUp]
    public IEnumerator Set()
    {
        CreateNewScene();
        yield return null;
        (player, floor) = TestUtils.Set();
        health = player.GetComponent<Health>();
        stats = player.Stats;
        yield return new WaitForSeconds(0.2f);
    }

    private static IEnumerable TakeDamageTestCases()
    {
        yield return new TestCase<int, int> { value1 = 1, value2 = 2 };
        yield return new TestCase<int, int> { value1 = 2, value2 = 1 };
        yield return new TestCase<int, int> { value1 = 3, value2 = 0 };
    }

    [UnityTest]
    public IEnumerator TakeDamage_BasicValues([ValueSource(nameof(TakeDamageTestCases))] TestCase<int, int> testCase)
    {
        yield return null;
        health.MaxHealth = 3;
        health.CurrentHealth = 3;
        health.TakeDamage(testCase.value1);
        Assert.AreEqual(testCase.value2, health.CurrentHealth);
    }

    private static IEnumerable TakeDamageDeadTestCases()
    {
        yield return new TestCase<int, bool> { value1 = 1, value2 = false };
        yield return new TestCase<int, bool> { value1 = 3, value2 = true };
        yield return new TestCase<int, bool> { value1 = 5, value2 = true };
    }

    [UnityTest]
    public IEnumerator TakeDamage_LethalAmountOfDamage([ValueSource(nameof(TakeDamageDeadTestCases))] TestCase<int, bool> testCase)
    {
        yield return null;
        health.MaxHealth = 3;
        health.CurrentHealth = 3;
        health.TakeDamage(testCase.value1);
        Assert.AreEqual(testCase.value2, health.Dead);
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhileDead_HealthDoesntChange()
    {
        health.MaxHealth = 3;
        health.CurrentHealth = 3;
        health.Dead = true;
        health.TakeDamage(1);
        yield return null;
        Assert.AreEqual(health.CurrentHealth, health.MaxHealth);
    }

    [UnityTest]
    public IEnumerator TakeDamage_InvincibilityTriggers()
    {
        health.MaxHealth = 3;
        health.CurrentHealth = 3;
        health.InvincibilityTime = 0.2f;
        health.TakeDamage(1);
        yield return null;
        Assert.True(health.Invincible);
    }

    [UnityTest]
    public IEnumerator TakeDamage_WhileInvincible_HealthDoesntChange()
    {
        health.MaxHealth = 3;
        health.CurrentHealth = 3;
        health.Invincible = true;
        health.TakeDamage(1);
        yield return null;
        Assert.AreEqual(health.CurrentHealth, health.MaxHealth);
    }

    [UnityTest]
    public IEnumerator Invincibility_InvincibilityRunsOut()
    {
        health.MaxHealth = 3;
        health.CurrentHealth = 3;
        health.InvincibilityTime = 0.2f;
        health.TakeDamage(1);
        yield return new WaitForSeconds(0.2f);
        Assert.False(health.Invincible);
    }

    [UnityTest]
    public IEnumerator BlackoutAndRespawn_HealthResets()
    {
        health.MaxHealth = 3;
        health.CurrentHealth = 0;
        health.Dead = true;

        yield return player.StartCoroutine(health.BlackoutAndRespawn());
        Assert.False(health.Dead);
        Assert.AreEqual(health.CurrentHealth, health.MaxHealth);
    }
}
