using Silk.NET.OpenGL;

namespace Bearing;

public class ScreenTexture
{
	private uint fbo;
	private uint colTex;
	private uint depTex;

	private ScreenTexture() {}

	public static ScreenTexture CreateEmpty()
	{
		ScreenTexture result = new ScreenTexture();

	    GL GL = GLContext.gl;

		result.fbo = GL.GenFramebuffer();

	    result.Bind();

	    result.colTex = CreateColour();

	    result.depTex = CreateDepth();

	    GLEnum status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
	    if (status != GLEnum.FramebufferComplete)
		{
		    Logger.LogError("failed to create a frame buffer for depth texture! Status: " + status);
		}

		result.Unbind();

		return result;
	}

	private static unsafe uint CreateColour()
	{
	    GL GL = GLContext.gl;
		uint colorTex;
		colorTex = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2D, colorTex);

		GL.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba8, (uint)Game.instance.ClientSize.X, (uint)Game.instance.ClientSize.Y, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTex, 0);

		return colorTex;
	}

	private static unsafe uint CreateDepth()
	{
	    GL GL = GLContext.gl;
		uint depthTex;
		depthTex = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2D, depthTex);

		GL.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.DepthComponent24, (uint)Game.instance.ClientSize.X, (uint)Game.instance.ClientSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		GL.BindTexture(TextureTarget.Texture2D, 0);

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTex, 0);

		return depthTex;
	}

	private unsafe void Resize(uint width, uint height)
	{
		GL GL = GLContext.gl;

		GL.BindTexture(GLEnum.Texture2D, depTex);
		GL.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, null);

		GL.BindTexture(GLEnum.Texture2D, colTex);
		GL.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba8, width, height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
		GL.BindTexture(GLEnum.Texture2D, 0);
	}

	public unsafe void Bind()
	{
		GL GL = GLContext.gl;

		// rescale textures incase window has resized since creation
		// TODO: OPTIMISATION - maybe this could be faster by first checking the size and seeing if the window size is different
		Resize((uint)Game.instance.ClientSize.X, (uint)Game.instance.ClientSize.Y);

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
	}

	public void Unbind()
	{
		GLContext.gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public uint GetColourHandle()
	{
		return colTex;
	}

	public uint GetDepthHandle()
	{
		return depTex;
	}

	public void Use(TextureUnit colourUnit, TextureUnit depthUnit)
	{
		GL GL = GLContext.gl;
		GL.ActiveTexture(colourUnit);
        GL.BindTexture(TextureTarget.Texture2D, colTex);

        GL.ActiveTexture(depthUnit);
        GL.BindTexture(TextureTarget.Texture2D, depTex);
	}

	public byte[] GetColourData()
	{
		GL GL = GLContext.gl;

		GL.BindTexture(TextureTarget.Texture2D, colTex);
		int width, height;
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureWidth,out width);
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureHeight,out height);

	    byte[] data = new byte[width * height * 4];
		unsafe
		{
			fixed(void* pix = data)
			{
				GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pix);
			}
		}
		GL.BindTexture(TextureTarget.Texture2D, 0);

		return data;
	}

	public byte[] GetDepthData()
	{
		GL GL = GLContext.gl;

		GL.BindTexture(TextureTarget.Texture2D, depTex);
		int width, height;
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureWidth,out width);
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureHeight,out height);

	    byte[] data = new byte[width * height * 4];
		unsafe
		{
			fixed(void* pix = data)
			{
				GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pix);
			}
		}
		GL.BindTexture(TextureTarget.Texture2D, 0);

		return data;
	}

	///<summary>
	///This function creates a new texture everytime it is called, hence usage of this function should be minimised as much as possible.
	///</summary>
	public Texture ExtractColourTexture()
	{
		GL GL = GLContext.gl;
		GL.BindTexture(TextureTarget.Texture2D, colTex);
		int width, height;
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureWidth,out width);
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureHeight,out height);

		return Texture.FromData(
				width, height, GetColourData(),
				TextureWrapMode.ClampToEdge,
				TextureMinFilter.Nearest,
				TextureMagFilter.Nearest
			);
	}

	///<summary>
	///This function creates a new texture everytime it is called, hence usage of this function should be minimised as much as possible.
	///</summary>
	public Texture ExtractDepthTexture()
	{
		GL GL = GLContext.gl;
		GL.BindTexture(TextureTarget.Texture2D, depTex);
		int width, height;
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureWidth,out width);
	    GL.GetTexLevelParameter(TextureTarget.Texture2D,0,GetTextureParameter.TextureHeight,out height);

		return Texture.FromData(
				width, height, GetDepthData(),
				TextureWrapMode.ClampToEdge,
				TextureMinFilter.Nearest,
				TextureMagFilter.Nearest
			);
	}

	public void Dispose()
	{
		GLContext.gl.DeleteFramebuffer(fbo);
		GLContext.gl.DeleteTexture(colTex);
		GLContext.gl.DeleteTexture(depTex);
	}
}