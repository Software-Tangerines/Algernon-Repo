//
using System.Collections.Generic;

//
public class ViewEffectPipeline
{
    private readonly List<ViewEffect> chain = new List<ViewEffect>();

    public IReadOnlyList<ViewEffect> Chain => chain;

    //
    public ViewEffect Find(DistortionID id)
    {
        foreach (var e in chain) if (e.Id == id) return e;
        return null;
    }

    //new id goes to the END and existing id keeps its positon 
    public void Add(ViewEffect effect)
    {
        if (Find(effect.Id) != null) return;
        chain.Add(effect);
        Relink();
    }

    //
    public void Remove(DistortionID id)
    {
        var e = Find(id);
        if(e == null) return;
        chain.Remove(e);
        Relink();
    }

    //
    public void Clear()
    {
        chain.Clear();
    }

    public CameraState Run(CameraState baseState)
    {
        return chain.Count == 0 ? baseState : chain[0].Process(baseState);
    }

    private void Relink()
    {
        for(int i = 0; i < chain.Count; i++)
        {
            chain[i].Next = i + 1 < chain.Count ? chain[i +1] : null;
        }
    }
}