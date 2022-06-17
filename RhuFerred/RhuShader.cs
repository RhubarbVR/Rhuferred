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
layout(location = 1) in vec3 In_Tangent;
layout(location = 2) in vec3 In_Normal;
layout(location = 3) in vec2 In_UV1;
layout(location = 4) in vec2 In_UV2;
layout(location = 5) in vec2 In_UV3;
layout(location = 6) in vec2 In_UV4;
layout(location = 7) in flat uint In_MaterialIndex;

layout(location = 0) out vec4 Out_Albdo;
layout(location = 1) out vec4 Out_Specular_Metallic;
layout(location = 2) out vec4 Out_Emission_AmbientOcclusion;
layout(location = 3) out vec4 Out_Normals_Roughness;
layout(location = 4) out vec4 Out_SubSurfaces_DecalStencil;
layout(location = 5) out vec4 Out_Positions_UserData;


";
		public const string VERTDATA =
@"
layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec3 Tangent;
layout(location = 3) in vec3 Normal;
layout(location = 4) in vec2 UV1;
layout(location = 5) in vec2 UV2;
layout(location = 6) in vec2 UV3;
layout(location = 7) in vec2 UV4;
layout(location = 8) in ivec4 Bones;
";
		public const string VERTADDEDCODE =
@"

layout(set = 0, binding = 0) uniform WorldData
{
	mat4 Projection;
    mat4 View;
    mat4 World;
	uint MaterialIndex;
};

layout(location = 0) out vec4 Out_Color;
layout(location = 1) out vec3 Out_Tangent;
layout(location = 2) out vec3 Out_Normal;
layout(location = 3) out vec2 Out_UV1;
layout(location = 4) out vec2 Out_UV2;
layout(location = 5) out vec2 Out_UV3;
layout(location = 6) out vec2 Out_UV4;
layout(location = 7) out flat uint Out_MaterialIndex;

vec4 GetVertexPos() {
    return Projection * View * World * vec4(Position, 1);
}

void PassThrough() {
	Out_Color = Color;
	Out_Tangent = Tangent;
	Out_Normal = Normal;
	Out_UV1 = UV1;
	Out_UV2 = UV2;
	Out_UV3 = UV3;
	Out_UV4 = UV4;
	Out_MaterialIndex = MaterialIndex;
}

";


		public const string ADDEDCODE_MAIN_VERT_SHADER_CODE =
@$"
#version 450
{VERTDATA}
{VERTADDEDCODE}
";
		public const string ADDEDCODE_SHADOW_FRAG_SHADER_CODE =
@"
#version 450

layout(location = 0) in vec4 In_Color;
layout(location = 1) in vec3 In_Tangent;
layout(location = 2) in vec3 In_Normal;
layout(location = 3) in vec2 In_UV1;
layout(location = 4) in vec2 In_UV2;
layout(location = 5) in vec2 In_UV3;
layout(location = 6) in vec2 In_UV4;

layout(location = 0) out vec4 Out_Shadow;

";
		public const string ADDEDCODE_SHADOW_VERT_SHADER_CODE =
@$"
#version 450
{VERTDATA}
{VERTADDEDCODE}


";

		public const string MAIN_FRAG_SHADER_CODE =
@"
void main()
{
    Out_Albdo = In_Color;
	Out_Normals_Roughness =  vec4(normalize(In_Normal),1);
	Out_Positions_UserData = vec4(gl_FragCoord.xyz,0);
	Out_Specular_Metallic  = vec4(0.2);
	Out_Emission_AmbientOcclusion = vec4(0.3);
	Out_SubSurfaces_DecalStencil = vec4(0.5);
}
";
		public const string MAIN_VERT_SHADER_CODE =
