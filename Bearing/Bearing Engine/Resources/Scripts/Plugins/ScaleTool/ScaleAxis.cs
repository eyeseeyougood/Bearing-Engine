using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public enum ScaleAxisType
{
    X, Y, Z
}

public class ScaleAxis : Component
{
    public ScaleAxisType axisToMove;
    private BearingColour axisColour = BearingColour.White;
    private Material mat;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        axisColour = axisToMove switch {
            ScaleAxisType.X => BearingColour.FromZeroToOne(new Vector3(1,0,0)),
            ScaleAxisType.Y => BearingColour.FromZeroToOne(new Vector3(0,1,0)),
            ScaleAxisType.Z => BearingColour.FromZeroToOne(new Vector3(0,0,1)),
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
    private Vector3 startScale = Vector3.Zero;
    private Vector3 selectedStart = Vector3.Zero;
    private bool clicked = false;
    public override void OnTick(float dt)
    {
        Transform3D trans = ((Transform3D)gameObject.transform);

        axisColour.zeroToOne = new Vector4(axisColour.zeroToOne.X, axisColour.zeroToOne.Y, axisColour.zeroToOne.Z, Hierarchy.instance.selectedObjID == -1 ? 0: 1);

        mat.SetShaderParameter(new ShaderParam() { name = "mainColour", vector4 = axisColour.zeroToOne });

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
                startScale = selTrans.scale;
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
            Vector3 axis = axisToMove switch { ScaleAxisType.X => Vector3.UnitX, ScaleAxisType.Y => Vector3.UnitY, ScaleAxisType.Z => Vector3.UnitZ };
            Vector3 newPos = startPoint + (startPoint - Extensions.FindClosestPointLineAxis(startPoint, axis, Game.instance.camera.Position, Game.instance.camera.Front));
            Vector3 newScale = newPos - startPoint;
            ((Transform3D)gameObject.transform).position = newPos;
            ((Transform3D)selected.transform).scale = startScale + newScale*2f;
        }
        else
            trans.position = new Vector3(
                axisToMove == ScaleAxisType.X ? (selTrans.position.X + trans.scale.X / 2 + selTrans.scale.X / 2) : selTrans.position.X,
                axisToMove == ScaleAxisType.Y ? (selTrans.position.Y + trans.scale.Y / 2 + selTrans.scale.Y / 2) : selTrans.position.Y,
                axisToMove == ScaleAxisType.Z ? (selTrans.position.Z + trans.scale.Z / 2 + selTrans.scale.Z / 2) : selTrans.position.Z
            );
    }

    private float CheckDistTo(Vector3 pos)
    {
        Vector3 axis = axisToMove switch { ScaleAxisType.X => Vector3.UnitX, ScaleAxisType.Y => Vector3.UnitY, ScaleAxisType.Z => Vector3.UnitZ };

        Transform3D trans = ((Transform3D)gameObject.transform);

        Vector3 @base = new Vector3(
            axisToMove == ScaleAxisType.X ? 0 : trans.position.X,
            axisToMove == ScaleAxisType.Y ? 0 : trans.position.Y,
            axisToMove == ScaleAxisType.Z ? 0 : trans.position.Z
        );

        float dist = (Game.instance.camera.Position+Extensions.FindClosestPointLineAxis(-@base, axis, Game.instance.camera.Position, Game.instance.camera.Front)).Length;

        Vector3 p = Game.instance.camera.Position + Game.instance.camera.Front * dist;

        dist = (p - pos).Length;

        return dist;
    }
}