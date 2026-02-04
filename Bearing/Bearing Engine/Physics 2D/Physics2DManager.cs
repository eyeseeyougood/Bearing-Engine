using System.Timers;
using Timer = System.Timers.Timer;
using Box2D.NET;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Types;

namespace Bearing.Physics2D;

public static class Physics2DManager
{
	public static float tps { get; set; } = 20f;

	private static B2WorldId world;
	public static List<GameObject> physicsObjects = new List<GameObject>();

	public static bool simulating = true;

	public static void Init()
	{
		B2WorldDef def = new B2WorldDef();
		def.gravity = new B2Vec2(0,-9.8f);
		world = b2CreateWorld(in def);

		DateTime startTime = DateTime.Now;
        Timer timer = new Timer(1000f / tps);
        timer.Elapsed += (s, e) => {
            Tick((float)(e.SignalTime - startTime).TotalSeconds);
            startTime = DateTime.Now;
        };
        timer.AutoReset = true;
        timer.Enabled = true;
	}

	public static B2BodyId CreateBody(B2BodyDef def)
	{
		return b2CreateBody(world, in def);
	}

	public static void Tick(float dt)
	{
		if (simulating)
			b2World_Step(world, dt, 4);

        foreach (GameObject sh in physicsObjects.ToList())
        {
            Rigidbody2D rb = (Rigidbody2D)sh.GetComponent(typeof(Rigidbody2D));
            
            if (rb == null) // I think this can be the case during cleanup or sum idrk
                continue;

            B2Transform trans = b2Body_GetTransform(rb.GetBody());

            Logger.Log(b2Body_GetLinearVelocity(rb.GetBody()).Y);
/*
            ((Transform2D)sh.transform).position = trans.p.ToTKVector();
            ((Transform2D)sh.transform).rotation = MathF.Asin(trans.q.s);*/
        }
	}
}