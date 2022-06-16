using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RhuFerred
{
	public static class ImageSharpExtensions
	{
		public unsafe static void UpdateTexture(this Texture tex, ImageSharpTexture update, GraphicsDevice gd, ResourceFactory factory) {

			if (!(update.Width == tex.Width && update.Height == tex.Height && update.MipLevels == tex.MipLevels)) {
				throw new Exception("Not Same");
			}

			var staging = factory.CreateTexture(
				TextureDescription.Texture2D(update.Width, update.Height, update.MipLevels, 1, update.Format, TextureUsage.Staging));

			var ret = tex;

			var cl = gd.ResourceFactory.CreateCommandList();
			cl.Begin();
			for (uint level = 0; level < update.MipLevels; level++) {
				var image = update.Images[level];
				if (!image.TryGetSinglePixelSpan(out var pixelSpan)) {
					throw new VeldridException("Unable to get image pixelspan.");
				}
				fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan)) {
					var map = gd.Map(staging, MapMode.Write, level);
					var rowWidth = (uint)(image.Width * 4);
					if (rowWidth == map.RowPitch) {
						Unsafe.CopyBlock(map.Data.ToPointer(), pin, (uint)(image.Width * image.Height * 4));
					}
					else {
						for (uint y = 0; y < image.Height; y++) {
							var dstStart = (byte*)map.Data.ToPointer() + (y * map.RowPitch);
							var srcStart = (byte*)pin + (y * rowWidth);
							Unsafe.CopyBlock(dstStart, srcStart, rowWidth);
						}
					}
					gd.Unmap(staging, level);

					cl.CopyTexture(
						staging, 0, 0, 0, level, 0,
						ret, 0, 0, 0, level, 0,
						(uint)image.Width, (uint)image.Height, 1, 1);

				}
			}
			cl.End();

			gd.SubmitCommands(cl);
			staging.Dispose();
			cl.Dispose();

		}

		public static Image<Rgba32> CreateTextureColor(int Width, int Height, RgbaFloat color) {
			var from = new Rgba32(color.R, color.G, color.B, color.A);
			var img = new Image<Rgba32>(Width, Height);
			for (var i = 0; i < img.Height; i++) {
				for (var wi = 0; wi < img.Width; wi++) {
					img[i, wi] = from;
				}
			}
			return img;
		}

		public unsafe static void UpdateTextureColor(this Texture tex, RgbaFloat color, GraphicsDevice gd, ResourceFactory factory) {
			var from = new Color(new Rgba32(color.R, color.G, color.B, color.A));
			var img = new Image<Rgba32>((int)tex.Width, (int)tex.Height);
			for (var i = 0; i < img.Height; i++) {
				foreach (var item in img.GetPixelRowSpan(i)) {
					item.FromRgba32(from);
				}
			}
			tex.UpdateTexture(new ImageSharpTexture(img, false), gd, factory);
		}

	}
}