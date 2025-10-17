using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using BulletSharp;
using OpenTK.Mathematics;
using SkiaSharp;

namespace Bearing;

[RequireComponent(typeof(MeshRenderer))]
public class BearingRigidbody : Component
{
    public RigidBody rb { get; private set; }
    public float mass { get; set; } = 1.0f;
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
        gameObject.transform.onTransformChanged += TranformChanged;

        // setting up rigidbody
        float mass = 1.0f;
        BulletSharp.Math.Vector3 localInertia;
        collider.CalculateLocalInertia(mass, out localInertia);
        var motionState = new DefaultMotionState(gameObject.transform.GetModelMatrix().ToBulletMatrix());
        var rbInfo = new RigidBodyConstructionInfo(mass, motionState, collider, localInertia);
        rb = new RigidBody(rbInfo);

        // Add the rigidbody to the world 
        PhysicsManager.physicsObjects.Add(gameObject);
        PhysicsManager.Register(rb);
    }

    private Vector3 prevPos = Vector3.One;
    private Vector3 prevRot = Vector3.Zero;
    private void TranformChanged()
    {
        UpdatePhysicsScaling();
        if (prevPos != gameObject.transform.position)
        {
            prevPos = gameObject.transform.position;
            SetPosition(gameObject.transform.position, false);
        }

        if (prevRot != gameObject.transform.eRotation)
        {
            prevRot = gameObject.transform.eRotation;
            UpdateFromModelMatrix();
        }
    }

    private Vector3 prevScale = Vector3.One;
    public void UpdatePhysicsScaling()
    {
        if (gameObject.transform.scale == prevScale) return;
        prevScale = gameObject.transform.scale;

        Unfreeze();
        collider.LocalScaling = gameObject.transform.scale.ToBulletVector();
        rb.CollisionShape = collider;
        Logger.Log(gameObject.transform.scale);
        UpdateFreezeState();
    }

    public void SetPosition(Vector3 newPosition, bool setTransform = true)
    {
        Logger.Log("moving to: " + newPosition);
        if (setTransform)
            gameObject.transform.position = newPosition;
        UpdateFromModelMatrix();
    }

    private void UpdateFromModelMatrix()
    {
        BulletSharp.Math.Matrix m = gameObject.transform.GetModelMatrix().ToBulletMatrix();
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
        if (rb.InvMass == mass && !force)
            return;

        rb.SetMassProps(mass, rb.CollisionShape.CalculateLocalInertia(mass));
        rb.UpdateInertiaTensor();
        rb.CollisionFlags &= ~CollisionFlags.KinematicObject;
        rb.CollisionFlags &= ~CollisionFlags.StaticObject;

        Logger.Log("unfrozen");
    }

    private void Freeze()
    {
        if (rb.InvMass == 0)
            return;

        rb.SetMassProps(0, BulletSharp.Math.Vector3.Zero);
        rb.UpdateInertiaTensor();
        rb.AngularVelocity = BulletSharp.Math.Vector3.Zero;
        rb.LinearVelocity = BulletSharp.Math.Vector3.Zero;
        rb.CollisionFlags &= CollisionFlags.KinematicObject;

        Logger.Log("frozen");
    }

    public override void OnTick(float dt)
    {
        rb.Activate(true);
        UpdateFreezeState();

        if (rb.CollisionShape.LocalScaling != gameObject.transform.scale.ToBulletVector())
        {
            TranformChanged();
        }
    }

    public override void Cleanup()
    {
        PhysicsManager.Dispose(rb);
        rb.MotionState?.Dispose();
        rb.Dispose();
    }
}