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
			var render = new Renderer(GraphicsBackend.Vulkan);
			render.Init(render.CreateNewWindow("Main Cam"));
			var maincam = render.CreateCamera();
			render.FirstWindow.TargetCamera(maincam);
			render.FirstWindow.Sdl2Window.KeyDown += (KeyEvente) => {
				if (KeyEvente.Key == Key.F1) {
					Console.WriteLine("FPS:" + render.FPS);
				}
			};
			var testMesh = new RhuMesh();
			var meshRender = render.AttachMeshRender(testMesh, render.NewMaterial(render.MainShader));
			static Matrix4x4 GetMatrix(float scale, Quaternion rot, float x, float y, float z) {
				return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(x, y, z);
			}
			var speen = 0.0;
			while (render.Step(() => {
				speen += render.DeltaTime;
				meshRender.WorldPos = GetMatrix(1,Quaternion.Identity,0,0,3) * GetMatrix(1, Quaternion.CreateFromYawPitchRoll((float)(speen/10),0, (float)(speen / 20)), 0, 0, 0);
			})) { };
			render.Dispose();
		}
	}
}
