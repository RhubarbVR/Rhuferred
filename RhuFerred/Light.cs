using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	public class Light:IDisposable
	{
		public Light(Renderer renderer) {
			Renderer = renderer;
			Renderer.Lights.Add(this);
		}

		public Renderer Renderer { get; }

		public void Dispose() {
			Renderer.Lights.Remove(this);
		}
	}
}
