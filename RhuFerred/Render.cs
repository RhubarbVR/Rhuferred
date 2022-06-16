using System;

using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using System.Diagnostics;
using System.Collections.Generic;
using Veldrid.ImageSharp;
using System.Numerics;

namespace RhuFerred
{
	public class Renderer : IDisposable
	{
		private readonly bool _debuging;

		public RhuWindow FirstWindow => Windows.Count == 0 ? null : Windows[0];

		public GraphicsDeviceOptions GraphicsDeviceOptions => new(_debuging, null, false, ResourceBindingModel.Improved, true, true, true);

		public GraphicsBackend? PreferredGraphicsBackend { get; private set; }
		public readonly SafeHashSet<Camera> Cameras = new();
		public readonly SafeHashSet<RenderedMesh> RenderedMeshes = new();
		public readonly SafeHashSet<Light> Lights = new();

		public ILogger Logger { get; private set; }
		public Renderer(bool debug, GraphicsBackend? preferredGraphicsBackend = null, ILogger logger = null) {
			_debuging = debug;
			PreferredGraphicsBackend = preferredGraphicsBackend;
			Logger = logger ?? new BasicLogger();
		}

		public GraphicsDevice MainGraphicsDevice { get; internal set; }

		public readonly List<RhuWindow> Windows = new();

		public RhuWindow CreateNewWindow(string windowName = "RhuFerred Window", int WindowHeight = 540, int WindowWidth = 960, int x = 100, int y = 100) {
			var window = new RhuWindow(this);
			window.InitWindow(windowName, WindowHeight, WindowWidth, x, y);
			Windows.Add(window);
			return window;
		}

		public Camera CreateCamera(uint width = 960, uint height = 540) {
			return new Camera(this, width, height);
		}

		public RenderedMesh AttachMeshRender(RhuMesh rhuMesh, params RhuMaterial[] mits) {
			var rMesh = new RenderedMesh(this);
			rMesh.UpdateMesh(rhuMesh);
			rMesh.UpdateMaterials(mits);
			return rMesh;
		}

		public bool IsRunning = true;

		private readonly Stopwatch _stopwatch = new();

		public double DeltaTime { get; private set; }

		public double FPS => 1000 / DeltaTime;

		public bool Step(Action value = null) {
			DeltaTime = _stopwatch.Elapsed.TotalMilliseconds;
			_stopwatch.Restart();
			for (var i = Windows.Count - 1; i >= 0; i--) {
				var item = Windows[i];
				if (!item.Sdl2Window.Exists) {
					Windows.Remove(item);
					item.Dispose();
				}
			}
			foreach (var item in Windows) {
				item.UpdateInput();
			}
			if (Windows.Count <= 0) {
				Exit();
			}
			value?.Invoke();
			Cameras.ForEach((cam) => cam.Render());
			foreach (var item in Windows) {
				item.Update();
			}
			return IsRunning;
		}

		public void Exit() {
			IsRunning = false;
		}

		public void Init(RhuWindow window) {
			MainGraphicsDevice = window.GraphicsDevice;
			Init();
		}

