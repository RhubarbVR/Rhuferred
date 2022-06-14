using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;

namespace RhuFerred
{

	public class DeferredFrameBuffer : IDisposable
	{
		public void ClearColors(CommandList commandList) {
			commandList.ClearColorTarget(0,RgbaFloat.Black);
			commandList.ClearColorTarget(1, RgbaFloat.Black);
			commandList.ClearColorTarget(2, RgbaFloat.Black);
		}

		public DeferredFrameBuffer(Renderer renderer) {
			Renderer = renderer;
		}

		public Renderer Renderer { get; }

		public ResourceFactory Factory => Renderer.GraphicsDevice.ResourceFactory;
		public Texture Normals { get; private set; }
		public Texture Albdo { get; private set; }
		public Texture Specular { get; private set; }


		public Texture Depth { get; private set; }

		public Framebuffer Framebuffer { get; private set; }

		private void DistroyFrameBuffer() {
			Framebuffer?.Dispose();
			Framebuffer = null;
			Albdo?.Dispose();
			Albdo = null;
			Normals?.Dispose();
			Normals = null;
			Specular?.Dispose();
			Specular = null;
			Depth?.Dispose();
			Depth = null;
		}

		public void BuildFrameBuffer(uint width, uint hight) {
			Albdo = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm_SRgb, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Normals = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm_SRgb, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Specular = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Depth = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
			Framebuffer = Factory.CreateFramebuffer(new FramebufferDescription(Depth, Albdo, Normals, Specular));
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
