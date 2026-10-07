/* 
GENERAL ABOUT

Chain of responsibility handler. the pipline only ever holds ViewAEffect 
references, so which process() runs is decided by dynamic binding 

Dynamic-biding demo: remove 'virtual' here (and change 'override' to 'new' in the subclasses so it compiles)
Every call then binds to THIS method
which only fowards; the whole chain becomes a pass-through.
*/

/*
This is the base class for every distortion(mirror, Rotation, Blackout, ect...) its abstract so you can never write new ViewEffect() only MirrorViewEffect() and the effects. the pipeline stores everything as viewEffect references which is what lets it treat all the effects the same way 
*/
public abstract class ViewEffect
{

    /*
    This is the "chain" in the chain of respobsibilty gof im using. each effect holds a reference to the effect after it, sorta like a linked list. the last effect in the chain has NEXT = null. I dont set this viewEffectPipeline.relink() wires it up whenver an efect is added or removed

    Each subclass has to say which distortion it is, for example mirrorViewEffect will return DistortionID.Mirror, since its abstract. the compiler force the subclasses to fill it in. the pipeline uses it in Find() so that applyDistorition(mirror... ) can check whether a mirror effect is already being used in the chain
    */
    public ViewEffect Next {get; set;}
    public abstract DistortionID Id {get;}


    /*
    Target and current are for ramping up the distortion effects that feature 1 asks for . current is where the effect level is at currently and target is the target level  the range is from 0-1 ie(0<=range<1) like 0.8  the scheduler will mover currents value towards targets a little over each frame instead of snapping it on immeditratly 
    */
    public float Target {get; set;}
    public float Current {get; set;} // ramped toward Target by the shcedular in task 9

    protected float I => UnityEngine.Mathf.Clamp01(Current); // returns current, range [0,1] 



    /*
    this method is the core of the class. the base version passes the state along, uf theres is an effect the is passes that state to this method and if its the end of the chain it returns the state as a final result

    subclasses override it with the same pattern each time.
    */
    public virtual CameraState Process(CameraState state)
    {
        return Next != null ? Next.Process(state) : state;
    }
}