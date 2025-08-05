using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class CameraMovement : Component
{
    public float mouseSenseX { get; set; } = 0.25f;
    public float mouseSenseY { get; set; } = 0.25f;
    public float camSpeed { get; set; } = 2f;
    public float speedMultiplier { get; set; } = 2f;
    public float scrollMoveDist { get; set; } = 1f;

    private bool moving = false;
    private float currentSpeed;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
    }

    public override void OnTick(float dt)
    {
        if (Input.GetMouseButtonDown(1) && !UIManager.cursorOverUI)
        {
            moving = true;
        }

        if (moving)
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            Game.instance.camera.Pitch -= mouseDelta.Y * mouseSenseY;
            Game.instance.camera.Yaw += mouseDelta.X * mouseSenseX;

            Camera cam = Game.instance.camera;

            Vector3 moveDir = (Input.GetKey(Keys.W) ? 1 : 0) * cam.Front
                            + (Input.GetKey(Keys.S) ? 1 : 0) * -cam.Front
                            + (Input.GetKey(Keys.A) ? 1 : 0) * -cam.Right
                            + (Input.GetKey(Keys.D) ? 1 : 0) * cam.Right
                            + (Input.GetKey(Keys.Q) ? 1 : 0) * -cam.Up
                            + (Input.GetKey(Keys.E) ? 1 : 0) * cam.Up;

            if (moveDir.Length != 0)
                moveDir.Normalize();

            currentSpeed = camSpeed * (Input.GetKey(Keys.LeftShift) ? speedMultiplier : 1);

            cam.Position += moveDir * currentSpeed * dt;

            if (Input.GetMouseScrollDelta().Y > 0)
            {
                cam.Position += cam.Front * scrollMoveDist;
            }
            if (Input.GetMouseScrollDelta().Y < 0)
            {
                cam.Position -= cam.Front * scrollMoveDist;
            }
        }

        if (Input.GetMouseButtonUp(1) && moving)
        {
            moving = false;
        }
    }
}