@"
void main()
{
	PassThrough();
    gl_Position = GetVertexPos();
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
    PassThrough();
    gl_Position = GetVertexPos();
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
			shaderUniforms = rhuRawShaderData.shaderUniforms;
			var mainResourceLayoutElementDescriptions = new List<ResourceLayoutElementDescription> {
				new ResourceLayoutElementDescription("",ResourceKind.UniformBuffer,ShaderStages.Vertex)
			};
			var shadowResourceLayoutElementDescriptions = new List<ResourceLayoutElementDescription>();
			foreach (var item in shaderUniforms) {
				var shadowPass = ShaderStages.None;
				var mainPass = ShaderStages.None;
				if (rhuRawShaderData.ShadowFragShaderCode?.Contains(item.FieldName)??false) {
					shadowPass |= ShaderStages.Fragment;
				}
				if (rhuRawShaderData.ShadowVertShaderCode?.Contains(item.FieldName)??false) {
					shadowPass |= ShaderStages.Vertex;
				}
				if (rhuRawShaderData.MainFragShaderCode?.Contains(item.FieldName) ?? false) {
					mainPass |= ShaderStages.Fragment;
				}
				if (rhuRawShaderData.MainVertShaderCode?.Contains(item.FieldName) ?? false) {
					mainPass |= ShaderStages.Vertex;
				}
				if (shadowPass == ShaderStages.None && mainPass == ShaderStages.None) {
					Console.WriteLine($"Uniform {item.FieldName} Name:{item.Name} Type:{item.Type} Is never used in shaderCode");
					return;
				}
				if (item.Type is UniformType.Texture1D or UniformType.Texture2D or UniformType.Texture3D) {
					if (shadowPass != ShaderStages.None) {
						shadowResourceLayoutElementDescriptions.Add(new ResourceLayoutElementDescription(item.FieldName, ResourceKind.TextureReadOnly, shadowPass));
						shadowResourceLayoutElementDescriptions.Add(new ResourceLayoutElementDescription(item.FieldName + "_Sampler", ResourceKind.TextureReadOnly, shadowPass));
					}
					if (mainPass != ShaderStages.None) {
						mainResourceLayoutElementDescriptions.Add(new ResourceLayoutElementDescription(item.FieldName, ResourceKind.TextureReadOnly, mainPass));
						mainResourceLayoutElementDescriptions.Add(new ResourceLayoutElementDescription(item.FieldName + "_Sampler", ResourceKind.TextureReadOnly, mainPass));
					}
				}
				else {
					if (shadowPass != ShaderStages.None) {
						shadowResourceLayoutElementDescriptions.Add(new ResourceLayoutElementDescription(item.FieldName, ResourceKind.UniformBuffer, shadowPass));
					}
					if (mainPass != ShaderStages.None) {
						mainResourceLayoutElementDescriptions.Add(new ResourceLayoutElementDescription(item.FieldName, ResourceKind.UniformBuffer, mainPass));
					}
				}
			}
			MainResourceLayout = Renderer.MainGraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(mainResourceLayoutElementDescriptions.ToArray()));
			ShadowResourceLayout = Renderer.MainGraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(shadowResourceLayoutElementDescriptions.ToArray()));
			DefferedKey = rhuRawShaderData.DeferedKey;
			LoadedShader = true;
		}
		public ResourceLayout ShadowResourceLayout { get; private set; }

		public ResourceLayout MainResourceLayout { get; private set; }

		public ShaderUniform[] shaderUniforms;

		public Renderer Renderer { get; }
		public string DefferedKey { get; private set; }

		public void Dispose() {
			LoadedShader = false;
			if (MainShaders is not null) {
				foreach (var item in MainShaders) {
					item.Dispose();
				}
			}
			MainShaders = null;
			MainResourceLayout?.Dispose();
			MainResourceLayout = null;
			if (ShadowShaders is not null) {
				foreach (var item in ShadowShaders) {
					item.Dispose();
				}
			}
			ShadowShaders = null;
			ShadowResourceLayout?.Dispose();
			ShadowResourceLayout = null;
		}
	}
}
