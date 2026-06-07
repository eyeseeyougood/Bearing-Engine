using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

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
    }

    private Vector3 startPoint = Vector3.Zero;
    private Vector3 selectedStart = Vector3.Zero;
    private bool clicked = false;
    public override void OnTick(float dt)
    {
        Transform3D trans = ((Transform3D)gameObject.transform);

        axisColour.zeroToOne = new Vector4(axisColour.zeroToOne.X, axisColour.zeroToOne.Y, axisColour.zeroToOne.Z, Hierarchy.instance.selectedObjID == -1 ? 0: 1);

        mat.SetShaderParameter("mainColour", axisColour.zeroToOne);

        if (Hierarchy.instance.selectedObjID == -1) return;
        GameObject selected = GameObject.Find(Hierarchy.instance.selectedObjID);
        Transform3D selTrans = ((Transform3D)selected.transform);

        if (Input.GetMouseButtonDown(0) && !UIManager.cursorOverUI && !Input.mouseOccupiedBy.Contains("selectionTool"))
        {
            if (CheckDistTo(trans.position) < 0.5f) { clicked = true; }

            if (clicked)
            {
                selectedStart = selTrans.position;
                startPoint = trans.position;
            }
        }

        if (Input.GetMouseButtonUp(0) && clicked)
        {
            clicked = false;

            Vector3 endPos = new Vector3(((Transform3D)selected.transform).position);
            Vector3 capturedStart = new Vector3(selectedStart);
            GameObject capturedObj = selected;

            CommandManager.Register(
            /**/(/* Do */)=>
            {
                ((Transform3D)capturedObj.transform).position = endPos;
                Inspector.instance.UpdateView();
            },
            /**/(/*Undo*/)=>
            {
                ((Transform3D)capturedObj.transform).position = capturedStart;
                Inspector.instance.UpdateView();
            });
        }

        if (clicked)
        {
            Vector3 axis = axisToMove switch { MovingAxisType.X => Vector3.UnitX, MovingAxisType.Y => Vector3.UnitY, MovingAxisType.Z => Vector3.UnitZ };
            ((Transform3D)gameObject.transform).position = startPoint + (startPoint - Extensions.FindClosestPointLineAxis(startPoint, axis, Game.instance.camera.Position, Game.instance.camera.Front));

            ((Transform3D)selected.transform).position = new Vector3(
            axisToMove == MovingAxisType.X ? (trans.position.X - trans.scale.X / 2 - selTrans.scale.X / 2) : selTrans.position.X,
            axisToMove == MovingAxisType.Y ? (trans.position.Y - trans.scale.Y / 2 - selTrans.scale.Y / 2) : selTrans.position.Y,
            axisToMove == MovingAxisType.Z ? (trans.position.Z - trans.scale.Z / 2 - selTrans.scale.Z / 2) : selTrans.position.Z
            );
        }
        else
            trans.position = new Vector3(
            axisToMove == MovingAxisType.X ? (selTrans.position.X + trans.scale.X / 2 + selTrans.scale.X / 2) : selTrans.position.X,
            axisToMove == MovingAxisType.Y ? (selTrans.position.Y + trans.scale.Y / 2 + selTrans.scale.Y / 2) : selTrans.position.Y,
            axisToMove == MovingAxisType.Z ? (selTrans.position.Z + trans.scale.Z / 2 + selTrans.scale.Z / 2) : selTrans.position.Z
        );
    }

    private float CheckDistTo(Vector3 pos)
    {
        Vector3 axis = axisToMove switch { MovingAxisType.X => Vector3.UnitX, MovingAxisType.Y => Vector3.UnitY, MovingAxisType.Z => Vector3.UnitZ };

        Transform3D trans = ((Transform3D)gameObject.transform);

        Vector3 @base = new Vector3(
            axisToMove == MovingAxisType.X ? 0 : trans.position.X,
            axisToMove == MovingAxisType.Y ? 0 : trans.position.Y,
            axisToMove == MovingAxisType.Z ? 0 : trans.position.Z
        );

        float dist = (Game.instance.camera.Position+Extensions.FindClosestPointLineAxis(-@base, axis, Game.instance.camera.Position, Game.instance.camera.Front)).Length;

        Vector3 p = Game.instance.camera.Position + Game.instance.camera.Front * dist;

        dist = (p - pos).Length;

        return dist;
    }
}