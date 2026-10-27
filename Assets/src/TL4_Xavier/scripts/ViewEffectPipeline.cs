using System.Collections.Generic;

/*
F5 ViewEffectPipeline: holds the chain of active ViewEffects and runs a CameraState through it.

- At most one effect per DistortionID.
- Order is the order effects were added. A new one goes on the end; re-adding an existing id keeps its place.
- Run() is pure: same input state + same chain = same output, no Unity scene access. That makes it unit-testable.
- An empty chain returns the input unchanged (the "identity" test).

DistortionScheduler decides WHICH effects are in here and WHEN; this class only stores and runs them.
*/
public class ViewEffectPipeline
{
    private readonly List<ViewEffect> chain = new List<ViewEffect>();

    // Read-only view of the chain, first effect first.
    public IReadOnlyList<ViewEffect> Chain => chain;

    // The effect with this id, or null if it isn't in the chain.
    public ViewEffect Find(DistortionID id)
    {
        foreach (var e in chain) if (e.Id == id) return e;
        return null;
    }

    // Appends to the end. Does nothing if an effect with the same id is already in the chain.
    public void Add(ViewEffect effect)
    {
        if (Find(effect.Id) != null) return;
        chain.Add(effect);
        Relink();
    }

    // Removes the effect with this id (if any); the others keep their order.
    public void Remove(DistortionID id)
    {
        var e = Find(id);
        if (e == null) return;
        chain.Remove(e);
        Relink();
    }

    // Removes everything at once.
    public void Clear()
    {
        chain.Clear();
    }

    // Sends the base state into the first effect; the chain handles the rest via Next.
    public CameraState Run(CameraState baseState)
    {
        return chain.Count == 0 ? baseState : chain[0].Process(baseState);
    }

    // Points each effect's Next at the one after it (last one gets null).
    private void Relink()
    {
        for (int i = 0; i < chain.Count; i++)
        {
            chain[i].Next = i + 1 < chain.Count ? chain[i + 1] : null;
        }
    }
}
