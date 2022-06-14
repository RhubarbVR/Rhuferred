using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	[Flags]
	public enum RenderPass
	{
		None,
		Position = 1,
		Depth = 1,
		Normals = 2,
		Albdo = 4,
		Specular = 8,
		Shadows = 16,
		All = 1 | 2 | 4 | 8 | 16
	}
}
