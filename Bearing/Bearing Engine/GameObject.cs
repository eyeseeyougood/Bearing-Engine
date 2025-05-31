using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class GameObject
{
    public string name { get; set; }

    public Transform3D transform { get; set; }

    public List<GameObject> immediateChildren { get; set; }

    public List<Component> components { get; set; }

    private GameObject _parent;

    public GameObject() { }

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
        foreach (Component c in components)
        {
            c.OnTick();
        }
    }

    public virtual void Load()
    {
        immediateChildren ??= new List<GameObject>();
        components ??= new List<Component>();

        transform.onTransformChanged += OnTransformChanged;

        foreach (GameObject child in immediateChildren)
        {
            child._parent = this;
            child.transform.parent = transform;
            child.OnParentChanged();
            child.Load();
        }
        foreach (Component c in components)
        {
            c.gameObject = this;
            c.OnLoad();
        }
    }

    public Component? GetComponent(Type type)
    {
        Component? result = null;

        foreach (Component comp in components)
        {
            if (comp.GetType() == type)
            {
                result = comp;
                break;
            }
        }

        return result;
    }

    public void AddComponent(Component component, bool load = true)
    {
        components.Add(component);
        if (!load)
            return;

        component.gameObject = this;
        component.OnLoad();
    }

    public void RemoveComponent(Component component)
    {
        component.Cleanup();
        components.Remove(component);
    }

    public bool HasComponent(Type type)
    {
        bool result = false;

        foreach (Component comp in components)
        {
            if (comp.GetType() == type)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    public virtual void Cleanup()
    {
        transform.onTransformChanged -= OnTransformChanged;
        foreach (Component c in components)
        {
            RemoveComponent(c);
        }
    }

    protected virtual void OnParentChanged() { }

    protected virtual void OnTransformChanged() { }
}