using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Tractor : Component
{
    public float speed { get; set; }
    public float rotSpeed { get; set; }

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
    }

    public override void OnTick(float dt)
    {
        Vector3 direction = Vector3.Zero;
        if (Input.GetKey(Keys.W))
        {
            direction += gameObject.transform.GetForward();
        }
        if (Input.GetKey(Keys.S))
        {
            direction -= gameObject.transform.GetForward();
        }

        if (Input.GetKey(Keys.A))
        {
            gameObject.transform.eRotation += Vector3.UnitY * dt * rotSpeed;
        }
        if (Input.GetKey(Keys.D))
        {
            gameObject.transform.eRotation -= Vector3.UnitY * dt * rotSpeed;
        }

        gameObject.transform.position += direction * speed * dt;
    }
}