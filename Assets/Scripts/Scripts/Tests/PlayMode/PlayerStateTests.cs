using System.Collections;
using System.Drawing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class PlayerStateTests
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
    public IEnumerator GetCurrentState_Idle()
    {
        yield return null;
        Assert.AreEqual(PlayerState.Idle, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_Running()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(-1, 0)
        };

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(PlayerState.Running, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_Falling()
    {
        player.transform.position = new(0, 5, 0);

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(PlayerState.Falling, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_Jumping()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return null;
        Assert.AreEqual(PlayerState.Jumping, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_Gliding()
    {
        player.transform.position = new Vector3(0, 4, 0);
        player.Abilities.UnlockAbility(AbilityType.Glide);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(PlayerState.Gliding, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_Dashing()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(PlayerState.Dashing, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_DoubleJump()
    {
        player.Abilities.UnlockAbility(AbilityType.DoubleJump);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };
        yield return null;
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };
        yield return new WaitForSeconds(0.2f);
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };
        yield return null;

        Assert.AreEqual(PlayerState.DoubleJumping, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_WallJumpLeft()
    {
        player.transform.position = new Vector3(0, 5, 0);
        player.Abilities.UnlockAbility(AbilityType.WallJump);

        TestUtils.CreateWall(new Vector2(-stats.wallCheckDistance - 0.5f, 0), new Vector2(1, 10));

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(-1, 0)
        };

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(PlayerState.WallSlidingLeft, player.GetCurrentState());
    }

    [UnityTest]
    public IEnumerator GetCurrentState_WallJumpRight()
    {
        player.transform.position = new Vector3(0, 5, 0);
        player.Abilities.UnlockAbility(AbilityType.WallJump);

        TestUtils.CreateWall(new Vector2(stats.wallCheckDistance + 0.5f, 0), new Vector2(1, 10));

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(PlayerState.WallSlidingRight, player.GetCurrentState());
    }
}
