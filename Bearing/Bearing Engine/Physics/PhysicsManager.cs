using BulletSharp;
using BulletSharp.Math;
using System.Timers;
using Timer = System.Timers.Timer;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Bearing;

public static class PhysicsManager
{
    private static DiscreteDynamicsWorld world;
    public static List<GameObject> physicsObjects = new List<GameObject>();

    public static int tps = 40;

    public static float gravity { get; private set; } = -9.81f;

    public static void Init()
    {
        var collisionConfig = new DefaultCollisionConfiguration();
        var dispatcher = new CollisionDispatcher(collisionConfig);
        var broadphase = new DbvtBroadphase();
        var solver = new SequentialImpulseConstraintSolver();

        // Create the physics world
        world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
        world.Gravity = new Vector3(0, -9.81f, 0).ToBulletVector();

        DateTime startTime = DateTime.Now;
        Timer timer = new Timer(1000f / tps);
        timer.Elapsed += (s, e) => {
            Tick((float)(e.SignalTime - startTime).TotalSeconds);
            startTime = DateTime.Now;
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    public static void Tick(float delta)
    {
        world.StepSimulation(delta, 10, delta / 10f);
        
        // Get updated cube position
        foreach (GameObject sh in physicsObjects.ToList())
        {
            BearingRigidbody brb = (BearingRigidbody)sh.GetComponent(typeof(BearingRigidbody));
            if (brb == null) // I think this can be the case during cleanup or sum idrk
                continue;

            RigidBody rb = brb.rb;
            rb.MotionState.GetWorldTransform(out Matrix worldTransform);

            Vector3 sBefore = sh.transform.scale;
            Matrix4 m = worldTransform.ToTKMatrix();
            m = m.ClearScale();
            m = Matrix4.CreateScale(sBefore) * m;
            sh.transform.FromModel(m, false);
        }
    }

    public static DiscreteDynamicsWorld GetWorld()
    {
        return world;
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