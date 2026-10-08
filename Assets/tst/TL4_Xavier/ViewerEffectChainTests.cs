using NUnit.Framework;
using UnityEngine;

// EditMode tests 
public class ViewEffectChainTests
{
    class SpyEffect : ViewEffect
    {
        public int calls;
        public override DistortionID Id => DistortionID.Fog; // any unused id
        public override CameraState Process(CameraState s) { calls++; return base.Process(s); }
    }

    static CameraState Base() => CameraState.Default(Vector2.zero, 5f);

    [Test]
    public void IdentityChain_EmptyPipelineReturnsInputUnchanged()
    {
        var p = new ViewEffectPipeline();
        var s = Base();
        var r = p.Run(s);
        Assert.AreEqual(s.ViewMatrix, r.ViewMatrix);
        Assert.AreEqual(s.saturation, r.saturation);
        Assert.AreEqual(s.blackedOut, r.blackedOut);
    }

    [Test]
    public void Blackout_HaltsChain()
    {
        var p = new ViewEffectPipeline();
        p.Add(new BlackoutViewEffect { Current = 1f });
        var spy = new SpyEffect();
        p.Add(spy);

        for (int f = 0; f < 1000; f++)
            Assert.IsTrue(p.Run(Base()).blackedOut);

        Assert.AreEqual(0, spy.calls);
    }

    [Test]
    public void OrderMatters_RotationThenMirror_vs_MirrorThenRotation()
    {
        var a = new ViewEffectPipeline();
        a.Add(new RotationViewEffect { Current = 0.25f });
        a.Add(new MirrorViewEffect { Current = 1f });

        var b = new ViewEffectPipeline();
        b.Add(new MirrorViewEffect { Current = 1f });
        b.Add(new RotationViewEffect { Current = 0.25f });

        Vector3 pa = a.Run(Base()).distortion.MultiplyPoint3x4(new Vector3(1, 0, 0));
        Vector3 pb = b.Run(Base()).distortion.MultiplyPoint3x4(new Vector3(1, 0, 0));

        Assert.That(Vector2.Distance(pa, new Vector2(0, 1)), Is.LessThan(1e-4f));
        Assert.That(Vector2.Distance(pb, new Vector2(0, -1)), Is.LessThan(1e-4f));
        Assert.That(Vector2.Distance(pa, pb), Is.GreaterThanOrEqualTo(1.9f));
    }
}