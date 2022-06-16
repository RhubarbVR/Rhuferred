using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	public static class ShaderCode
	{
		public const string BLANKSHADER =
@"
shader""Blank Shader""{
	uniforms{
	}
}
";
		public const string MAINSHADER =
@"
shader""Main Shader""{
	uniforms{
		_MainTexture(""Main Color"",texture2d) = ""White"";
	}
}
";
	}
}
