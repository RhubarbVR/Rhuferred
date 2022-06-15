using System;

using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using System.Diagnostics;
using System.Collections.Generic;

namespace RhuFerred
{
	public class Renderer : IDisposable
	{
		public RhuWindow FirstWindow => Windows.Count == 0 ? null : Windows[0];

		public GraphicsDeviceOptions GraphicsDeviceOptions => new(false, null, false, ResourceBindingModel.Improved, true, true, true);

		public GraphicsBackend? PreferredGraphicsBackend { get; private set; }
		public readonly SafeHashSet<Camera> Cameras = new();
		public readonly SafeHashSet<RenderedMesh> RenderedMeshes = new();
		public readonly SafeHashSet<Light> Lights = new();
		
		public ILogger Logger { get; private set; }
		public Renderer(GraphicsBackend? preferredGraphicsBackend = null, ILogger logger = null) {
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

		public Camera CreateCamera(uint width = 960,uint height = 540) {
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

		public RhuShader LoadShader(string RhubarbShaderCode) {
			return LoadShader(RhuShaderParser.ParseShaderCode(RhubarbShaderCode));
		}

		public RhuShader LoadShader(RhuRawShaderData RhubarbShaderCode) {
			return new RhuShader(this, RhubarbShaderCode);
		}
		public RhuMaterial NewMaterial(RhuShader shader) {
			return new RhuMaterial(this, shader);
		}
		private void Init() {
			Logger.Info($"DeviceName:{MainGraphicsDevice.DeviceName} Backend:{MainGraphicsDevice.BackendType} ApiVersion:{MainGraphicsDevice.ApiVersion}");
			BankShader = LoadShader(ShaderCode.BLANKSHADER);
			MainShader = LoadShader(ShaderCode.MAINSHADER);
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
