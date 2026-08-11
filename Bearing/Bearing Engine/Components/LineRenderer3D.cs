using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public class LineRenderer3D : Renderable
{
    private List<Vector3> points = new List<Vector3>() {};

    public float width = 1.0f;

    public bool drawOnTop;

    public Texture texture0;
    public Texture texture1;
    public Texture texture2;

    protected uint ebo;
    protected uint vao;
    protected uint vbo;

    public LineRenderer3D() {}

    public override void OnLoad()
    {
        material = new Material()
        {
            shader = new Shader("eng/lineRender3D.vert", "eng/default.frag"),
            parameters = { new() { name = "mainColour", value = new List<object> {0.0f,0.0f,0.0f,1.0f} } }
        };

        GL GL = GLContext.gl;

        float[] vertexData = mesh.GetVertexData();

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexData.Length * sizeof(float)), new ReadOnlySpan<float>(vertexData), BufferUsageARB.StaticDraw);

        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(mesh.indices.Length * sizeof(uint)), new ReadOnlySpan<uint>(mesh.indices), BufferUsageARB.StaticDraw);

        material.Use();

        base.OnLoad();
    }

    public override void OnTick(float dt) {}

    public override unsafe void Render()
    {
        GL GL = GLContext.gl;

        if (drawOnTop)
            GL.Disable(GLEnum.DepthTest);
        else
            GL.Enable(GLEnum.DepthTest);

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        material.SetShaderParameter("model", ((Transform3D)gameObject.transform).GetModelMatrix());
        material.SetShaderParameter("view", Game.instance.camera.GetViewMatrix());
        material.SetShaderParameter("projection", Game.instance.camera.GetProjectionMatrix());
        material.SetShaderParameter("cameraBackDirection", -Game.instance.camera.Front);

        material.SetShaderParameter("lineWidth", width);

        int pID = 0;
        foreach (Vector3 point in points)
        {
            GL.Uniform3(material.shader.GetUniformLoc($"points[{pID}].pos"), point.X, point.Y, point.Z);
            pID++;
        }
        
        material.SetShaderParameter("numPoints", points.Count);

        material.LoadParameters();

        LightManager.AddLightingInfo(material);

        if (texture0 != null)
            texture0.Use(TextureUnit.Texture0);

        if (texture1 != null)
            texture1.Use(TextureUnit.Texture1);

        if (texture2 != null)
            texture2.Use(TextureUnit.Texture2);

        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);

        AfterRender();
    }

    private void RegenerateMesh()
    {
        if (points.Count < 2)
            return;

        Mesh3D newMesh = Mesh3D.CreateEmpty();
        newMesh.vertices = new MeshVertex3D[2 * points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            newMesh.vertices[2*i].position = points[i];
            newMesh.vertices[2*i+1].position = points[i];

            newMesh.vertices[2*i].normal = new Vector3(i, -1, 0);
            newMesh.vertices[2*i+1].normal = new Vector3(i, 1, 0);
        }


        newMesh.indices = new uint[6 * points.Count - 6];
        newMesh.indices[0] = 0;
        newMesh.indices[1] = 2;
        newMesh.indices[2] = 1;
        newMesh.indices[3] = 1;
        newMesh.indices[4] = 2;
        newMesh.indices[5] = 3;
        int pointer = 5;
        for (int i = 0; i < points.Count - 2; i++)
        {
            uint a = newMesh.indices[pointer-1];
            uint b = newMesh.indices[pointer];
            uint c = b + 1;
            uint d = c + 1;

            newMesh.indices[pointer+1] = a;
            newMesh.indices[pointer+3] = b;
            newMesh.indices[pointer+2] = c;

            newMesh.indices[pointer+4] = b;
            newMesh.indices[pointer+5] = c;
            newMesh.indices[pointer+6] = d;

            pointer += 6;
        }

        newMesh.name = "Generated Line Mesh";

        mesh = newMesh;
    }

    public Vector3 GetPoint(int index) { return points[index]; }

    public void SetPoint(int index, Vector3 newPosition)
    {
        points[index] = newPosition;
    }

    ///<summary>
    ///This method fully regenerates the mesh and so should be used sparingly. If you only mean to move a point, use SetPoint() instead
    ///</summary>
    public void AddPoint(Vector3 point)
    {
        points.Add(point);

        RegenerateMesh();
    }

    protected virtual void BeforeRender() { }

    protected virtual void AfterRender() { }

    public override void Cleanup()
    {
        GL GL = GLContext.gl;

        GL.DeleteBuffer(ebo);
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);

        if (texture0 != null)
            texture0.Dispose();
        
        if (texture1 != null)
            texture1.Dispose();

        if (texture2 != null)
            texture2.Dispose();

        material.Cleanup();

        base.Cleanup();
    }
}