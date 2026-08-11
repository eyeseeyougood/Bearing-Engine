using OpenTK.Mathematics;
using Silk.NET.Input;
using Bearing;

public class CameraMovement : Component
{
	public float normalSpeed { get; set; }
	public float fastSpeed { get; set; }
	public float mouseSenseX { get; set; }
	public float mouseSenseY { get; set; }

	private bool mouseLocked;

    public override void OnLoad() {}

    public override void OnTick(float dt)
    {
    	if (Input.GetMouseButtonDown(1) && !mouseLocked && !UIManager.cursorOverUI)
    	{
			Input.LockCursor();
			mouseLocked = true;
    	}

    	if (Input.GetMouseButtonUp(1) && mouseLocked)
    	{
			Input.UnlockCursor();
			mouseLocked = false;
    	}

    	if (!mouseLocked)
    		return;
    
    	Vector2 mouse = Input.GetMouseDelta() * new Vector2(mouseSenseX, mouseSenseY);

    	Game.instance.camera.Pitch -= mouse.Y;
    	Game.instance.camera.Yaw += mouse.X;

    	float speed = normalSpeed;
    	if (Input.GetKey(Key.ShiftLeft))
    		speed = fastSpeed;

    	if (Input.GetKey(Key.A))
    	{
    		Game.instance.camera.Position -= Game.instance.camera.Right * dt * speed;
    	}

    	if (Input.GetKey(Key.D))
    	{
    		Game.instance.camera.Position += Game.instance.camera.Right * dt * speed;
    	}

    	if (Input.GetKey(Key.W))
    	{
    		Game.instance.camera.Position += Game.instance.camera.Front * dt * speed;
    	}

    	if (Input.GetKey(Key.S))
    	{
    		Game.instance.camera.Position -= Game.instance.camera.Front * dt * speed;
    	}

    	if (Input.GetKey(Key.E))
    	{
    		Game.instance.camera.Position += Game.instance.camera.Up * dt * speed;
    	}

    	if (Input.GetKey(Key.Q))
    	{
    		Game.instance.camera.Position -= Game.instance.camera.Up * dt * speed;
    	}

    	Game.instance.camera.Position += Game.instance.camera.Front * Input.GetMouseScrollDelta().Y * 5;
    }
    
    public override void Cleanup() {}
}