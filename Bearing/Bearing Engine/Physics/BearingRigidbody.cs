using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using BulletSharp;
using OpenTK.Mathematics;

namespace Bearing;

[RequireComponent(typeof(MeshRenderer))]
public class BearingRigidbody : Component
{
    public RigidBody rb { get; private set; }
    public float mass { get; set; } = 1.0f;
    private CollisionShape collider;
    public CollisionShape Collider
    {
        get { return collider; }
        set {
            UpdateCollider(value);
        }
    }

    public bool frozen { get; set; } = true;

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

    private void TranformChanged()
    {
        SetPosition(gameObject.transform.position, false);
    }

    public void SetPosition(Vector3 newPosition, bool setTransform = true)
    {
        if (setTransform)
            gameObject.transform.position = newPosition;
        UpdateFromModelMatrix();
    }

    private void UpdateFromModelMatrix()
    {
        BulletSharp.Math.Matrix m = gameObject.transform.GetModelMatrix().ToBulletMatrix();
        rb.MotionState.SetWorldTransform(ref m);
        if (frozen)
        {
            PhysicsManager.GetWorld().UpdateSingleAabb(rb);
        }
    }

    public override void OnTick(float dt)
    {
    }

    public override void Cleanup()
    {
        PhysicsManager.Dispose(rb);
        rb.MotionState?.Dispose();
        rb.Dispose();
    }
}