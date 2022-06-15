using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;

namespace RhuFerred
{

	public class Gbuffer : IDisposable
	{
		public void ClearColors(CommandList commandList) {
			commandList.ClearColorTarget(0, RgbaFloat.Black);
			commandList.ClearColorTarget(1, RgbaFloat.Black);
			commandList.ClearColorTarget(2, RgbaFloat.Black);
		}

		public Gbuffer(Renderer renderer) {
			Renderer = renderer;
		}

		public Renderer Renderer { get; }

		public ResourceFactory Factory => Renderer.MainGraphicsDevice.ResourceFactory;
		public Texture Positions { get; private set; }

		public Texture Normals { get; private set; }
		public Texture AlbdoSpec { get; private set; }


		public Texture Depth { get; private set; }

		public Framebuffer Framebuffer { get; private set; }

		private void DistroyFrameBuffer() {
			Framebuffer?.Dispose();
			Framebuffer = null;
			AlbdoSpec?.Dispose();
			AlbdoSpec = null;
			Normals?.Dispose();
			Normals = null;
			Positions?.Dispose();
			Positions = null;
			Depth?.Dispose();
			Depth = null;
		}

		public void BuildFrameBuffer(uint width, uint hight) {
			AlbdoSpec = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Normals = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Positions = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Depth = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
			Framebuffer = Factory.CreateFramebuffer(new FramebufferDescription(Depth, AlbdoSpec, Normals, Positions));
		}

		public void Resize(uint width, uint hight) {
			DistroyFrameBuffer();
			BuildFrameBuffer(width, hight);
		}

		public void Dispose() {
			DistroyFrameBuffer();
		}
	}
}
