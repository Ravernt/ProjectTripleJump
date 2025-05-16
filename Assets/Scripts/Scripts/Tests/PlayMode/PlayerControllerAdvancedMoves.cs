using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using static TestUtils;

public class PlayerControllerAdvancedMoves
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
    public IEnumerator HandleDash_CanDash()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return null;

        Assert.True(player.IsDashing);
    }

    [UnityTest]
    public IEnumerator HandleDash_NotUnlocked_CantDash()
    {
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return null;

        Assert.False(player.IsDashing);
    }

    [UnityTest]
    public IEnumerator HandleDash_NotMoving_CantDash()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);
        yield return null;

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(0, 0)
        };
        yield return null;

        Assert.False(player.IsDashing);
    }

    [UnityTest]
    public IEnumerator Dash_CantChangeDashDirectionMidDash()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };
        yield return new WaitForSeconds(0.05f);

        float changePoint = player.transform.position.x;

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(-1, 0)
        };
        yield return new WaitForSeconds(0.05f);

        Assert.True(changePoint < player.transform.position.x);
    }

    private static IEnumerable DashTimeTestCases()
    {
        yield return new TestCase<float, bool> { value1 = 0.15f, value2 = true };
        yield return new TestCase<float, bool> { value1 = 0.25f, value2 = false };
    }

    [UnityTest]
    public IEnumerator DashCooldownRoutine_DashResetsRightTime([ValueSource(nameof(DashTimeTestCases))] TestCase<float, bool> testCase)
    {
        float pointInTime = testCase.value1;
        bool result = testCase.value2;

        player.Abilities.UnlockAbility(AbilityType.Dash);
        stats.dashDuration = 0.2f;

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };
        yield return null;

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };
        yield return new WaitForSeconds(0.05f);
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };
        yield return new WaitForSeconds(pointInTime - 0.05f);

        Assert.AreEqual(result, player.IsDashing);
    }

    [UnityTest]
    public IEnumerator HandleDash_DashOnCooldown_CantDash()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };
        yield return null;
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(stats.dashDuration + stats.dashCooldown * 0.5f);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };
        yield return null;

        Assert.False(player.IsDashing);
    }

    [UnityTest]
    public IEnumerator HandleDash_CooldownEnded_CanDash()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };
        yield return null;
        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(stats.dashDuration + stats.dashCooldown);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };
        yield return null;

        Assert.True(player.IsDashing);
    }

    [UnityTest]
    public IEnumerator HandleGravity_Dashing_NotAffectedByGravity()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);
        player.transform.position = new(0, 4);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(0.25f * stats.dashDuration);
        float height = player.transform.position.y;
        yield return new WaitForSeconds(0.75f * stats.dashDuration);

        Assert.AreEqual(Math.Round(height, 2), Math.Round(player.transform.position.y, 2));
    }

    [UnityTest]
    public IEnumerator HandleDash_MiddleOfJump_CanDash()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

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
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(0.25f * stats.dashDuration);
        float height = player.transform.position.y;
        yield return new WaitForSeconds(0.75f * stats.dashDuration);

        Assert.AreEqual(Math.Round(height, 2), Math.Round(player.transform.position.y, 2));
    }

    [UnityTest]
    public IEnumerator HandleJump_MiddleOfDash_CantJump()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(0.5f * stats.dashDuration);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(1, 0)
        };
        yield return null;

        Assert.AreNotEqual(stats.JumpPower, player.transform.position.y);
    }

    [UnityTest]
    public IEnumerator JumpAndDashAtTheSameTime_DashTriggers()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return null;
        Assert.True(player.IsDashing);
    }

    [UnityTest]
    public IEnumerator JumpAndDashAtTheSameTime_JumpNotTrigger()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return null;
        Assert.AreEqual(0, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_CanDoubleJump()
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

        Assert.AreEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_DoubleJumpNotUnlocked_CantDoubleJump()
    {
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

        Assert.True(stats.JumpPower > player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_Falling_CanUseDoubleJump()
    {
        player.Abilities.UnlockAbility(AbilityType.DoubleJump);

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
        Assert.AreEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_CanTripleJump()
    {
        player.Abilities.UnlockAbility(AbilityType.DoubleJump);
        player.Abilities.UnlockAbility(AbilityType.TripleJump);

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

        Assert.AreEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_TripleJumpNotUnlocked_CantTripleJump()
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

        Assert.True(stats.JumpPower > player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_DoubleJumpNotUnlocked_CantTripleJump()
    {
        player.Abilities.UnlockAbility(AbilityType.TripleJump);

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

        Assert.AreNotEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleJump_AfterDashing_CanDoubleJump()
    {
        player.Abilities.UnlockAbility(AbilityType.Dash);
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
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return new WaitForSeconds(stats.dashDuration);

        player.FrameInput = new FrameInput
        {
            JumpDown = true,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(1, 0)
        };

        yield return null;

        Assert.AreEqual(stats.JumpPower, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleGlide_InAir_CanGlide()
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

        yield return null;

        Assert.True(player.IsGliding);
    }

    [UnityTest]
    public IEnumerator HandleGlide_InAirButNotUnlocked_CantGlide()
    {
        player.transform.position = new Vector3(0, 4, 0);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };

        yield return null;

        Assert.False(player.IsGliding);
    }

    [UnityTest]
    public IEnumerator HandleGravity_LowerGravityWhileGliding()
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

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(player.FrameVelocity.y, -stats.glideFallSpeed);
    }


    [UnityTest]
    public IEnumerator HandleGlide_NotHoldingButton_GlideCanceled()
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

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = Vector2.zero
        };

        yield return null;

        Assert.False(player.IsGliding);
    }

    [UnityTest]
    public IEnumerator HandleDash_Gliding_CanDash()
    {
        player.transform.position = new Vector3(0, 4, 0);
        player.Abilities.UnlockAbility(AbilityType.Glide);
        player.Abilities.UnlockAbility(AbilityType.Dash);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = Vector2.zero
        };

        yield return new WaitForSeconds(0.5f);

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = true,
            Move = new Vector2(1, 0)
        };

        yield return null;
        yield return null;

        Assert.False(player.IsGliding);
    }

    [UnityTest]
    public IEnumerator HandleWallSlide_CanWallSlide()
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

        Assert.True(player.IsWallSliding);
    }

    [UnityTest]
    public IEnumerator HandleGravity_WallSliding_LowerGravity()
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

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(-stats.wallSlideSpeed, player.FrameVelocity.y);
    }

    [UnityTest]
    public IEnumerator HandleWallSlide_MoveAwayWall_WallSlideCancel()
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

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(1, 0)
        };

        yield return null;

        Assert.True(!player.IsWallSliding);
    }

    [UnityTest]
    public IEnumerator HandleDash_WallSliding_CanDash()
    {
        player.transform.position = new Vector3(0, 5, 0);
        player.Abilities.UnlockAbility(AbilityType.WallJump);
        player.Abilities.UnlockAbility(AbilityType.Dash);

        TestUtils.CreateWall(new Vector2(-stats.wallCheckDistance - 0.5f, 0), new Vector2(1, 10));

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(-1, 0)
        };

        yield return null;
        yield return null;
        yield return null;

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = true,
            Move = new Vector2(-1, 0)
        };

        yield return null;
        yield return null;

        Assert.False(player.IsWallSliding);
    }


    [UnityTest]
    public IEnumerator HandleWallSlide_Gliding_CanWallSlide()
    {
        player.transform.position = new Vector3(0, 4, 0);
        player.Abilities.UnlockAbility(AbilityType.WallJump);
        player.Abilities.UnlockAbility(AbilityType.Glide);

        TestUtils.CreateWall(new Vector2(-stats.wallCheckDistance - 1f, 4), new Vector2(1, 10));

        player.FrameInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = true,
            DashDown = false,
            Move = new Vector2(-1, 0)
        };

        yield return new WaitForSeconds(0.5f);

        Assert.True(player.IsWallSliding);
    }
}
