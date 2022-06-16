using Veldrid;

namespace RhuFerred.MaterialUniforms
{
	public class MaterialUniform_ColorR: MaterialUniformGen1<float>
	{
		public override void SetData(object value) {
			if (value is RgbaFloat rgbaFloat) {
				Value = rgbaFloat.R;
			}
			if (value is RgbaByte rgbaByte) {
				Value = (float)rgbaByte.R / 255;
			}
			else {
				base.SetData(value);
			}
		}
	}
}