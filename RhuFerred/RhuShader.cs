using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;
using Veldrid.SPIRV;

namespace RhuFerred
{
	public static class RhuShaderStatics
	{
		public const string ADDEDCODE_MAIN_FRAG_SHADER_CODE =
@"
#version 450

layout(location = 0) in vec4 In_Color;

layout(location = 0) out vec4 Out_AlbdoSpec;
layout(location = 1) out vec4 Out_Normals;
layout(location = 2) out vec4 Out_Positions;
layout(location = 3) out vec4 Out_Color;


";
		public const string ADDEDCODE_MAIN_VERT_SHADER_CODE =
@"
#version 450

layout(location = 0) in vec3 In_Position;
layout(location = 1) in vec4 In_Color;
layout(location = 0) out vec4 Out_Color;

";
		public const string ADDEDCODE_SHADOW_FRAG_SHADER_CODE =
@"
#version 450
layout(location = 0) out vec4 Out_Shadow;

";
		public const string ADDEDCODE_SHADOW_VERT_SHADER_CODE =
@"
#version 450

layout(location = 0) in vec3 In_Position;

";

		public const string MAIN_FRAG_SHADER_CODE =
@"
void main()
{
    Out_AlbdoSpec = In_Color;
    Out_Normals = In_Color;
    Out_Positions = In_Color;
    Out_Color = In_Color;
}
";
		public const string MAIN_VERT_SHADER_CODE =
@"
void main()
{
    gl_Position = vec4(In_Position, 1);
	Out_Color = vec4(1);
}
";
		public const string SHADOW_FRAG_SHADER_CODE =
@"
void main()
{
    Out_Shadow = vec4(1);
}
";
		public const string SHADOW_VERT_SHADER_CODE =
@"
void main()
{
    gl_Position = vec4(In_Position, 1);
}
";
	}

	public class RhuShader : IDisposable
	{

		public bool LoadedShader { get; private set; }

		public string ShaderName { get; private set; }

		public RhuShader(Renderer renderer) {
			Renderer = renderer;
		}
		public RhuShader(Renderer renderer, RhuRawShaderData rhuRawShaderData) {
			Renderer = renderer;
			Init(rhuRawShaderData);
		}

		public Shader[] MainShaders { get; private set; }
		public Shader[] ShadowShaders { get; private set; }

		public void Init(RhuRawShaderData rhuRawShaderData) {
			Renderer.Logger.Info($"Loading Shader:{rhuRawShaderData.ShaderName} With {rhuRawShaderData.shaderUniforms.Length} shaderUniforms");
			ShaderName = rhuRawShaderData.ShaderName;
			var MainFragShaderCode = RhuShaderStatics.ADDEDCODE_MAIN_FRAG_SHADER_CODE;
			var MainVertShaderCode = RhuShaderStatics.ADDEDCODE_MAIN_VERT_SHADER_CODE;
			var ShadowFragShaderCode = RhuShaderStatics.ADDEDCODE_SHADOW_FRAG_SHADER_CODE;
			var ShadowVertShaderCode = RhuShaderStatics.ADDEDCODE_SHADOW_VERT_SHADER_CODE;
			MainFragShaderCode += rhuRawShaderData.MainFragShaderCode ?? RhuShaderStatics.MAIN_FRAG_SHADER_CODE;
			MainVertShaderCode += rhuRawShaderData.MainVertShaderCode ?? RhuShaderStatics.MAIN_VERT_SHADER_CODE;
			ShadowFragShaderCode += rhuRawShaderData.ShadowFragShaderCode ?? RhuShaderStatics.SHADOW_FRAG_SHADER_CODE;
			ShadowVertShaderCode += rhuRawShaderData.ShadowVertShaderCode ?? RhuShaderStatics.SHADOW_VERT_SHADER_CODE;

			var mainVertexShaderDesc = new ShaderDescription(
				ShaderStages.Vertex,
				Encoding.UTF8.GetBytes(MainVertShaderCode),
				"main");
			var mainFragmentShaderDesc = new ShaderDescription(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(MainFragShaderCode),
				"main");
			MainShaders = Renderer.MainGraphicsDevice.ResourceFactory.CreateFromSpirv(mainVertexShaderDesc, mainFragmentShaderDesc);


			var shadowVertexShaderDesc = new ShaderDescription(
				ShaderStages.Vertex,
				Encoding.UTF8.GetBytes(ShadowVertShaderCode),
				"main");
			var shadowFragmentShaderDesc = new ShaderDescription(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(ShadowFragShaderCode),
				"main");
			ShadowShaders = Renderer.MainGraphicsDevice.ResourceFactory.CreateFromSpirv(mainVertexShaderDesc, mainFragmentShaderDesc);

			for (var i = 0; i < rhuRawShaderData.shaderUniforms.Length; i++) {
				var uniform = rhuRawShaderData.shaderUniforms[i];
			
			}
			LoadedShader = true;
		}

		public Renderer Renderer { get; }

		public void Dispose() {
			if (MainShaders is not null) {
				foreach (var item in MainShaders) {
					item.Dispose();
				}
			}
			MainShaders = null;
			if (ShadowShaders is not null) {
				foreach (var item in ShadowShaders) {
					item.Dispose();
				}
			}
			ShadowShaders = null;
		}
	}
}
