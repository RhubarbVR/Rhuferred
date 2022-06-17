using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using RhuFerred.MaterialUniforms;

using Veldrid;

namespace RhuFerred
{
	public class RhuMaterial : IDisposable
	{
		private RhuShader _rhuShader;

		public string OverRideDefferedKey = null;
		public string DefferedKey => OverRideDefferedKey ?? _rhuShader.DefferedKey;

		public RhuShader RhuShader
		{
			get => _rhuShader;
			set => UpdateShaderInfo(value);
		}

		private void UpdateShaderInfo(RhuShader rhuShader) {
			Renderer.Logger.Info($"Loaded Shader");
			MitLoaded = false;
			if (_rhuShader is not null) {
				Renderer.Logger.Info("Removing Last Shader");
			}
			Renderer.Logger.Info("Loading Shader Into material");
			if (rhuShader is null) {
				Renderer.Logger.Info("Shader Null");
				return;
			}
			if (!rhuShader.LoadedShader) {
				Renderer.Logger.Info("Shader not loaded");
				return;
			}
			_rhuShader = rhuShader;
			var newUniforms = 0;
			var oldUniforms = 0;
			var replacedUniforms = 0;
			Renderer.Logger.Info($"Uniforms Loading");

			for (var i = 0; i < rhuShader.shaderUniforms.Length; i++) {
				var uniform = rhuShader.shaderUniforms[i];
				if (_uniforms.TryGetValue(uniform.FieldName, out var value)) {
					if (value.UniType == uniform.Type) {
						value.SetDefaults(uniform.Defaults);
						oldUniforms++;
						continue;
					}
					else {
						_uniforms.Remove(uniform.FieldName);
						replacedUniforms++;
						newUniforms--;
						value.Dispose();
					}
				}
				newUniforms++;
				var newData = ShaderUniformHelper.GetShaderUniform(uniform.Type);
				newData.BindToMaterial(this);
				newData.SetDefaults(uniform.Defaults);
				newData.CreateDeviceResource();
				_uniforms.Add(uniform.FieldName, newData);
			}
			Renderer.Logger.Info($"Loaded {newUniforms} New Uniforms, {oldUniforms} Old Uniforms and Replaced {replacedUniforms}");
			LoadPipeLines();
		}
		public static readonly ResourceLayoutDescription ProjViewWorldLayoutDescription = new(
			new ResourceLayoutElementDescription("WorldData", ResourceKind.UniformBuffer, ShaderStages.Vertex)
		);

		private static ResourceLayout _resourceLayout;

		public static ResourceLayout GetProjViewWorldLayout(ResourceFactory resourceFactory) {
			_resourceLayout ??= resourceFactory.CreateResourceLayout(ProjViewWorldLayoutDescription);
			return _resourceLayout;
		}

		public Pipeline MainPipeline { get; private set; }

		public Pipeline ShadowPipeline { get; private set; }

		private void LoadPipeLines() {
			MainPipeline?.Dispose();
			var vertexLayouts = new VertexLayoutDescription[]
			{
				new VertexLayoutDescription(
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
					new VertexElementDescription("Tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
					new VertexElementDescription("UV1", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
					new VertexElementDescription("UV2", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
					new VertexElementDescription("UV3", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
					new VertexElementDescription("UV4", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
					new VertexElementDescription("Bones", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
				),
			};
			var outputDesciption = new OutputDescription {
				ColorAttachments = new OutputAttachmentDescription[] {
						new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float),// Albdo:4
						new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float),// Specular:3 Metallic:1
						new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float),// Emission:3 Ambient Occlusion:1 
						new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float),// Normals:3  Roughness:1
						new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float),// SubSurfaces:3 DecalStencil : 1
						new OutputAttachmentDescription(PixelFormat.R32_G32_B32_A32_Float),// Position: 3 UserData: 1
					},
				DepthAttachment = new OutputAttachmentDescription {
					Format = PixelFormat.R32_Float
				},				
			};

			var resourceLayout = GetProjViewWorldLayout(Renderer.MainGraphicsDevice.ResourceFactory);
			var mainDescription = new GraphicsPipelineDescription(
				BlendStateDescription.Empty,
				DepthStencilStateDescription.Disabled,//Renderer.MainGraphicsDevice.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
				RasterizerStateDescription.CullNone,
				PrimitiveTopology.TriangleList,
				new ShaderSetDescription(vertexLayouts, RhuShader.MainShaders, new[] { new SpecializationConstant(100, Renderer.MainGraphicsDevice.IsClipSpaceYInverted) }),
				resourceLayout,
				outputDesciption
			);
			
			MainPipeline = Renderer.MainGraphicsDevice.ResourceFactory.CreateGraphicsPipeline(mainDescription);

			MainResourceSet = Renderer.MainGraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
				GetProjViewWorldLayout(Renderer.MainGraphicsDevice.ResourceFactory),_wvpBuffer));

			MitLoaded = true;
			Console.WriteLine("Loaded PipeLines");
		}
		private readonly Dictionary<string, MaterialUniform> _uniforms = new();

		public MaterialUniform GetUniform(string key) {
			return _uniforms[key];
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct WorldData
		{
			public Matrix4x4 Projection;
			public Matrix4x4 View;
			public Matrix4x4 World;
			public uint MaterialIndex;
		}
		public void UpdateUbo(CommandList _commandList, Camera camera, Matrix4x4 WorldPos, uint mitindex) {
			_commandList.UpdateBuffer(_wvpBuffer, 0, new WorldData {
				World = WorldPos,
				MaterialIndex = mitindex,
				View = camera.View,
				Projection = camera.Projection
			});
		}
		public object this[string key]
		{
			get => _uniforms[key].GetData();
			set => _uniforms[key].SetData(value);
		}

		private DeviceBuffer _wvpBuffer;

		public unsafe RhuMaterial(Renderer renderer) {
			Renderer = renderer;
			_wvpBuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1568, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}

		public unsafe RhuMaterial(Renderer renderer, RhuShader targetShader) {
			Renderer = renderer;
			_wvpBuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(1568, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
			RhuShader = targetShader;
		}

		public Renderer Renderer { get; }
		public ResourceSet MainResourceSet { get; private set; }
		public bool MitLoaded { get; private set; }

		public void Dispose() {
			foreach (var item in _uniforms) {
				item.Value.Dispose();
			}
			_wvpBuffer.Dispose();
			_wvpBuffer = null;
		}
	}
}
