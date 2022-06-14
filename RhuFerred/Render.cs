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
		public SafeHashSet<Camera> Cameras = new();

		public ILogger Logger { get; private set; }
		public Renderer(ILogger logger = null) {
			Logger = logger ?? new BasicLogger();
		}

		public GraphicsDevice GraphicsDevice { get; private set; }

		public Sdl2Window MainWindow { get; private set; }

		public bool IsRunning = true;

		private readonly Stopwatch _stopwatch = new();

		public double DeltaTime { get; private set; }

		public bool Step(Action value = null) {
			DeltaTime = _stopwatch.Elapsed.TotalMilliseconds;
			_stopwatch.Restart();
			MainWindow.PumpEvents();
			if (!MainWindow.Exists) {
				Exit();
			}
			Cameras.ForEach((cam) => cam.Render());
			value?.Invoke();
			return IsRunning;
		}

		public void Exit() {
			IsRunning = false;
		}

		public static Sdl2Window CreateBasicWindow(string windowName = "RhuFerred Window") {
			var windowCI = new WindowCreateInfo() {
				WindowHeight = 540,
				WindowWidth = 960,
				X = 100,
				Y = 100,
				WindowTitle = windowName
			};
			return VeldridStartup.CreateWindow(ref windowCI);
		}

		public void Init(Sdl2Window window, GraphicsBackend? graphicsBackend = null) {
			var options = new GraphicsDeviceOptions {
				PreferStandardClipSpaceYDirection = true,
				PreferDepthRangeZeroToOne = true
			};
			MainWindow = window;
			GraphicsDevice = graphicsBackend is null
				? VeldridStartup.CreateGraphicsDevice(window, options)
				: VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend ?? GraphicsBackend.Vulkan);
			Init();
		}

		public void Init(SwapchainDescription swapchainDescription, GraphicsBackend? graphicsBackend = null) {
			var options = new GraphicsDeviceOptions {
				PreferStandardClipSpaceYDirection = true,
				PreferDepthRangeZeroToOne = true
			};
			var backend = graphicsBackend ?? GraphicsBackend.Vulkan;
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
			GraphicsDevice = backend switch {
				GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(options, swapchainDescription),
				GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(options, swapchainDescription),
				GraphicsBackend.OpenGL => throw new Exception("Not possible"),
				GraphicsBackend.Metal => GraphicsDevice.CreateMetal(options, swapchainDescription),
				GraphicsBackend.OpenGLES => GraphicsDevice.CreateOpenGLES(options, swapchainDescription),
				_ => throw new Exception("Not possible"),
			};
			Init();
		}

		private void Init() {
			Logger.Info($"DeviceName:{GraphicsDevice.DeviceName} Backend:{GraphicsDevice.BackendType} ApiVersion:{GraphicsDevice.ApiVersion}");
		}

		public void Dispose() {
			foreach (var item in Cameras.values) {
				item.Dispose();
			}
			GraphicsDevice?.Dispose();
			GraphicsDevice = null;
		}
	}
}
