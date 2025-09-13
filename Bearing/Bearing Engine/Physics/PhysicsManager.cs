using BulletSharp;
using BulletSharp.Math;
using System.Reflection;

namespace Bearing;

public static class PhysicsManager
{
    private static DiscreteDynamicsWorld world;
    public static List<GameObject> physicsObjects = new List<GameObject>();

    public static int ticksPerTick = 1;

    public static float gravity { get; private set; } = -9.81f;

    public static void Init()
    {
        var collisionConfig = new DefaultCollisionConfiguration();
        var dispatcher = new CollisionDispatcher(collisionConfig);
        var broadphase = new DbvtBroadphase();
        var solver = new SequentialImpulseConstraintSolver();

        // Create the physics world
        world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
        world.Gravity = new Vector3(0, -9.81f, 0);
    }

    public static void Tick()
    {
        float timeStep = 1f / 60f; // 60 FPS
        timeStep /= ticksPerTick;
        for (int i = 0; i < ticksPerTick; i++)
        {
            // Get updated cube position
            foreach (GameObject sh in physicsObjects)
            {
                BearingRigidbody brb = (BearingRigidbody)sh.GetComponent(typeof(BearingRigidbody));
                if (brb == null) // I think this can be the case during cleanup or sum idrk
                    continue;

                RigidBody rb = brb.rb;
                rb.MotionState.GetWorldTransform(out Matrix worldTransform);
                sh.transform.FromModel(worldTransform.ToTKMatrix());
                Vector3 position = worldTransform.Origin;

                FreezeRigidbody(sh, brb.frozen);
            }

            world.StepSimulation(timeStep);
        }
    }

    public static DiscreteDynamicsWorld GetWorld()
    {
        return world;
    }

    private static void FreezeRigidbody(GameObject sender, bool freeze)
    {
        BearingRigidbody brb = (BearingRigidbody)sender.GetComponent(typeof(BearingRigidbody));
        RigidBody rigidBody = brb.rb;
        if (freeze)
        {
            if ((rigidBody.CollisionFlags & CollisionFlags.KinematicObject) != 0)
                return;

            rigidBody.LinearVelocity = Vector3.Zero;
            rigidBody.AngularVelocity = Vector3.Zero;
            rigidBody.ForceActivationState(ActivationState.DisableSimulation);
            rigidBody.CollisionFlags |= CollisionFlags.KinematicObject;
        }
        else
        {
            if ((rigidBody.CollisionFlags & CollisionFlags.KinematicObject) != 0)
            {
                rigidBody.CollisionFlags &= ~CollisionFlags.KinematicObject;
                rigidBody.ForceActivationState(ActivationState.ActiveTag);
                rigidBody.Activate(true);
            }
        }
    }

    public static void Register(RigidBody body)
    {
        world.AddRigidBody(body);
    }

    public static void Dispose(RigidBody body)
    {
        world.RemoveRigidBody(body);
    }
}
