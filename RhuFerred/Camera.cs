using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Veldrid;

namespace RhuFerred
{
	public class Camera:IDisposable
	{
		public Matrix4x4 WorldPos;

		public Matrix4x4 Perspective;

		public DeferredFrameBuffer deferredFrame;
		public uint Width { get; private set; }
		public uint Hight { get; private set; }

		private CommandList _commandList;

		public Camera(Renderer renderer,uint width,uint hight) {
			Renderer = renderer;
			Width = width;
			Hight = hight;
			renderer.Cameras.Add(this);
			Initialize();
		}

		private void Initialize() {
			deferredFrame = new DeferredFrameBuffer(Renderer);
			deferredFrame.BuildFrameBuffer(Width, Hight);
			WorldPos = Matrix4x4.CreateScale(1f);
			Perspective = Matrix4x4.CreatePerspective(1.047197f, (float)Width / (float)Hight, 0.01f, 1000f);
			_commandList = deferredFrame.Factory.CreateCommandList();
		}

		public void Render() {
			_commandList.Begin();
			_commandList.SetFramebuffer(deferredFrame.Framebuffer);
			_commandList.ClearDepthStencil(1f);
			//Add fancy stuff here
			_commandList.End();
			Renderer.GraphicsDevice.SubmitCommands(_commandList);
		}

		public void Destroy() {
			Renderer.Cameras.Remove(this);
			Dispose();
		}
		public void Dispose() {
			deferredFrame.Dispose();
			_commandList.Dispose();
			_commandList = null;
		}

		

		public Renderer Renderer { get; }


	}
}
