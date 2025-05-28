using BulletSharp;
using BulletSharp.Math;

namespace Bearing
{
    public static class PhysicsManager
    {/*
        private static DiscreteDynamicsWorld world;
        public static List<Shape> shapes = new List<Shape>();

        public static int ticksPerTick;

        public static void Init()
        {
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            var broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();

            // Create the physics world
            world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
            world.Gravity = new Vector3(0, -9.81f, 0);

            ticksPerTick = 1;
        }

        public static void Tick()
        {
            float timeStep = 1f / 60f; // 60 FPS
            timeStep /= ticksPerTick;
            for (int i = 0; i < ticksPerTick; i++)
            {
                world.StepSimulation(timeStep);

                // Get updated cube position
                foreach (Shape sh in shapes)
                {
                    sh.rb.MotionState.GetWorldTransform(out Matrix worldTransform);
                    sh.transform.FromModel(worldTransform.ToTKMatrix());
                    Vector3 position = worldTransform.Origin;

                    FreezeRigidbody(sh, sh.anchored);
                }
            }
        }

        private static void FreezeRigidbody(Shape sender, bool freeze)
        {
            RigidBody rigidBody = sender.rb;
            if (freeze)
            {
                if (rigidBody.InvMass == 0)
                    return;

                Dispose(rigidBody);

                RigidBody staticBody = CreateStaticCopy(rigidBody);
                sender.rb = staticBody;

                Register(staticBody);
            }
            else
            {
                if (rigidBody.InvMass == 0)
                {
                    Dispose(rigidBody);

                    RigidBody dynamicBody = CreateDynamicCopy(rigidBody);
                    sender.rb = dynamicBody;
                    Register(dynamicBody);
                }
            }
        }

        private static RigidBody CreateStaticCopy(RigidBody original)
        {
            CollisionShape shape = original.CollisionShape;
            Matrix transform;
            original.MotionState.GetWorldTransform(out transform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(transform), shape);
            return new RigidBody(rbInfo);
        }

        private static RigidBody CreateDynamicCopy(RigidBody original)
        {
            CollisionShape shape = original.CollisionShape;
            Matrix transform;
            original.MotionState.GetWorldTransform(out transform);

            float mass = 1.0f;
            Vector3 localInertia;
            shape.CalculateLocalInertia(mass, out localInertia);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, new DefaultMotionState(transform), shape, localInertia);
            return new RigidBody(rbInfo);
        }

        public static void Register(RigidBody body)
        {
            world.AddRigidBody(body);
        }

        public static void Dispose(RigidBody body)
        {
            world.RemoveRigidBody(body);
        }*/
    }
}
