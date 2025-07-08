using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenTK.Graphics.OpenGL4;

namespace Bearing;

public static class LightManager
{
    public static List<Light> lights = new List<Light>(100);

    public static void AddLight(Light light)
    {
        lights.Add(light);
    }

    public static void AddLightingInfo(Material mat)
    {
        if (!mat.shader.HasUniform("numPointLights")) { return; } // incase this isn't a lighting shader

        int pointId = 0;
        foreach (Light l in lights)
        {
            if (l is PointLight pl)
            {
                GL.UseProgram(mat.shader.Handle);
                GL.Uniform3(mat.shader.GetUniformLoc($"pointLights[{pointId}].pos"), pl.gameObject.transform.position);
                GL.Uniform4(mat.shader.GetUniformLoc($"pointLights[{pointId}].col"), pl.colour.GetZeroToOneA());
                GL.Uniform1(mat.shader.GetUniformLoc($"pointLights[{pointId}].intensity"), pl.intensity);
                GL.Uniform1(mat.shader.GetUniformLoc($"pointLights[{pointId}].range"), pl.range);
                pointId++;
            }
        }
        mat.SetShaderParameter(new ShaderParam("numPointLights", pointId+1));
        mat.SetShaderParameter(new ShaderParam("cameraPos", Game.instance.camera.Position));
    }
}