using System.Collections.Generic;
using NUnit.Framework;

public class SessionMachineTests
{
    // Helper: returns a machine already in the target state, reached only through legal transitions.
    // Fails the test immediately if any step along the path is rejected.
    private static SessionMachine MachineIn(SessionState target)
    {
        var paths = new Dictionary<SessionState, SessionState[]>
        {
            { SessionState.Boot,         new SessionState[0] },
            { SessionState.Menu,         new[] { SessionState.Menu } },
            { SessionState.LevelLoading, new[] { SessionState.Menu, SessionState.LevelLoading } },
            { SessionState.Playing,      new[] { SessionState.Menu, SessionState.LevelLoading, SessionState.Playing } },
            { SessionState.Paused,       new[] { SessionState.Menu, SessionState.LevelLoading, SessionState.Playing, SessionState.Paused } },
            { SessionState.Won,          new[] { SessionState.Menu, SessionState.LevelLoading, SessionState.Playing, SessionState.Won } },
            { SessionState.Lost,         new[] { SessionState.Menu, SessionState.LevelLoading, SessionState.Playing, SessionState.Lost } },
            { SessionState.RunComplete,  new[] { SessionState.Menu, SessionState.LevelLoading, SessionState.Playing, SessionState.Won, SessionState.RunComplete } },
        };

        var machine = new SessionMachine();
        foreach (var step in paths[target])
        {
            Assert.IsTrue(machine.TransitionTo(step), $"Setup failed: could not reach {step} on the way to {target}");
        }
        return machine;
    }

    // 1. Starting state matches the Champion doc
    [Test]
    public void NewMachine_StartsInBoot()
    {
        var machine = new SessionMachine();

        Assert.AreEqual(SessionState.Boot, machine.State);
    }

    // 2. Basic legal transition
    [Test]
    public void Boot_ToMenu_ReturnsTrue_AndChangesState()
    {
        var machine = new SessionMachine();

        bool result = machine.TransitionTo(SessionState.Menu);

        Assert.IsTrue(result);
        Assert.AreEqual(SessionState.Menu, machine.State);
    }

    // 3. Can't skip LevelLoading
    [Test]
    public void Menu_ToPlaying_IsRejected()
    {
        var machine = MachineIn(SessionState.Menu);

        bool result = machine.TransitionTo(SessionState.Playing);

        Assert.IsFalse(result);
        Assert.AreEqual(SessionState.Menu, machine.State);
    }

    // 4. Nothing ever returns to Boot (one test case per state)
    [TestCase(SessionState.Boot)]
    [TestCase(SessionState.Menu)]
    [TestCase(SessionState.LevelLoading)]
    [TestCase(SessionState.Playing)]
    [TestCase(SessionState.Paused)]
    [TestCase(SessionState.Won)]
    [TestCase(SessionState.Lost)]
    [TestCase(SessionState.RunComplete)]
    public void AnyState_ToBoot_IsRejected(SessionState from)
    {
        var machine = MachineIn(from);

        Assert.IsFalse(machine.TransitionTo(SessionState.Boot));
        Assert.AreEqual(from, machine.State);
    }

    // 5. Pause round trip
    [Test]
    public void Playing_PauseAndResume_BothSucceed()
    {
        var machine = MachineIn(SessionState.Playing);

        Assert.IsTrue(machine.TransitionTo(SessionState.Paused));
        Assert.IsTrue(machine.TransitionTo(SessionState.Playing));
        Assert.AreEqual(SessionState.Playing, machine.State);
    }

    // 6. A level can't end while paused (the "timer wins" rule)
    [TestCase(SessionState.Won)]
    [TestCase(SessionState.Lost)]
    public void Paused_CannotEndLevel(SessionState next)
    {
        var machine = MachineIn(SessionState.Paused);

        Assert.IsFalse(machine.TransitionTo(next));
        Assert.AreEqual(SessionState.Paused, machine.State);
    }

    // 7. Pause menu can restart or quit
    [TestCase(SessionState.LevelLoading)]
    [TestCase(SessionState.Menu)]
    public void Paused_CanRestartOrQuit(SessionState next)
    {
        var machine = MachineIn(SessionState.Paused);

        Assert.IsTrue(machine.TransitionTo(next));
    }

    // 8. Death screen: reset or quit to menu
    [TestCase(SessionState.LevelLoading)]
    [TestCase(SessionState.Menu)]
    public void Lost_CanRestartOrQuit(SessionState next)
    {
        var machine = MachineIn(SessionState.Lost);

        Assert.IsTrue(machine.TransitionTo(next));
    }

    // 9. Win on the last level leads to RunComplete, then back to Menu
    [Test]
    public void Won_ToRunComplete_ToMenu()
    {
        var machine = MachineIn(SessionState.Won);

        Assert.IsTrue(machine.TransitionTo(SessionState.RunComplete));
        Assert.IsTrue(machine.TransitionTo(SessionState.Menu));
    }

    // 10. Transitioning to the state you're already in is rejected
    [Test]
    public void SameState_IsRejected()
    {
        var machine = MachineIn(SessionState.Playing);

        Assert.IsFalse(machine.TransitionTo(SessionState.Playing));
    }
}
