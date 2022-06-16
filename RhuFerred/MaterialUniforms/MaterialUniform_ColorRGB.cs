using Veldrid;

namespace RhuFerred.MaterialUniforms
{
	public class MaterialUniform_ColorRGB : MaterialUniformGen3<float>
	{
		public override void SetData(object value) {
			if (value is RgbaFloat rgbaFloat) {
				Value1 = rgbaFloat.R;
				Value2 = rgbaFloat.G;
				Value3 = rgbaFloat.B;
			}
			if (value is RgbaByte rgbaByte) {
				Value1 = (float)rgbaByte.R / 255;
				Value2 = (float)rgbaByte.G / 255;
				Value3 = (float)rgbaByte.B / 255;
			}
			else {
				base.SetData(value);
			}
		}
	}
}