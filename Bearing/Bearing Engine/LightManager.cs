using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public static class LightManager
{
    public static List<Light> lights = new List<Light>(100);

    public static void AddLight(Light light)
    {
        lights.Add(light);
    }

    private static void AddPointLights(Material mat)
    {
        GL GL = GLContext.gl;

        if (!mat.shader.HasUniform("numPointLights")) { return; } // incase this isn't a lighting shader

        int pointId = 0;
        foreach (Light l in lights)
        {
            if (l is PointLight pl)
            {
                GL.UseProgram((uint)mat.shader.Handle);
                Vector3 pos = ((Transform3D)pl.gameObject.transform).worldPosition;
                GL.Uniform3(mat.shader.GetUniformLoc($"pointLights[{pointId}].pos"), pos.X, pos.Y, pos.Z);
                Vector4 col = pl.colour.GetZeroToOneA();
                GL.Uniform4(mat.shader.GetUniformLoc($"pointLights[{pointId}].col"), col.X, col.Y, col.Z, col.W);
                GL.Uniform1(mat.shader.GetUniformLoc($"pointLights[{pointId}].intensity"), pl.intensity);
                GL.Uniform1(mat.shader.GetUniformLoc($"pointLights[{pointId}].range"), pl.range);
                pointId++;
            }
        }
        mat.SetShaderParameter("numPointLights", pointId+1);
        mat.SetShaderParameter("cameraPos", Game.instance.camera.Position);
    }

    private static void AddDirectionalLights(Material mat)
    {
        GL GL = GLContext.gl;

        if (!mat.shader.HasUniform("numDirectionalLights")) { return; } // incase this isn't a lighting shader

        int pointId = 0;
        foreach (Light l in lights)
        {
            if (l is DirectionalLight pl)
            {
                GL.UseProgram((uint)mat.shader.Handle);
                Vector3 dir = ((Transform3D)pl.gameObject.transform).GetForward();
                GL.Uniform3(mat.shader.GetUniformLoc($"directionalLights[{pointId}].direction"), dir.X, dir.Y, dir.Z);
                Vector4 col = pl.colour.GetZeroToOneA();
                GL.Uniform4(mat.shader.GetUniformLoc($"directionalLights[{pointId}].col"), col.X, col.Y, col.Z, col.W);
                GL.Uniform1(mat.shader.GetUniformLoc($"directionalLights[{pointId}].intensity"), pl.intensity);
                pointId++;
            }
        }
        mat.SetShaderParameter("numDirectionalLights", pointId+1);
        mat.SetShaderParameter("cameraPos", Game.instance.camera.Position);
    }

    public static void AddLightingInfo(Material mat)
    {
        AddPointLights(mat);
        AddDirectionalLights(mat);
    }
}