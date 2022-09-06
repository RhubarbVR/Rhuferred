using System;
using Veldrid;
using RhuFerred;
using System.Numerics;

namespace Tester
{
	public class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("Starting Render Tester!");
			var render = new Renderer(true);
			render.Init(render.CreateNewWindow("Main Cam"));
			var maincam = render.CreateCamera();
			maincam.WorldPos = Matrix4x4.CreateScale(1);
			render.FirstWindow.TargetCamera(maincam);
			render.FirstWindow.Sdl2Window.KeyDown += (KeyEvente) => {
				if (KeyEvente.Key == Key.F1) {
					Console.WriteLine("FPS:" + render.FPS);
					return;
				}
				var index = (GBufferTextures)(((int)KeyEvente.Key) - Key.Keypad0);
				if (index < 0) {
					return;
				}
				if ((int)index > 9) {
					return;
				}
				render.FirstWindow.LoadTexture(maincam.GetGBufferTexture(index));
				if (index > GBufferTextures.Depth) {
					Console.WriteLine("Changed window Texture to MainTexture");
				}
				else {
					Console.WriteLine($"Changed window Texture to {index}");
				}
			};
			var testMesh = render.LoadCube(0.25f);
			var mit = render.NewMaterial(render.BankShader);
			var meshRender = render.AttachMeshRender(testMesh, mit);
			meshRender.WorldPos = Matrix4x4.CreateTranslation(0, 0, -1);
			var meshRender2 = render.AttachMeshRender(testMesh, mit);
			meshRender2.WorldPos = Matrix4x4.CreateTranslation(0, 0, 1);
			while (render.Step()) { };
			render.Dispose();
		}
	}
}
