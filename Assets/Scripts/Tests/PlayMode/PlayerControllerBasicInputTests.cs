using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class PlayerControllerBasicInputTests
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
    public IEnumerator HandleDirection_MoveRight()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(0.5f);

        Assert.True(player.transform.position.x > 2);
    }

    [UnityTest]
    public IEnumerator HandleDirection_MoveLeft()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(-1, 0)
        };

        yield return new WaitForSeconds(0.5f);

        Assert.True(player.transform.position.x < -2);
    }

    [UnityTest]
    public IEnumerator HandleDirection_MoveSpeedAcceleration()
    {
        stats.Acceleration = 50;
        stats.MaxSpeed = 5;
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };
        yield return new WaitForSeconds(0.025f);
        float speed1 = player.FrameVelocity.x;
        yield return new WaitForSeconds(0.025f);
        float speed2 = player.FrameVelocity.x;

        yield return new WaitForSeconds(1f);

        Assert.True(speed1 < speed2 && speed2 < player.FrameVelocity.x);
    }

    [UnityTest]
    public IEnumerator HandleDirection_MoveSpeedAccelerationIsCapped()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };
        yield return new WaitForSeconds(1f);

        Assert.AreEqual(stats.MaxSpeed, player.FrameVelocity.x);
    }

    [UnityTest]
    public IEnumerator CheckCollisions_OnGround_IsGrounded()
    {
        yield return null;
        Assert.True(player.Grounded);
    }

    [UnityTest]
    public IEnumerator CheckCollisions_InAir_NotGrounded()
    {
        player.transform.position = new Vector3(0, 5, 0);
        yield return new WaitForSeconds(0.1f);
        Assert.False(player.Grounded);
    }

    [UnityTest]
    public IEnumerator ExecuteJump_JumpForceApplied()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return null;
        Assert.AreEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator ExecuteJump_InAir_JumpForceNotApplied()
    {
        player.transform.position = new Vector3(0, 3);
        yield return new WaitForSeconds(0.2f);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return null;
        Assert.AreNotEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleGravity_InAir_Deceleration()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };

        yield return null;
        float speed1 = player.FrameVelocity.y;
        yield return new WaitForSeconds(0.025f);
        float speed2 = player.FrameVelocity.y;
        yield return new WaitForSeconds(0.025f);

        Assert.True(speed1 > speed2 && speed2 > player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator CheckCollisions_AfterJump_IsGrounded()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
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

        yield return new WaitForSeconds(1f);

        Assert.True(player.Grounded);
    }

    [UnityTest]
    public IEnumerator HandleGravity_JumpEndedEarly_IsGrounded()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.1f);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.5f);

        Assert.True(player.Grounded);
    }

    [UnityTest]
    public IEnumerator HandleJump_Jumping_CantJump()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.4f);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.1f);

        float reachedHeight = player.transform.position.y;
        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.2f);

        Assert.True(player.transform.position.y < reachedHeight);
    }

    [UnityTest]
    public IEnumerator StopMovement_ResetsVelocity()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(-1, 0)
        };

        yield return new WaitForSeconds(0.3f);

        player.StopMovement();
        yield return new WaitForSeconds(0.1f);
        Assert.AreEqual(player.FrameVelocity.x, 0);
    }

    [UnityTest]
    public IEnumerator CheckCollision_JumpWhileAboveWall_SetVelocityYTo0()
    {
        TestUtils.CreateWall(new Vector2(0, 2), new Vector2(1, 1));

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

        yield return new WaitForSeconds(0.3f);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(0, 0)
        };

        yield return new WaitForSeconds(0.2f);

        Assert.True(player.Grounded);
    }
}
