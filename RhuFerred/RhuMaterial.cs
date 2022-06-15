using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	public class RhuMaterial : IDisposable
	{
		public RhuMaterial(Renderer renderer) {
			Renderer = renderer;
		}

		public void Init() {

		}

		public Renderer Renderer { get; }

		public void Dispose() {

		}
	}
}
