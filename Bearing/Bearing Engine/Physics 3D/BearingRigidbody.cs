using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using BulletSharp;
using SkiaSharp;
using OpenTK.Mathematics;

namespace Bearing;

[RequireComponent(typeof(MeshRenderer))]
public class BearingRigidbody : Component
{
    public RigidBody rb { get; private set; }
    public float mass { get; set; } = 1.0f;
    public float bounciness { get; set; } = 0.0f;
    private CollisionShape collider;

    [HideFromInspector]
    public CollisionShape Collider
    {
        get { return collider; }
        set {
            UpdateCollider(value);
        }
    }

    private bool _frozen = true;
    public bool frozen
    {
        get
        {
            return _frozen;
        }
        set
        {
            _frozen = value;
            UpdateFreezeState();
        }
    }

    public BearingRigidbody() { }

    public static BearingRigidbody FromRB(RigidBody rb)
    {
        BearingRigidbody b = new BearingRigidbody();
        b.rb = rb;
        return b;
    }

    public void UpdateRB(RigidBody newRb)
    {
        rb = newRb;
    }

    public void UpdateCollider(CollisionShape newShape)
    {
        if (newShape == null) return;

        if (collider != null)
            collider.Dispose();

        collider = newShape;
        collider.CalculateLocalInertia(mass, out BulletSharp.Math.Vector3 localInertia);

        if (rb == null) return;

        rb.CollisionShape = collider;
        rb.SetMassProps(mass, localInertia);
    }

    public override void OnLoad()
    {
        if (collider == null)
        {
            Mesh3D mesh = (Mesh3D)((MeshRenderer)gameObject.GetComponent(typeof(MeshRenderer))).mesh;

            Vector3 half = mesh.GetBoundingBox() / 2.0f;
            BulletSharp.Math.Vector3 halfExt = new BulletSharp.Math.Vector3(
                half.X,
                half.Y,
                half.Z
                );
            collider = new BoxShape(halfExt);
        }

        // link to transform
        Transform().onPositionChanged += PosChanged;
        Transform().onRotationChanged += RotChanged;
        Transform().onScaleChanged += ScaleChanged;

        // setting up rigidbody
        float mass = 1.0f;
        BulletSharp.Math.Vector3 localInertia;
        collider.CalculateLocalInertia(mass, out localInertia);
        var motionState = new DefaultMotionState(Transform().GetModelMatrix().ToBulletMatrix());
        var rbInfo = new RigidBodyConstructionInfo(mass, motionState, collider, localInertia);
        rb = new RigidBody(rbInfo);

        // Add the rigidbody to the world 
        PhysicsManager.physicsObjects.Add(gameObject);
        PhysicsManager.Register(rb);
    }

    private Transform3D Transform()
    {
        return (Transform3D)gameObject.transform;
    }

    private void ScaleChanged()
    {
        UpdatePhysicsScaling();

    }

    private Vector3 prevRot = Vector3.Zero;
    private void RotChanged()
    {
        if (prevRot != Transform().eRotation)
        {
            prevRot = Transform().eRotation;
            UpdateFromModelMatrix();
        }
    }

    private Vector3 prevPos = Vector3.One;
    private void PosChanged()
    {
        if (prevPos != Transform().position)
        {
            prevPos = Transform().position;
            SetPosition(Transform().position, false);
        }
    }

    private Vector3 prevScale = Vector3.One;
    public void UpdatePhysicsScaling()
    {
        if (Transform().scale == prevScale) return;
        prevScale = Transform().scale;

        Unfreeze();
        collider.LocalScaling = Transform().scale.ToBulletVector();
        rb.CollisionShape = collider;
        UpdateFreezeState();
    }

    public void SetPosition(Vector3 newPosition, bool setTransform = true)
    {
        if (setTransform)
            Transform().position = newPosition;
        UpdateFromModelMatrix();
    }

    private void UpdateFromModelMatrix()
    {
        BulletSharp.Math.Matrix m = Transform().GetModelMatrix().ToBulletMatrix();
        rb.WorldTransform = m;
    }

    private void UpdateFreezeState()
    {
        if (frozen)
        {
            Freeze();
        }
        else
        {
            Unfreeze();
        } 
    }

    private void Unfreeze(bool force = false)
    {
        if (rb == null) return;

        if (rb.InvMass == mass && !force)
            return;

        rb.SetMassProps(mass, rb.CollisionShape.CalculateLocalInertia(mass));
        rb.UpdateInertiaTensor();
        rb.CollisionFlags &= ~CollisionFlags.KinematicObject;
        rb.CollisionFlags &= ~CollisionFlags.StaticObject;
    }

    private void Freeze()
    {
        if (rb == null) return;

        if (rb.InvMass == 0)
            return;

        rb.SetMassProps(0, BulletSharp.Math.Vector3.Zero);
        rb.UpdateInertiaTensor();
        rb.AngularVelocity = BulletSharp.Math.Vector3.Zero;
        rb.LinearVelocity = BulletSharp.Math.Vector3.Zero;
        rb.CollisionFlags &= CollisionFlags.KinematicObject;
    }

    public override void OnTick(float dt)
    {
        rb.Activate(true);
        UpdateFreezeState();

        if (rb.CollisionShape.LocalScaling != Transform().scale.ToBulletVector())
        {
            ScaleChanged();
        }

        rb.Restitution = bounciness;
    }

    public override void Cleanup()
    {
        PhysicsManager.Dispose(rb);
        rb.MotionState?.Dispose();
        rb.Dispose();
    }
}