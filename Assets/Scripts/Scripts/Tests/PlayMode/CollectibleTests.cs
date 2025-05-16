using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class CollectibleTests
{
    PlayerController player;
    ScriptableStats stats;
    GameObject floor;

    [UnitySetUp]
    public IEnumerator Set()
    {
        CreateNewScene();
        yield return null;
        (player, floor) = TestUtils.Set();
        stats = player.Stats;

        var obj = new GameObject("Manager");
        obj.AddComponent<CollectibleManager>();

        yield return new WaitForSeconds(0.2f);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_CollectibleCollect_SpeedBoost()
    {
        CollectibleManager.Instance.Current = 0;
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Collectible"), new Vector3(0, 0), Quaternion.identity);
        obj.GetComponent<Collectible>().PerkType = PerkType.SpeedBoost;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(1, CollectibleManager.Instance.Current);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_CollectibleCollect_DashSpeed()
    {
        CollectibleManager.Instance.Current = 0;
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Collectible"), new Vector3(0, 0), Quaternion.identity);
        obj.GetComponent<Collectible>().PerkType = PerkType.DashSpeed;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(1, CollectibleManager.Instance.Current);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_CollectibleCollect_JumpBoost()
    {
        CollectibleManager.Instance.Current = 0;
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Collectible"), new Vector3(0, 0), Quaternion.identity);
        obj.GetComponent<Collectible>().PerkType = PerkType.JumpBoost;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(1, CollectibleManager.Instance.Current);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_CollectibleCollect_DashCooldown()
    {
        CollectibleManager.Instance.Current = 0;
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Collectible"), new Vector3(0, 0), Quaternion.identity);
        obj.GetComponent<Collectible>().PerkType = PerkType.DashCooldown;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(1, CollectibleManager.Instance.Current);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_CollectibleCollect_DashDuration()
    {
        CollectibleManager.Instance.Current = 0;
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Collectible"), new Vector3(0, 0), Quaternion.identity);
        obj.GetComponent<Collectible>().PerkType = PerkType.DashDuration;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(1, CollectibleManager.Instance.Current);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_CollectibleCollect_GlideSlideDown()
    {
        CollectibleManager.Instance.Current = 0;
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Collectible"), new Vector3(0, 0), Quaternion.identity);
        obj.GetComponent<Collectible>().PerkType = PerkType.GlideSlowdown;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(1, CollectibleManager.Instance.Current);
    }
}
