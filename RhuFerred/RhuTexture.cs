using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;

namespace RhuFerred
{
	public class RhuTexture:IDisposable
	{
		private Texture _loadedTexture;
		public bool IsDisposed => _loadedTexture?.IsDisposed ?? true;

		public RhuTexture(Texture texture) {
			_loadedTexture = texture;
		}

		public Texture LoadedTexture
		{
			get => _loadedTexture;
			set {
				_loadedTexture?.Dispose();
				_loadedTexture = value;
				TextureReloadEvent?.Invoke();
			}
		}

		public event Action TextureReloadEvent;

		public uint Width => _loadedTexture?.Width ?? 0;

		public uint Height => _loadedTexture?.Height ?? 0;

		public void Dispose() {
			_loadedTexture?.Dispose();
		}

		public void ReloadTexture(Texture texture) {
			LoadedTexture = texture;
		}
	}
}
