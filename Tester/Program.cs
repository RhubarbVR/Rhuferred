using System;
using Veldrid;
using RhuFerred;

namespace Tester
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Render Tester!");
			var render = new Renderer();
			render.Init(Renderer.CreateBasicWindow());
			var camra = new Camera(render, 960, 540);
			while(render.Step(() => { })) { };
			render.Dispose();
        }
    }
}
