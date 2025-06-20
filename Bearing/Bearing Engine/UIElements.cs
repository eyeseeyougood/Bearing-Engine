using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class UIElement : MeshRenderer
{
    public UIElement(string mesh) : base(mesh, true) { }
    
    UDim2 position = new UDim2(0,0,0,0);
    UDim2 size = UDim2.One;

    public override void OnLoad()
    {
        base.OnLoad();
        Game.instance.RemoveOpaqueRenderable(this); // ui should not be handled like all other renderables XDD

        UIManager.AddUI(this);
    }

    public override void OnTick(float dt)
    {
        gameObject.transform.position = Game.instance.camera.Position;
        gameObject.transform.position += Game.instance.camera.Front;

        float screenW = Game.instance.Size.X;
        float screenH = Game.instance.Size.Y;

        material.SetShaderParameter(new ShaderParam("posOffset", position.offset));
        material.SetShaderParameter(new ShaderParam("posScale", position.scale));
        //material.SetShaderParameter(new ShaderParam("sizeOffset", size.offset));
        //material.SetShaderParameter(new ShaderParam("sizeScale", size.scale));
    }
}

public class UILabel : UIElement
{
    public UILabel() : base("Quad.obj")
    {
        material = Material.uiFallback;
        setup3DMatrices = false;
        SetMesh(new Mesh2D("Quad.obj", true));
    }
}