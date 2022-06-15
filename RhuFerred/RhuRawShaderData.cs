using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;

namespace RhuFerred
{
	public enum ShaderTypes
	{
		None,
		MainFragShaderCode,
		MainVertShaderCode,
		MainGeometryShaderCode,
		ShadowFragShaderCode,
		ShadowVertShaderCode,
		ShadowGeometryShaderCode
	}

	public enum UniformType
	{
		Unknown,
		Texture1D,
		Texture2D,
		Texture3D,
		ColorR,
		ColorRG,
		ColorGB,
		ColorRGB,
		ColorRGBA,
		Float,
		Float2,
		Float3,
		Float4,
		Double,
		Double2,
		Double3,
		Double4,
		Matrix2x2,
		Matrix2x3,
		Matrix2x4,
		Matrix3x2,
		Matrix3x3,
		Matrix3x4,
		Matrix4x2,
		Matrix4x3,
		Matrix4x4,
		Int,
		Int2,
		Int3,
		Int4,
		UInt,
		UInt2,
		UInt3,
		UInt4,
		Short,
		Short2,
		Short3,
		Short4,
		UShort1,
		UShort2,
		UShort3,
		UShort4,
		bool1,
		bool2,
		bool3,
		bool4,
	}

	public struct ShaderUniform
	{
		public string Defaults;
		public UniformType Type;
		public string FieldName;
		public string Name;
	}

	public struct RhuRawShaderData
	{
		public string ShaderName;

		public ShaderUniform[] shaderUniforms;

		public string MainFragShaderCode;

		public string MainVertShaderCode;

		public string ShadowFragShaderCode;

		public string ShadowVertShaderCode;

	}
}
