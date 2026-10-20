using OpenTK.Mathematics;

namespace Bearing;

public class ParticleEmissionShape3D
{
    public virtual List<ParticleEmitter3D.Particle3D> GenerateParticles(int count, Matrix4 baseModel, float velocityMod = 0)
    {
    	List<ParticleEmitter3D.Particle3D> result = new List<ParticleEmitter3D.Particle3D>();

        for (int i = 0; i < count; i++)
        {
            ParticleEmitter3D.Particle3D particle = new ParticleEmitter3D.Particle3D(Time.now);
            particle.transform.FromModel(baseModel);
            particle.transform.scale = Vector3.One * 0.1f;
            particle.velocity = new Vector3(
                Random.Shared.Next(-10,10)/10f,
                Random.Shared.Next(-10,10)/10f,
                Random.Shared.Next(-10,10)/10f
            ) * velocityMod;
            result.Add(particle);
        }

        return result;
    }
}