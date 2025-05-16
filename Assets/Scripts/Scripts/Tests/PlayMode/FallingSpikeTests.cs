using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class FallingSpikeTests
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
        yield return new WaitForSeconds(0.2f);
    }

    [UnityTest]
    public IEnumerator Update_PlayerIsBelowSpike_SpikeTriggers()
    {
        GameObject spike = GameObject.Instantiate(Resources.Load<GameObject>("FallingSpike"), new Vector3(0, 5), Quaternion.Euler(0, 0, 180));
        yield return new WaitForSeconds(0.1f);

        Assert.True(spike.GetComponentInChildren<FalingSpike>().hasPlayed);
    }

    [UnityTest]
    public IEnumerator Update_PlayerIsNotBelowSpike_SpikeNotTriggers()
    {
        GameObject spike = GameObject.Instantiate(Resources.Load<GameObject>("FallingSpike"), new Vector3(1, 5), Quaternion.Euler(0, 0, 180));
        yield return new WaitForSeconds(0.1f);

        Assert.False(spike.GetComponentInChildren<FalingSpike>().hasPlayed);
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter2D_HitPlayer_SpikeGetsDestroyed()
    {
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("FallingSpike"), new Vector3(0, 2), Quaternion.Euler(0, 0, 180));
        var spike = obj.GetComponentInChildren<FalingSpike>();

        yield return new WaitForSeconds(0.7f);

        Assert.True(spike.Destroyed);
    }
}
