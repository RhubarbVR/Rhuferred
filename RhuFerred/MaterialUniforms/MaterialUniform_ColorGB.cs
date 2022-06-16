using Veldrid;
namespace RhuFerred.MaterialUniforms
{
	public class MaterialUniform_ColorGB: MaterialUniformGen2<float>
	{
		public override void SetData(object value) {
			if(value is RgbaFloat rgbaFloat) {
				Value1 = rgbaFloat.G;
				Value2 = rgbaFloat.B;
			}
			if (value is RgbaByte rgbaByte) {
				Value1 = (float)rgbaByte.G/255;
				Value2 = (float)rgbaByte.B/255;
			}
			else {
				base.SetData(value);
			}
		}
	}
}