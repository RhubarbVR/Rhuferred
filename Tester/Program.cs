using System;
using Veldrid;
using RhuFerred;

namespace Tester
{
	public class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("Starting Render Tester!");
			var render = new Renderer();
			render.Init(render.CreateNewWindow("Main Cam"));
			var maincam = render.CreateCamera();
			render.FirstWindow.TargetCamera(maincam);
			var camTwo = render.CreateCamera();
			camTwo.ClearColor = RgbaFloat.Orange;
			render.CreateNewWindow("Camera two").TargetCamera(camTwo);
			render.FirstWindow.Sdl2Window.KeyDown += (KeyEvente) => {
				if (KeyEvente.Key == Key.F1) {
					Console.WriteLine("FPS:" + render.FPS);
				}
			};
			while (render.Step(() => {})) { };
			render.Dispose();
		}
	}
}
