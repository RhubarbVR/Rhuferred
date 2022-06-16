using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;

namespace RhuFerred
{
	public enum GBufferTextures
	{
		Albdo,
		Specular_Metallic,
		Emission_AmbientOcclusion,
		Normals_Roughness,
		SubSurfaces_DecalStencil,
		Positions_UserData,
		Depth,
	}
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



		// Albdo:4
		public RhuTexture Albdo { get; private set; }
		// Specular:3 Metallic:1
		public RhuTexture Specular_Metallic { get; private set; }
		// Emission:3 Ambient Occlusion:1 
		public RhuTexture Emission_AmbientOcclusion { get; private set; }
		// Normals:3  Roughness:1
		public RhuTexture Normals_Roughness { get; private set; }
		// SubSurfaces:3 DecalStencil : 1
		public RhuTexture SubSurfaces_DecalStencil { get; private set; }
		// Position: 3 UserData: 1
		public RhuTexture Positions_UserData { get; private set; }

		public RhuTexture Depth { get; private set; }

		public Framebuffer Framebuffer { get; private set; }

		private void DistroyFrameBuffer() {
			Framebuffer?.Dispose();
			Framebuffer = null;
			Albdo?.Dispose();
			Albdo = null;
			Specular_Metallic?.Dispose();
			Specular_Metallic = null;
			Emission_AmbientOcclusion?.Dispose();
			Emission_AmbientOcclusion = null;
			Normals_Roughness?.Dispose();
			Normals_Roughness = null;
			SubSurfaces_DecalStencil?.Dispose();
			SubSurfaces_DecalStencil = null;
			Positions_UserData?.Dispose();
			Positions_UserData = null;
			Depth?.Dispose();
			Depth = null;
		}

		public void BuildFrameBuffer(uint width, uint hight) {
			var newAlbdo = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Albdo?.ReloadTexture(newAlbdo);
			Albdo ??= new RhuTexture(newAlbdo);
			var newSpecular_Metallic = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Specular_Metallic?.ReloadTexture(newSpecular_Metallic);
			Specular_Metallic ??= new RhuTexture(newSpecular_Metallic);
			var newEmission_AmbientOcclusion = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Emission_AmbientOcclusion?.ReloadTexture(newEmission_AmbientOcclusion);
			Emission_AmbientOcclusion ??= new RhuTexture(newEmission_AmbientOcclusion);
			var newNormals_Roughness = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Normals_Roughness?.ReloadTexture(newNormals_Roughness);
			Normals_Roughness ??= new RhuTexture(newNormals_Roughness);
			var newSubSurfaces_DecalStencil = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			SubSurfaces_DecalStencil?.ReloadTexture(newSubSurfaces_DecalStencil);
			SubSurfaces_DecalStencil ??= new RhuTexture(newSubSurfaces_DecalStencil);
			var newPositions_UserData = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			Positions_UserData?.ReloadTexture(newPositions_UserData);
			Positions_UserData ??= new RhuTexture(newPositions_UserData);
			var newDepth = Factory.CreateTexture(TextureDescription.Texture2D(width, hight, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
			Depth ??= new RhuTexture(newDepth);
			Framebuffer?.Dispose();
			Framebuffer = Factory.CreateFramebuffer(new FramebufferDescription(newDepth, newAlbdo, newSpecular_Metallic, newEmission_AmbientOcclusion, newNormals_Roughness, newSubSurfaces_DecalStencil, newPositions_UserData));
		}

		public void Resize(uint width, uint hight) {
			BuildFrameBuffer(width, hight);
		}

		public void Dispose() {
			DistroyFrameBuffer();
		}
	}
}
