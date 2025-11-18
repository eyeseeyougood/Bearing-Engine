using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing
{
    public class Shader
    {
        public readonly uint Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public string vert { get; set; }
        public string frag { get; set; }

        private Shader(uint handle, Dictionary<string, int> uniformLocations) { Handle = handle; _uniformLocations = uniformLocations; }
        public Shader(string vert, string frag)
        {
            GL GL = GLContext.gl;

            this.vert = vert;
            this.frag = frag;

            var shaderSource = Resources.ReadAllText(Resource.GetShader(vert, true));

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            
            GL.ShaderSource(vertexShader, shaderSource);

            CompileShader(vertexShader);

            shaderSource = Resources.ReadAllText(Resource.GetShader(frag, true));
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(Handle, ProgramPropertyARB.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, (uint)i, out _, out _);

                var location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);
            }
        }

        public static Shader FromResources(Resource vert, Resource frag)
        {
            GL GL = GLContext.gl;
            var shaderSource = Resources.ReadAllText(vert);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertexShader, shaderSource);

            CompileShader(vertexShader);

            shaderSource = Resources.ReadAllText(frag);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            uint nHandle = GL.CreateProgram();

            GL.AttachShader(nHandle, vertexShader);
            GL.AttachShader(nHandle, fragmentShader);

            LinkProgram(nHandle);

            GL.DetachShader(nHandle, vertexShader);
            GL.DetachShader(nHandle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(nHandle, ProgramPropertyARB.ActiveUniforms, out var numberOfUniforms);

            Dictionary<string, int> dict = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(nHandle, (uint)i, out _, out _);

                var location = GL.GetUniformLocation(nHandle, key);

                dict.Add(key, location);
            }

            Shader result = new Shader(nHandle, dict);
            result.vert = vert.fullpath;
            result.frag = frag.fullpath;

            return result;
        }

        public bool HasUniform(string val)
        {
            return _uniformLocations.ContainsKey(val);
        }

        public int GetUniformLoc(string val)
        {
            return _uniformLocations[val];
        }

        private static void CompileShader(uint shader)
        {
            GL GL = GLContext.gl;
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameterName.CompileStatus, out var code);
            if (code != (int)GLEnum.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(uint program)
        {
            GL GL = GLContext.gl;

            GL.LinkProgram(program);

            GL.GetProgram(program, ProgramPropertyARB.LinkStatus, out var code);
            if (code != (int)GLEnum.True)
            {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        public uint GetHandle()
        {
            return Handle;
        }

        public void Use()
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            GL GL = GLContext.gl;
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
            float[] all = new float[16]
            {
                data.M11, data.M12, data.M13, data.M14,
                data.M21, data.M22, data.M23, data.M24,
                data.M31, data.M32, data.M33, data.M34,
                data.M41, data.M42, data.M43, data.M44
            };
            GL.UniformMatrix4(_uniformLocations[name], true, new ReadOnlySpan<float>(all));
        }

        public void SetVector2(string name, Vector2 data)
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
            GL.Uniform2(_uniformLocations[name], data.X, data.Y);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data.X, data.Y, data.Z);
        }

        public void SetVector4(string name, Vector4 data)
        {
            GL GL = GLContext.gl;
            GL.UseProgram(Handle);
            GL.Uniform4(_uniformLocations[name], data.X, data.Y, data.Z, data.W);
        }

        public void Cleanup()
        {
            GL GL = GLContext.gl;
            GL.DeleteProgram(Handle);
        }
    }
}