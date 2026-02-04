using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Bearing;

public class Transform3D : Transform
{
    private Vector3 _position = Vector3.Zero;
    private Vector3 _eRotation = Vector3.Zero;
    private Quaternion _qRotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;

    public Vector3 position
    {
        get { return _position; }
        set
        {
            _position = value;
            UpdateModel();
            InvokePositionChanged();
            InvokeTransformChanged();
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
            InvokeRotationChanged();
            InvokeTransformChanged();
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
            InvokeRotationChanged();
            InvokeTransformChanged();
        }
    }

    public Vector3 scale
    {
        get { return _scale; }
        set
        {
            _scale = value;
            UpdateModel();
            InvokeScaleChanged();
            InvokeTransformChanged();
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
            model *= Matrix4.CreateScale(((Transform3D)parent).scale);
            model *= Matrix4.CreateFromQuaternion(((Transform3D)parent).qRotation);
            model *= Matrix4.CreateTranslation(((Transform3D)parent).position);
        }
    }

    public Vector3 GetForward()
    {
        Vector3 result = new Vector3(
                model.M13,
                model.M23,
                model.M33
            );

        return result.Normalized();
    }

    public Vector3 GetRight()
    {
        Vector3 result = new Vector3(
                model.M11,
                model.M21,
                model.M31
            );

        return result.Normalized();
    }

    public Vector3 GetUp()
    {
        Vector3 result = new Vector3(
                model.M12,
                model.M22,
                model.M32
            );

        return result.Normalized();
    }

    public Matrix4 GetModelMatrix()
    {
        return model;
    }

    public void FromModel(Matrix4 model, bool triggerTransformChanged = true)
    {
        _position = model.ExtractTranslation(); // using underscore to avoid calling transform changed
        _qRotation = model.ExtractRotation();
        _scale = model.ExtractScale();
        UpdateModel();
        if (triggerTransformChanged)
            InvokeTransformChanged();
    }
}