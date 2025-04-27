using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class PlayerControllerQoLTests
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

    private static IEnumerable BufferedJumpTestCases()
    {
        yield return new TestCase<float, bool> { value1 = 0.2f, value2 = false };
        yield return new TestCase<float, bool> { value1 = 0.1f, value2 = true };
        yield return new TestCase<float, bool> { value1 = 0, value2 = true };
    }

    [UnityTest]
    public IEnumerator HandleJump_JumpPressedBeforeHittingGround_CanJump([ValueSource(nameof(BufferedJumpTestCases))] TestCase<float, bool> testCase)
    {
        float bufferTime = testCase.value1;
        bool result = testCase.value2;
        stats.JumpBuffer = bufferTime;

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };
        yield return null;
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.2f);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.225f);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };
        yield return null;
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.4f);

        Assert.AreEqual(player.Grounded, result);
    }

    private static IEnumerable CoyoteTestCases()
    {
        yield return new TestCase<float, bool> { value1 = 0.2f, value2 = true };
        yield return new TestCase<float, bool> { value1 = 0.1f, value2 = false };
        yield return new TestCase<float, bool> { value1 = 0, value2 = false };
    }

    [UnityTest]
    public IEnumerator HandleJump_CanUseCoyoteTime_CanJump([ValueSource(nameof(CoyoteTestCases))] TestCase<float, bool> testCase)
    {
        float coyoteTime = testCase.value1;
        bool result = testCase.value2;
        stats.CoyoteTime = coyoteTime;
        float startPoint = player.transform.position.y;
        Object.Destroy(floor);
        yield return new WaitForSeconds(0.175f);
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.3f);

        Assert.AreEqual(player.transform.position.y > startPoint, result);
    }
}
