using OpenTK.Mathematics;
using Silk.NET.OpenGL;

namespace Bearing;

///<summary>
///This is the class to use if you want a particle system.
///</summary>
public class ParticleEmitter3D : MeshRenderer
{
    public struct Particle3D
    {
        public Transform3D transform = new Transform3D();
        public Vector3 velocity = new Vector3();
        public Quaternion angularVelocity = new Quaternion();
        public float lifetime = 10f;
        public float spawnTime { get; private set; }

        public Particle3D(float spawnTime) {this.spawnTime=spawnTime;}
    }

    ///<summary>
    ///Super Janky way to prevent future child classes from seeing or using this. It does nothing in the particle emitter so don't use it.
    ///</summary>
    protected new readonly bool setup3DMatrices;

    protected ParticleEmissionShape3D? _emissionShape = new ParticleEmissionShape3D();
    public ParticleEmissionShape3D? emissionShape
    {
        get{
            return _emissionShape;
        }
        set{
            _emissionShape = value;
        }
    }

    protected List<Particle3D> particles = new List<Particle3D>{};
    protected List<Matrix4> particleModels = new List<Matrix4>{};

    public int maxParticle = 1000000;
    public int spawnRate = 6000;
    public float velocity = 0.1f;

    protected int particleCount = 0;
    
    protected uint ssbo0;

	public ParticleEmitter3D(string embeddedMesh) : base(embeddedMesh) {}
	public ParticleEmitter3D(Resource mesh) : base(mesh) {}

    public override void OnLoad()
    {
        material = new Material()
        {
            shader = new Shader("eng/particles3D.vert", "eng/lighting.frag"),

            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", value = new List<object> {1.0f, 1.0f, 1.0f, 0.2f} },
            }
        };

        GenerateParticles();

        GenerateBuffer();
        UpdateBuffer();

    	base.OnLoad();
    }

    public virtual int GetParticleCount()
    {
        return particleCount;
    }

    protected virtual void GenerateParticles()
    {
        if (emissionShape is null)
        {
            Logger.LogError("ParticleEmitter3D has no ParticleEmissionShape3D assigned, no particle can be generated.");
        }

        // TODO: OPTIMISATION, I have no idea but maybe caching the result of gameObject.Transform3D() is an optimisation here
        if (particleCount < maxParticle && emissionShape is not null)
            particles.AddRange(emissionShape.GenerateParticles(spawnRate/60 + 1, gameObject.Transform3D().GetModelMatrix(), velocity));

        Delay(GenerateParticles, 1.0f / (float)spawnRate);
    }

    protected virtual void UpdateParticles(float dt)
    {
        if (particleModels.Count > particleCount)
        {
            particleModels.RemoveRange(particleCount, particleModels.Count - particleCount);
        }

        for (int i = 0; i < particleCount; i++)
        {
            if (i >= particleModels.Count)
            {
                particleModels.Add(Matrix4.Identity);
            }

            particles[i].transform.position += particles[i].velocity * dt;

            particleModels[i] = particles[i].transform.GetModelMatrix();
        }
    }

    protected virtual unsafe void GenerateBuffer()
    {
        GL GL = GLContext.gl;
        ssbo0 = GL.GenBuffer();
    }

    protected virtual unsafe void UpdateBuffer()
    {
        GL GL = GLContext.gl;

        GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, ssbo0);

        fixed(void* ptr = particleModels.ToArray())
            GL.BufferData(BufferTargetARB.ShaderStorageBuffer, (nuint)particleModels.Count * 16 * sizeof(float), ptr, BufferUsageARB.DynamicDraw);

        GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
    }

    protected virtual void KillParticles()
    {
        foreach (Particle3D particle in particles.ToList())
        {
            if (particle.lifetime + particle.spawnTime <= Time.now)
            {
                particles.Remove(particle);
            }
        }
    }

    protected virtual void BindSSBO()
    {
        GLContext.gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, ssbo0);
    }

    protected virtual void UnbindSSBO()
    {
        GLContext.gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, 0);
    }

    public override void OnTick(float dt)
    {
        particleCount = particles.Count;

        base.OnTick(dt);
    }

    private float lastTime = 0f;
    public override unsafe void Render()
    {
        GL GL = GLContext.gl;

        if (mesh is null)
            return;

        if (drawOnTop)
            GL.Disable(GLEnum.DepthTest);
        else
            GL.Enable(GLEnum.DepthTest);

        if (renderBackFaces)
            GL.Disable(GLEnum.CullFace);
        else
            GL.Enable(GLEnum.CullFace);

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        material.SetShaderParameter("view", Game.instance.camera.GetViewMatrix());
        material.SetShaderParameter("projection", Game.instance.camera.GetProjectionMatrix());

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

        UpdateParticles(Time.now - lastTime);
        UpdateBuffer();
        BindSSBO();

        GL.DrawElementsInstanced(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0, (uint)particleModels.Count);

        UnbindSSBO();

        KillParticles();

        AfterRender();

        lastTime = Time.now;
    }

    public override void Cleanup() { base.Cleanup(); }
}