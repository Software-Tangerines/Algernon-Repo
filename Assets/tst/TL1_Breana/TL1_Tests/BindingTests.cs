using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BindingTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void  BaseRef_ToSubclass_UsesOverride()
    {
        LevelRule baseline = new LevelRule();
        LevelRule actual = new PuzzleCompletionRule();
        string a = baseline.Evaluate();
        string b = actual.Evaluate();
        
        Assert.AreNotEqual(a, b); // assert: DIFFERENT
    }
    [Test]
    public void BaseRef_ToBase_NoBindingChange()
    {
        LevelRule baseline = new LevelRule();
        LevelRule actual = new LevelRule();

        Assert.AreNotEqual(baseline.Evaluate(), baseline.Evaluate());
    }
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator BindingTestsWithEnumeratorPasses()
    //{
        //Item baseline = new Item(); 
        //Item actual = new Item();
        //Assert.AreNotEqual(baseline.GetItemId(), actual.GetItemId());
        //Assert.AreEqual(baseline.GetItemId(), actual.GetItemId());
        // Run 1: RED - same output, nothing to bind to
        // Run 2: change to Assert.AreEqual -> GREEN -> commit
        //yield return null;
    //}
}
