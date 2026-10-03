using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class nickTL2testScript
{
    [Test]
    public void BaseRef_ToSubclass_UsesOverride(){
        MazeAlgorithm baseline = new MazeAlgorithm();
        MazeAlgorithm actual = new BraidedAlgo();
        string a = baseline.PostProcess();
        string b = actual.PostProcess();
        
        Assert.AreNotEqual(a, b); // assert: DIFFERENT​
    }

    [Test]
    public void BaseRef_ToBase_NoBindingChange()
    {
        MazeAlgorithm baseline = new MazeAlgorithm(); 
        MazeAlgorithm actual = new MazeAlgorithm();
        Assert.AreEqual(baseline.PostProcess(), actual.PostProcess());
    }
}
