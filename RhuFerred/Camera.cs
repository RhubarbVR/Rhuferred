using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Newtonsoft.Json.Linq;

using Veldrid;

namespace RhuFerred
{
	public class Camera : IDisposable
	{
		public RgbaFloat ClearColor = RgbaFloat.CornflowerBlue;

		public Matrix4x4 View;

		public Matrix4x4 WorldPos
		{
			get {
				Matrix4x4.Invert(View, out var vakue);
				return vakue;
			}
			set =>
				Matrix4x4.Invert(value, out View);
		}

		public Matrix4x4 Projection;

		public Gbuffer gbuffer;
		public uint Width { get; private set; }
		public uint Height { get; private set; }
		public float Fov { get; private set; } = 60f;
		public void SetFov(float newFov = 60f) {
			Fov = newFov;
			UpdatePerspective();
		}
		public void SetPerspective(float newFov = 60f, float newNearClip = 0.01f, float newFarClip = 1000f) {
			Fov = newFov;
			FarClip = newFarClip;
			NearClip = newNearClip;
			UpdatePerspective();
		}

		public RhuTexture GetGBufferTexture(GBufferTextures index) {
			return index switch {
				GBufferTextures.Albdo => gbuffer.Albdo,
				GBufferTextures.Specular_Metallic => gbuffer.Specular_Metallic,
				GBufferTextures.Emission_AmbientOcclusion => gbuffer.Emission_AmbientOcclusion,
				GBufferTextures.Normals_Roughness => gbuffer.Normals_Roughness,
				GBufferTextures.SubSurfaces_DecalStencil => gbuffer.SubSurfaces_DecalStencil,
				GBufferTextures.Positions_UserData => gbuffer.Positions_UserData,
				GBufferTextures.Depth => gbuffer.Depth,
				_ => MainTexture,
			};
		}

		public float NearClip { get; private set; } = 0.01f;
		public void SetNearClip(float newNearClip = 0.01f) {
			NearClip = newNearClip;
			UpdatePerspective();
		}
		public float FarClip { get; private set; } = 1000f;
		public void SetFarClip(float newFarClip = 1000f) {
			FarClip = newFarClip;
			UpdatePerspective();
		}
		public void SetClips(float newNearClip = 0.01f, float newFarClip = 1000f) {
			FarClip = newFarClip;
			NearClip = newNearClip;
			UpdatePerspective();
		}

		public void Resize(uint width, uint height) {
			Width = width;
			Height = height;
			gbuffer.Resize(width, height);
			ReBuildMainFrameBuffer();
			UpdatePerspective();
		}

		private CommandList _commandList;

		public Camera(Renderer renderer, uint width, uint height) {
			Renderer = renderer;
			Width = width;
			Height = height;
			renderer.Cameras.Add(this);
			Initialize();
		}


		private unsafe void Initialize() {
			gbuffer = new Gbuffer(Renderer);
			gbuffer.BuildFrameBuffer(Width, Height);
			WorldPos = Matrix4x4.CreateScale(1f);
			ReBuildMainFrameBuffer();
			UpdatePerspective();
			_commandList = gbuffer.Factory.CreateCommandList();
		}

		private void UpdatePerspective() {
			Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * Fov, (float)Width / (float)Height, NearClip, FarClip);
			Renderer.Logger.Info($"Perspective Update On Camera Fov:{Fov} Width:{Width} Hight:{Height} NearClip:{NearClip} FarClip:{FarClip}");
		}

		public void Render() {
			_commandList.Begin();
			_commandList.SetFramebuffer(gbuffer.Framebuffer);
			_commandList.ClearDepthStencil(1f);
			gbuffer.ClearColors(_commandList);
			RunMainRenderPass();
			ProcesssFinal();
			_commandList.End();
			Renderer.MainGraphicsDevice.SubmitCommands(_commandList);
			Renderer.MainGraphicsDevice.WaitForIdle();
		}

		private void ProcesssFinal() {
			_commandList.SetFramebuffer(MainFramebuffer);
			_commandList.ClearColorTarget(0, ClearColor);
		}

		private void RunMainRenderPass() {
			Renderer.RenderedMeshes.ForEach((item) => item.Render(_commandList, this));
		}

		public void Destroy() {
			Renderer.Cameras.Remove(this);
			Dispose();
		}

		/// <summary>
		/// Should use <see cref="Destroy"/> this is only for internal use
		/// </summary>
		public void Dispose() {
			gbuffer.Dispose();
			_commandList.Dispose();
			_commandList = null;
		}

		public Renderer Renderer { get; }

		public RhuTexture MainTexture { get; private set; }

		public Framebuffer MainFramebuffer { get; private set; }

		private void ReBuildMainFrameBuffer() {
			ReloadFinalTexture();
			MainFramebuffer?.Dispose();
			MainFramebuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, MainTexture.LoadedTexture));
		}

		private void ReloadFinalTexture() {
			var texture = Renderer.MainGraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(Width, Height, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
			MainTexture?.ReloadTexture(texture);
			MainTexture ??= new RhuTexture(texture);
		}
	}
}
