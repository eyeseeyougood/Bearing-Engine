namespace Bearing;

public class Transform
{
    private Transform? _parent;

    [HideFromInspector]
    public Transform? parent
    {
        get { return _parent; }
        set
        {
            _parent = value;
        }
    }

    public delegate void OnTransformChanged();
    public event OnTransformChanged onTransformChanged = ()=>{};
    public event OnTransformChanged onPositionChanged = ()=>{};
    public event OnTransformChanged onRotationChanged = ()=>{};
    public event OnTransformChanged onScaleChanged = ()=>{};

    protected void InvokeTransformChanged() { onTransformChanged.Invoke(); }
    protected void InvokePositionChanged() { onPositionChanged.Invoke(); }
    protected void InvokeRotationChanged() { onRotationChanged.Invoke(); }
    protected void InvokeScaleChanged() { onScaleChanged.Invoke(); }

    public virtual void Cleanup()
    {
        if (onTransformChanged != null)
        {
            Delegate[] subscribers = onTransformChanged.GetInvocationList();
            foreach (var d in subscribers)
                onTransformChanged -= d as OnTransformChanged;
        }
    }
}