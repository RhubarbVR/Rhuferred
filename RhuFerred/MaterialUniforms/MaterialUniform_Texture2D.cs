using Veldrid;
using Veldrid.ImageSharp;

namespace RhuFerred.MaterialUniforms
{
	public class MaterialUniform_Texture2D : MaterialUniform
	{
		private Texture _default;
		private RhuTexture _rhuTexture;
		public override int SizeInBytes => 0;

		public override object GetData() {
			return _rhuTexture;
		}

		public override void SetData(object value) {
			if (_rhuTexture is not null) {
				_rhuTexture.TextureReloadEvent -= RhuTexture_TextureReloadEvent;
			}
			if(value is RhuTexture texture) {
				_rhuTexture = texture;
			}
			if (_rhuTexture is null) {
				return;
			}
			_rhuTexture.TextureReloadEvent += RhuTexture_TextureReloadEvent;
			RhuTexture_TextureReloadEvent();
		}

		private void RhuTexture_TextureReloadEvent() {
			TargetResource = _rhuTexture is null
				? _default
				: _rhuTexture.LoadedTexture?.IsDisposed ?? true ? RhuMaterial.Renderer.NullTexture : (BindableResource)_rhuTexture.LoadedTexture;
		}

		public override void SetDefaults(string defaults) {
			defaults = defaults.ToLower();
			var color = defaults switch {
				"white" => RgbaFloat.White,
				"red" => RgbaFloat.Red,
				"darkred" => RgbaFloat.DarkRed,
				"green" => RgbaFloat.Green,
				"blue" => RgbaFloat.Blue,
				"yellow" => RgbaFloat.Yellow,
				"grey" => RgbaFloat.Grey,
				"lightgrey" => RgbaFloat.LightGrey,
				"cyan" => RgbaFloat.Cyan,
				"cornflowerblue" => RgbaFloat.CornflowerBlue,
				"clear" => RgbaFloat.Clear,
				"black" => RgbaFloat.Black,
				"pink" => RgbaFloat.Pink,
				"orange" => RgbaFloat.Orange,
				_ => RgbaFloat.White,
			};
			_default?.Dispose();
			var tmemp = new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, color), false);
			_default = tmemp.CreateDeviceTexture(RhuMaterial.Renderer.MainGraphicsDevice, RhuMaterial.Renderer.MainGraphicsDevice.ResourceFactory);
			tmemp.Images[0].Dispose();
		}

		public override void CreateDeviceResource() {
			RhuTexture_TextureReloadEvent();
		}

		public override void UpdateBuffer() {
			RhuTexture_TextureReloadEvent();
		}

		public override bool CanDispose => false;
	}
}