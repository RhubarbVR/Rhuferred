using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace RhuFerred
{
	//todo fix bug where secondary windows crash everything when resized
	public class RhuWindow : IDisposable
	{
		private DeviceBuffer _vertexBuffer;
		private DeviceBuffer _indexBuffer;
		private Shader[] _shaders;
		private Pipeline _pipeline;
		private ResourceLayout _rl;
		private ResourceSet _rs;
		/// <summary>
		/// Should use <see cref="Destroy"/> this is only for internal use
		/// </summary>
		public void Dispose() {
			Sdl2Window?.Close();
			Sdl2Window = null;
			_commandList?.Dispose();
			_commandList = null;
			_vertexBuffer?.Dispose();
			_vertexBuffer = null;
			_indexBuffer?.Dispose();
			_indexBuffer = null;
			_pipeline?.Dispose();
			_pipeline = null;
			_rl?.Dispose();
			_rl = null;
			_rs?.Dispose();
			_rs = null;
			if((_shaders?.Length??0) > 0) {
				foreach (var item in _shaders) {
					item?.Dispose();
				}
				_shaders = Array.Empty<Shader>();
			}
		}

		private const string VERTEX_CODE = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 0) out vec2 Position_out;

void main()
{
	Position_out = Position;
    gl_Position = vec4(Position, 0, 1);
}";

		private const string FRAGMENT_CODE = @"
#version 450

layout(location = 0) out vec4 fsout_Color;
layout(set = 0, binding = 0) uniform texture2D SourceTexture;
layout(set = 0, binding = 1) uniform sampler SourceSampler;
layout(location = 0) in vec2 Position_out;

layout(constant_id = 103) const bool OutputFormatSrgb = true;

vec3 LinearToSrgb(vec3 linear)
{
    // http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
    vec3 S1 = sqrt(linear);
    vec3 S2 = sqrt(S1);
    vec3 S3 = sqrt(S2);
    vec3 sRGB = 0.662002687 * S1 + 0.684122060 * S2 - 0.323583601 * S3 - 0.0225411470 * linear;
    return sRGB;
}

void main()
{
    vec4 color = texture(sampler2D(SourceTexture, SourceSampler), (Position_out + 1)/2);

    if (!OutputFormatSrgb)
    {
        color = vec4(LinearToSrgb(color.rgb), 1);
    }

    fsout_Color = color;
}";

		private CommandList _commandList;
		public Texture Texture { get; private set; }

		public void UpdateTexture(Texture newTexture) {
			Texture = newTexture;
			_rs?.Dispose();
			if (newTexture is null) {
				_rs = null;
				return;
			}
			_rs = GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_rl, newTexture, GraphicsDevice.Aniso4xSampler));
		}

		public Sdl2Window Sdl2Window { get; private set; }
		public Renderer Renderer { get; }
		public GraphicsDevice GraphicsDevice { get; private set; }

		public RhuWindow(Renderer renderer) {
			Renderer = renderer;
		}

		public void Destroy() {
			Sdl2Window?.Close();
		}

		
		//Width,Height
		public event Action<uint, uint> Resize;

		public void TargetCamera(Camera camera) {
			ResizeCameraWithWindow(camera);
			UpdateTexture(camera.MainTexture);
			camera.FinalTextureChange += UpdateTexture;
			camera.Resize((uint)Sdl2Window.Width, (uint)Sdl2Window.Height);
		}

		public void RemoveTargetCamera(Camera camera) {
			RemoveResizeCameraWithWindow(camera);
			UpdateTexture(null);
			camera.FinalTextureChange -= UpdateTexture;
		}

		public void ResizeCameraWithWindow(Camera cam) {
			Resize += cam.Resize;
		}

		public void RemoveResizeCameraWithWindow(Camera cam) {
			Resize -= cam.Resize;
		}

		internal void InitWindow(string windowName = "RhuFerred Window", int WindowHeight = 540, int WindowWidth = 960, int x = 100, int y = 100) {
			var windowCI = new WindowCreateInfo() {
				WindowHeight = WindowHeight,
				WindowWidth = WindowWidth,
				X = x,
				Y = y,
				WindowTitle = windowName
			};
			Sdl2Window = VeldridStartup.CreateWindow(ref windowCI);
			GraphicsDevice = Renderer.PreferredGraphicsBackend is null
				? VeldridStartup.CreateGraphicsDevice(Sdl2Window, Renderer.GraphicsDeviceOptions)
				: VeldridStartup.CreateGraphicsDevice(Sdl2Window, Renderer.GraphicsDeviceOptions, Renderer.PreferredGraphicsBackend ?? GraphicsBackend.Vulkan);

			Sdl2Window.Resized += () => WindowWasResized = true;
			Renderer.MainGraphicsDevice ??= GraphicsDevice;
			Vector2[] quadVertices =
									{
										new Vector2(-1f, 1f),
										new Vector2(1f),
										new Vector2(-1f, -1f),
										new Vector2(1f, -1f)
									};
			ushort[] quadIndices = { 0, 1, 2, 3 };
			_vertexBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(4 * 8, BufferUsage.VertexBuffer));
			_indexBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));
			GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);
			GraphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);
			var vertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
			);
			var vertexShaderDesc = new ShaderDescription(
	ShaderStages.Vertex,
	Encoding.UTF8.GetBytes(VERTEX_CODE),
	"main");
			var fragmentShaderDesc = new ShaderDescription(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(FRAGMENT_CODE),
				"main");
			_shaders = GraphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

			_rl = GraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
			var pipelineDescription = new GraphicsPipelineDescription {
				BlendState = BlendStateDescription.SingleOverrideBlend,

				DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual),

				RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Back,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false),

				PrimitiveTopology = PrimitiveTopology.TriangleStrip,
				ResourceLayouts = new ResourceLayout[] { _rl },

				ShaderSet = new ShaderSetDescription(
				vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
				shaders: _shaders),

				Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription
			};
			_pipeline = GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
			_commandList = GraphicsDevice.ResourceFactory.CreateCommandList();
		}

		public InputSnapshot Snapshot { get; private set; }
		public bool WindowWasResized { get; private set; }

		public void UpdateInput() {
			Snapshot = Sdl2Window.PumpEvents();
			if (WindowWasResized) {
				Resize?.Invoke((uint)Sdl2Window.Width, (uint)Sdl2Window.Height);
				WindowWasResized = false;
			}
		}

		public void Update() {
			if (!Sdl2Window.Exists) {
				return;
			}
			_commandList.Begin();
			_commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
			if(!(Texture is null || _rs is null)) {
				if (Texture.IsDisposed) {
					return;
				}
				_commandList.SetVertexBuffer(0, _vertexBuffer);
				_commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
				_commandList.SetPipeline(_pipeline);
				_commandList.SetGraphicsResourceSet(0, _rs);
				_commandList.DrawIndexed(
					indexCount: 4,
					instanceCount: 1,
					indexStart: 0,
					vertexOffset: 0,
					instanceStart: 0);
			}
			_commandList.End();
			GraphicsDevice.SubmitCommands(_commandList);
			GraphicsDevice.SwapBuffers();
			GraphicsDevice.WaitForIdle();
		}
	}
}
