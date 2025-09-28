using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class Transform3D
{
    private Vector3 _position = Vector3.Zero;
    private Vector3 _eRotation = Vector3.Zero;
    private Quaternion _qRotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;
    private Transform3D _parent;

    public delegate void OnTransformChanged();
    public event OnTransformChanged onTransformChanged = ()=>{};

    [HideFromInspector]
    public Transform3D parent
    {
        get { return _parent; }
        set
        {
            _parent = value;

        }
    }

    public Vector3 position
    {
        get { return _position; }
        set
        {
            _position = value;
            UpdateModel();
            onTransformChanged.Invoke();
        }
    }
    public Vector3 eRotation
    {
        get { return _eRotation; }
        set
        {

            _eRotation = value;
            _qRotation = Quaternion.FromEulerAngles(_eRotation * MathHelper.DegToRad);
            UpdateModel();
            onTransformChanged.Invoke();
        }
    }

    [HideFromInspector]
    public Quaternion qRotation
    {
        get { return _qRotation; }
        set
        {
            _qRotation = value;
            _eRotation = _qRotation.ToEulerAngles();
            UpdateModel();
            onTransformChanged.Invoke();
        }
    }
    public Vector3 scale
    {
        get { return _scale; }
        set
        {
            _scale = value;
            UpdateModel();
            onTransformChanged.Invoke();
        }
    }

    private Matrix4 model = Matrix4.Identity;

    private void UpdateModel()
    {
        model = Matrix4.CreateScale(scale);
        model *= Matrix4.CreateFromQuaternion(qRotation);
        model *= Matrix4.CreateTranslation(position);
        if (parent != null)
        {
            model *= Matrix4.CreateScale(parent.scale);
            model *= Matrix4.CreateFromQuaternion(parent.qRotation);
            model *= Matrix4.CreateTranslation(parent.position);
        }
    }

    public Vector3 GetForward()
    {
        return model.Row2.Xyz.Normalized();
    }

    public Vector3 GetRight()
    {
        return model.Row0.Xyz.Normalized();
    }

    public Vector3 GetUp()
    {
        return model.Row1.Xyz.Normalized();
    }

    public Matrix4 GetModelMatrix()
    {
        return model;
    }

    public void FromModel(Matrix4 model)
    {
        _position = model.ExtractTranslation(); // using underscore to avoid calling transform changed
        _qRotation = model.ExtractRotation();
        _scale = model.ExtractScale();
        UpdateModel();
    }

    public void Cleanup()
    {
        if (onTransformChanged != null)
        {
            Delegate[] subscribers = onTransformChanged.GetInvocationList();
            foreach (var d in subscribers)
                onTransformChanged -= d as OnTransformChanged;
        }
    }
}