		public void InitForMobile(SwapchainDescription swapchainDescription) {
			var backend = PreferredGraphicsBackend ?? GraphicsBackend.Vulkan;
			if (GraphicsDevice.IsBackendSupported(backend)) {
			}
			else {
				backend = GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)
					? GraphicsBackend.Vulkan
					: GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11)
									? GraphicsBackend.Direct3D11
									: GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal)
													? GraphicsBackend.Metal
													: GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES)
																	? GraphicsBackend.OpenGLES
																	: GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL)
																					? GraphicsBackend.OpenGL
																					: throw new Exception("No compatible backend found");
			}
			MainGraphicsDevice = backend switch {
				GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(GraphicsDeviceOptions, swapchainDescription),
				GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(GraphicsDeviceOptions, swapchainDescription),
				GraphicsBackend.OpenGL => throw new Exception("Not possible"),
				GraphicsBackend.Metal => GraphicsDevice.CreateMetal(GraphicsDeviceOptions, swapchainDescription),
				GraphicsBackend.OpenGLES => GraphicsDevice.CreateOpenGLES(GraphicsDeviceOptions, swapchainDescription),
				_ => throw new Exception("Not possible"),
			};
			Init();
		}

		public RhuShader BankShader { get; private set; }
		public RhuShader MainShader { get; private set; }
		public Texture NullTexture { get; private set; }

		public RhuShader LoadShader(string RhubarbShaderCode) {
			return LoadShader(RhuShaderParser.ParseShaderCode(RhubarbShaderCode));
		}

		public RhuShader LoadShader(RhuRawShaderData RhubarbShaderCode) {
			return new RhuShader(this, RhubarbShaderCode);
		}
		public RhuMaterial NewMaterial(RhuShader shader) {
			return new RhuMaterial(this, shader);
		}

		public RhuMesh NewMesh(uint[] indexes, VertexInfo[] vertexInfos) {
			var vw = new RhuMesh(this);
			vw.LoadMainMesh(indexes, vertexInfos);
			return vw;
		}

		public RhuMesh LoadCube(float halfSize = 0.5f) {
			var verts = new VertexInfo[] {
				        //Top
				new VertexInfo(new Vector3(-halfSize,halfSize,-halfSize)),
				new VertexInfo(new Vector3(halfSize,halfSize,-halfSize)),
				new VertexInfo(new Vector3(-halfSize,halfSize,halfSize)),
				new VertexInfo(new Vector3(halfSize,halfSize,halfSize)),
				        //Bottom
				new VertexInfo(new Vector3(-halfSize,-halfSize,-halfSize)),
				new VertexInfo(new Vector3(halfSize,-halfSize,-halfSize)),
				new VertexInfo(new Vector3(-halfSize,-halfSize,halfSize)),
				new VertexInfo(new Vector3(halfSize,-halfSize,halfSize)),
				        //Front
				new VertexInfo(new Vector3(-halfSize,halfSize,halfSize)),
				new VertexInfo(new Vector3(halfSize,halfSize,halfSize)),
				new VertexInfo(new Vector3(-halfSize,-halfSize,-halfSize)),
				new VertexInfo(new Vector3(halfSize,-halfSize,-halfSize)),
				        //Back
				new VertexInfo(new Vector3(-halfSize,halfSize,-halfSize)),
				new VertexInfo(new Vector3(halfSize,halfSize,-halfSize)),
				new VertexInfo(new Vector3(-halfSize,halfSize,halfSize)),
				new VertexInfo(new Vector3(halfSize,halfSize,halfSize)),
				        //Left
				new VertexInfo(new Vector3(-halfSize,halfSize,halfSize)),
				new VertexInfo(new Vector3(-halfSize,halfSize,-halfSize)),
				new VertexInfo(new Vector3(-halfSize,-halfSize,-halfSize)),
				new VertexInfo(new Vector3(halfSize,-halfSize,-halfSize)),
				        //Right
				new VertexInfo(new Vector3(halfSize,halfSize,halfSize)),
				new VertexInfo(new Vector3(halfSize,halfSize,-halfSize)),
				new VertexInfo(new Vector3(halfSize,-halfSize,halfSize)),
				new VertexInfo(new Vector3(halfSize,-halfSize,-halfSize)),
			};
			var indes = new uint[] {
				//Top
				0, 1, 2,
				2, 3, 1,

				//Bottom
				4, 5, 6,
				6, 7, 5,

				//Front
				8, 9, 10,
				10, 11, 9,

				//Back
				12, 13, 14,
				14, 15, 13,

				//Left
				16, 17, 18,
				18, 19, 17,

				//Right
				20, 21, 22,
				22, 23, 21
			};
			return NewMesh(indes, verts);
		}
		private void Init() {
			Logger.Info($"DeviceName:{MainGraphicsDevice.DeviceName} Backend:{MainGraphicsDevice.BackendType} ApiVersion:{MainGraphicsDevice.ApiVersion}");
			BankShader = LoadShader(ShaderCode.BLANKSHADER);
			MainShader = LoadShader(ShaderCode.MAINSHADER);
			var tmemp = new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, RgbaFloat.Pink), false);
			NullTexture = tmemp.CreateDeviceTexture(MainGraphicsDevice, MainGraphicsDevice.ResourceFactory);
			tmemp.Images[0].Dispose();
		}

		public void Dispose() {
			foreach (var item in Cameras.values) {
				item.Dispose();
			}
			foreach (var item in Windows) {
				item.Dispose();
			}
			MainGraphicsDevice = null;
		}
	}
}
