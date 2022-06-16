using Veldrid;

namespace RhuFerred.MaterialUniforms
{
	internal class MaterialUniform_ColorRG: MaterialUniformGen2<float>
	{
		public override void SetData(object value) {
			if (value is RgbaFloat rgbaFloat) {
				Value1 = rgbaFloat.R;
				Value2 = rgbaFloat.G;
			}
			if (value is RgbaByte rgbaByte) {
				Value1 = (float)rgbaByte.R / 255;
				Value2 = (float)rgbaByte.G / 255;
			}
			else {
				base.SetData(value);
			}
		}
	}
}