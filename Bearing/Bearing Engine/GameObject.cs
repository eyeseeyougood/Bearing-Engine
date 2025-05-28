using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class GameObject
{
    public string name;

    public Transform3D transform = new Transform3D();

    public List<GameObject> immediateChildren = new List<GameObject>();

    private GameObject _parent;

    public GameObject()
    {
        transform.onTransformChanged += OnTransformChanged;
    }

    ~GameObject()
    {
        Cleanup();
    }

    public GameObject parent
    {
        get { return _parent; }
        set {
            if (_parent != null)
                _parent.immediateChildren.Remove(this);
            _parent = value;
            if (value == null) { return; }
            _parent.immediateChildren.Add(this);
            transform.parent = value.transform;
            OnParentChanged();
        }
    }

    public void Tick()
    {
    }

    public virtual void Load()
    {

    }

    public virtual void Cleanup()
    {
        transform.onTransformChanged -= OnTransformChanged;
    }

    protected virtual void OnParentChanged() { }

    protected virtual void OnTransformChanged() { }
}