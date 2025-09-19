using Bearing;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MovingAxisType
{
    X, Y, Z
}

public class MovingAxis : Component
{
    public MovingAxisType axisToMove;
    private BearingColour axisColour = BearingColour.White;
    private Material mat;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        axisColour = axisToMove switch {
            MovingAxisType.X => BearingColour.FromZeroToOne(new Vector3(1,0,0)),
            MovingAxisType.Y => BearingColour.FromZeroToOne(new Vector3(0,1,0)),
            MovingAxisType.Z => BearingColour.FromZeroToOne(new Vector3(0,0,1)),
        };

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        mat = new Material();
        mr.material = mat;
        mr.material.shader = new Shader("default.vert", "default.frag");
        mr.material.attribs = new List<ShaderAttrib>()
        {
            new ShaderAttrib() { name = "aPosition", size = 3 },
            new ShaderAttrib() { name = "aTexCoord", size = 2 },
            new ShaderAttrib() { name = "aNormal", size = 3 },
        };
    }

    private Vector3 startPoint = Vector3.Zero;
    private Vector3 selectedStart = Vector3.Zero;
    private bool clicked = false;
    public override void OnTick(float dt)
    {
        axisColour.zeroToOne = new Vector4(axisColour.zeroToOne.X, axisColour.zeroToOne.Y, axisColour.zeroToOne.Z, Hierarchy.instance.selectedObjID == -1 ? 0: 1);

        mat.SetShaderParameter(new ShaderParam() { name = "mainColour", vector4 = axisColour.zeroToOne });

        if (Hierarchy.instance.selectedObjID == -1) return;
        GameObject selected = GameObject.Find(Hierarchy.instance.selectedObjID);

        if (Input.GetMouseButtonDown(0))
        {
            if (CheckDistTo(gameObject.transform.position) < 0.5f) { clicked = true; }

            if (clicked)
            {
                selectedStart = selected.transform.position;
                startPoint = gameObject.transform.position;
            }
        }

        if (Input.GetMouseButtonUp(0) && clicked)
        {
            clicked = false;

            Vector3 endPos = new Vector3(selected.transform.position);
            Vector3 capturedStart = new Vector3(selectedStart);
            GameObject capturedObj = selected;

            CommandManager.Register(
            /**/(/* Do */)=>
            {
                capturedObj.transform.position = endPos;
                Inspector.instance.UpdateView();
            },
            /**/(/*Undo*/)=>
            {
                capturedObj.transform.position = capturedStart;
                Inspector.instance.UpdateView();
            });
        }

        if (clicked)
        {
            Vector3 axis = axisToMove switch { MovingAxisType.X => Vector3.UnitX, MovingAxisType.Y => Vector3.UnitY, MovingAxisType.Z => Vector3.UnitZ };
            gameObject.transform.position = startPoint + (startPoint - Extensions.FindClosestPointLineAxis(startPoint, axis, Game.instance.camera.Position, Game.instance.camera.Front));
        }
        else
            gameObject.transform.position = new Vector3(
            axisToMove == MovingAxisType.X ? (selected.transform.position.X + gameObject.transform.scale.X / 2 + selected.transform.scale.X / 2) : selected.transform.position.X,
            axisToMove == MovingAxisType.Y ? (selected.transform.position.Y + gameObject.transform.scale.Y / 2 + selected.transform.scale.Y / 2) : selected.transform.position.Y,
            axisToMove == MovingAxisType.Z ? (selected.transform.position.Z + gameObject.transform.scale.Z / 2 + selected.transform.scale.Z / 2) : selected.transform.position.Z
        );

        selected.transform.position = new Vector3(
            axisToMove == MovingAxisType.X ? (gameObject.transform.position.X - gameObject.transform.scale.X / 2 - selected.transform.scale.X / 2) : selected.transform.position.X,
            axisToMove == MovingAxisType.Y ? (gameObject.transform.position.Y - gameObject.transform.scale.Y / 2 - selected.transform.scale.Y / 2) : selected.transform.position.Y,
            axisToMove == MovingAxisType.Z ? (gameObject.transform.position.Z - gameObject.transform.scale.Z / 2 - selected.transform.scale.Z / 2) : selected.transform.position.Z
        );
    }

    private float CheckDistTo(Vector3 pos)
    {
        Vector3 axis = axisToMove switch { MovingAxisType.X => Vector3.UnitX, MovingAxisType.Y => Vector3.UnitY, MovingAxisType.Z => Vector3.UnitZ };

        Vector3 @base = new Vector3(
            axisToMove == MovingAxisType.X ? 0 : gameObject.transform.position.X,
            axisToMove == MovingAxisType.Y ? 0 : gameObject.transform.position.Y,
            axisToMove == MovingAxisType.Z ? 0 : gameObject.transform.position.Z
        );

        float dist = (Game.instance.camera.Position+Extensions.FindClosestPointLineAxis(-@base, axis, Game.instance.camera.Position, Game.instance.camera.Front)).Length;

        Vector3 p = Game.instance.camera.Position + Game.instance.camera.Front * dist;

        dist = (p - pos).Length;

        return dist;
    }
}