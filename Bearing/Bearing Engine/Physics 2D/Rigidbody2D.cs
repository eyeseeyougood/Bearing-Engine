using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Types;

using OpenTK.Mathematics;

namespace Bearing.Physics2D;

public class Rigidbody2D : Component
{
    public float density { get; set; } = 1f;
    public bool isStatic { get; set; } = false;

    private B2BodyId body;
    private B2Polygon collider;
    private B2ShapeId shape;

    public override void Cleanup()
    {
        Physics2DManager.physicsObjects.Remove(gameObject);
    }
    
    public override void OnLoad()
    {/*
        B2SurfaceMaterial surface = new B2SurfaceMaterial();

        B2Filter filter = new B2Filter();

        B2ShapeDef shapeDef = new B2ShapeDef();
        shapeDef.density = density;
        shapeDef.material = surface;
        shapeDef.filter = filter;

        collider = new B2Polygon();
        collider = b2MakeBox(0.5f, 0.5f);

        B2BodyDef def = new B2BodyDef();
        def.type = isStatic ? B2BodyType.b2_staticBody : B2BodyType.b2_dynamicBody;
        def.gravityScale = 1f;
        def.enableSleep = false;
        Logger.Log($"INITIALISING TYPE OF BODY: {def.type}");

        // link to transform

        Transform().onPositionChanged += PosChanged;
        Transform().onRotationChanged += RotChanged;


        body = Physics2DManager.CreateBody(def);
        Logger.Log($"BODY VALID: {B2Worlds.b2Body_IsValid(body)}");

        shape = b2CreatePolygonShape(body, in shapeDef, in collider);

        UpdateFromModelMatrix();

        b2Body_ApplyMassFromShapes(body);

        Physics2DManager.physicsObjects.Add(gameObject);*/

        B2BodyDef bodyDef = b2DefaultBodyDef();
        B2ShapeDef shapeDef = b2DefaultShapeDef();

        B2Polygon box = b2MakeBox(0.5f, 0.5f);
        bodyDef.position = new B2Vec2(0.0f, 1.0f);
        bodyDef.linearVelocity = new B2Vec2(5.0f, 0.0f);
        bodyDef.type = B2BodyType.b2_dynamicBody;
        body = Physics2DManager.CreateBody(bodyDef);
        shape = b2CreatePolygonShape(body, shapeDef, box);

        Physics2DManager.physicsObjects.Add(gameObject);
    }

    private float prevRot = 0f;
    private void RotChanged()
    {
        if (prevRot != Transform().rotation)
        {
            prevRot = Transform().rotation;
            UpdateFromModelMatrix();
        }
    }

    private Vector2 prevPos = Vector2.One;
    private void PosChanged()
    {
        if (prevPos != Transform().position)
        {
            prevPos = Transform().position;
            SetPosition(Transform().position, false);
        }
    }

    public void SetPosition(Vector2 newPosition, bool setTransform = true)
    {
        if (setTransform)
            Transform().position = newPosition;
        //UpdateFromModelMatrix();
    }

    private void UpdateFromModelMatrix()
    {
        B2Rot rot = new B2Rot(MathF.Cos(Transform().rotation), MathF.Sin(Transform().rotation));
        B2Bodies.b2Body_SetTransform(body, Transform().position.ToB2Vector(), rot);
    }

    private Transform2D Transform()
    {
        return (Transform2D)gameObject.transform;
    }

    public B2BodyId GetBody()
    {
        return body;
    }
    
    public override void OnTick(float dt)
    {
    }
}