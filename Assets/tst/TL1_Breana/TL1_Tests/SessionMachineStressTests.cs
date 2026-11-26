using System;
using NUnit.Framework;

public class SessionMachineStressTests
{
    [Test]
    public void TenThousandRandomTransitions_NeverReachAnUndefinedState()
    {
        // 1. A new SessionMachine, and a System.Random with a FIXED seed (e.g. 12345)
        SessionMachine StressSessionMachine = new SessionMachine();
        System.Random RandomNumber = new Random(12345);
        // 2. Get every SessionState value into an array
        //Size of array equals the amount of session states
        SessionState[] states = (SessionState[])Enum.GetValues(typeof(SessionState));
        // 3. Loop 10,000 times:
        //      - pick a random target state
        //      - remember the state before
        //      - call TransitionTo(target) and keep the bool it returns
        //      - assert: the current State is a defined SessionState value
        //      - assert: if it returned true,  State == target
        //                if it returned false, State == the state before (nothing changed)
        int AcceptedCount = 0;
        for(int i = 0; i < 10000; i++)
        {
            SessionState target = states[RandomNumber.Next(states.Length)];
            SessionState before = StressSessionMachine.State;
            bool accepted = StressSessionMachine.TransitionTo(target);
            if (accepted)
            {
                Assert.AreEqual(target, StressSessionMachine.State, $"step {i}");
            } else
            {
                Assert.AreEqual(before, StressSessionMachine.State, $"step {i}");
            }
            Assert.IsTrue(Enum.IsDefined(typeof(SessionState),StressSessionMachine.State), $"step {i}");
            if (accepted)
            {
                AcceptedCount++;
            }
        }
        Assert.Greater(AcceptedCount, 0, "No transitions were accepted, the test proved nothing");

        // 4. After the loop: assert at least some transitions were accepted
        //    (otherwise the test could pass by rejecting everything)
    }
}