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
    private CollisionShape collider;

    public bool frozen { get; set; }

    public BearingRigidbody() { }

    public static BearingRigidbody FromRB(RigidBody rb)
    {
        BearingRigidbody b = new BearingRigidbody();
        b.rb = rb;
        return b;
    }

    public override void OnLoad()
    {
        Mesh mesh = ((MeshRenderer)gameObject.GetComponent(typeof(MeshRenderer))).mesh;

        Vector3 half = mesh.GetBoundingBox() / 2.0f;
        BulletSharp.Math.Vector3 halfExt = new BulletSharp.Math.Vector3(
        half.X,
        half.Y,
            half.Z
            );
        collider = new BoxShape(halfExt);

        // setting up rigidbody
        float mass = 1.0f;
        BulletSharp.Math.Vector3 localInertia;
        collider.CalculateLocalInertia(mass, out localInertia); // Calculate inertia
        var motionState = new DefaultMotionState(gameObject.transform.GetModelMatrix().ToBulletMatrix());
        var rbInfo = new RigidBodyConstructionInfo(mass, motionState, collider, localInertia);
        rb = new RigidBody(rbInfo);

        // Add the rigidbody to the world
        PhysicsManager.physicsObjects.Add(gameObject);
        PhysicsManager.Register(rb);
    }

    public override void OnTick()
    {
    }

    public override void Cleanup()
    {
    }
}
