/*
F5 ViewEffect: abstract base class for every distortion (Chain of Responsibility handler).

Each effect is one link in a chain, like a linked list:
    Mirror -> Rotation -> Blackout -> null
ViewEffectPipeline.Run() hands a CameraState to the first link. Each link changes the state
and calls base.Process(s), which forwards it to Next. The last link returns the final state.
A link may also STOP the chain by returning without calling base (BlackoutViewEffect does this).
That stopping is why this is Chain of Responsibility and not Decorator.

Plain C# class on purpose, NOT a MonoBehaviour: effects are created with `new`, can be chained
in any order at runtime, and can be unit tested without a scene.

DYNAMIC BINDING (oral exam demo):
The pipeline only holds ViewEffect references, so which Process() runs is decided at runtime
by the object's real type. Remove `virtual` below (and change `override` to `new` in the subclasses
so it still compiles) and every call binds to THIS method, which only forwards: the whole chain
becomes a pass-through and all distortions vanish, while game logic is unaffected.

To add a new effect: subclass this, return its DistortionID from Id, override Process
(change the state, then `return base.Process(s);`), and add it to DistortionScheduler.CreateEffect.
*/
public abstract class ViewEffect
{
    // The next link in the chain (null = last link). Don't set this yourself;
    // ViewEffectPipeline.Relink() rewires it whenever an effect is added or removed.
    public ViewEffect Next { get; set; }

    // Which distortion this is. Abstract, so every subclass is forced to say.
    // The pipeline uses it in Find() so applying Mirror twice updates the existing effect instead of adding a second.
    public abstract DistortionID Id { get; }

    // Ramping: Target is the intensity asked for (0..1). Current is where the effect is right now.
    // DistortionScheduler.Tick moves Current toward Target a bit each frame, so effects fade in/out
    // instead of snapping. Effects read Current (through I), never Target.
    public float Target { get; set; }
    public float Current { get; set; }

    // Current intensity, clamped to [0, 1]. Use this inside Process.
    protected float I => UnityEngine.Mathf.Clamp01(Current);

    // Default behaviour: pass the state on unchanged, or return it if this is the last link.
    public virtual CameraState Process(CameraState state)
    {
        return Next != null ? Next.Process(state) : state;
    }
}
