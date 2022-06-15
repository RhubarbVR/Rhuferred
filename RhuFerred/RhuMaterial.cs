using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	public class RhuMaterial : IDisposable
	{
		private RhuShader _rhuShader;

		public RhuShader RhuShader
		{
			get => _rhuShader;
			set { UpdateShaderInfo(value);  _rhuShader = value;  }
		}

		private void UpdateShaderInfo(RhuShader rhuShader) {
			if(_rhuShader is not null) {
				Renderer.Logger.Info("Removing Last Shader");
				return;
			}
			Renderer.Logger.Info("Loading Shader");

		}

		public RhuMaterial(Renderer renderer) {
			Renderer = renderer;
		}

		public RhuMaterial(Renderer renderer, RhuShader targetShader) {
			Renderer = renderer;
			RhuShader = targetShader;
		}

		public Renderer Renderer { get; }

		public void Dispose() {

		}
	}
}
