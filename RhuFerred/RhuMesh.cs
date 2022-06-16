using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Veldrid;

using Vortice.Mathematics;

namespace RhuFerred
{
	public struct VertexInfo
	{
		public Vector3 Position;
		public RgbaFloat Color;
		public Vector3 Tangent;
		public Vector3 Normal;
		public Vector2 UV1;
		public Vector2 UV2;
		public Vector2 UV3;
		public Vector2 UV4;
		public Int4 Bones;

		public VertexInfo(Vector3 pos) {
			Position = pos;
			Color = RgbaFloat.White;
			Tangent = Vector3.UnitY;
			Normal = Vector3.UnitZ;
			UV1 = Vector2.Zero;
			UV2 = Vector2.Zero;
			UV3 = Vector2.Zero;
			UV4 = Vector2.Zero;
			Bones = Int4.Zero;
		}
		public VertexInfo(Vector3 pos, RgbaFloat color) {
			Position = pos;
			Color = color;
			Tangent = Vector3.UnitY;
			Normal = Vector3.UnitZ;
			UV1 = Vector2.Zero;
			UV2 = Vector2.Zero;
			UV3 = Vector2.Zero;
			UV4 = Vector2.Zero;
			Bones = Int4.Zero;
		}
	}

	public class MainMeshData: IDisposable
	{
		public DeviceBuffer PositionBuffer { get; private set; }
		public Vector3[] Position;
		public DeviceBuffer ColorBuffer { get; private set; }
		public RgbaFloat[] Color;
		public DeviceBuffer TangentBuffer { get; private set; }
		public Vector3[] Tangent;
		public DeviceBuffer NormalBuffer { get; private set; }
		public Vector3[] Normal;
		public DeviceBuffer UV1Buffer { get; private set; }
		public Vector2[] UV1;
		public DeviceBuffer UV2Buffer { get; private set; }
		public Vector2[] UV2;
		public DeviceBuffer UV3Buffer { get; private set; }
		public Vector2[] UV3;
		public DeviceBuffer UV4Buffer { get; private set; }
		public Vector2[] UV4;


		public MainMeshData(VertexInfo[] vertexInfos) {
			var length = vertexInfos.Length;
			Position = new Vector3[length];
			Color = new RgbaFloat[length];
			Tangent = new Vector3[length];
			Normal = new Vector3[length];
			UV1 = new Vector2[length];
			UV2 = new Vector2[length];
			UV3 = new Vector2[length];
			UV4 = new Vector2[length];
			for (var i = 0; i < length; i++) {
				var v = vertexInfos[i];
				Position[i] = v.Position;
				Color[i] = v.Color;
				Tangent[i] = v.Tangent;
				Normal[i] = v.Normal;
				UV1[i] = v.UV1;
				UV2[i] = v.UV2;
				UV3[i] = v.UV3;
				UV4[i] = v.UV4;
			}
		}

		public MainMeshData(Vector3[] position, RgbaFloat[] color, Vector3[] tangent, Vector3[] normal, Vector2[] uV1, Vector2[] uV2, Vector2[] uV3, Vector2[] uV4) {
			Position = position;
			Color = color;
			Tangent = tangent;
			Normal = normal;
			UV1 = uV1;
			UV2 = uV2;
			UV3 = uV3;
			UV4 = uV4;
		}
		public void UpdateBuffers(GraphicsDevice gd) {
			gd.UpdateBuffer(PositionBuffer, 0, Position);
			gd.UpdateBuffer(ColorBuffer, 0, Color);
			gd.UpdateBuffer(TangentBuffer, 0, Tangent);
			gd.UpdateBuffer(NormalBuffer, 0, Normal);
			gd.UpdateBuffer(UV1Buffer, 0, UV1);
			gd.UpdateBuffer(UV2Buffer, 0, UV2);
			gd.UpdateBuffer(UV3Buffer, 0, UV3);
			gd.UpdateBuffer(UV4Buffer, 0, UV4);

		}
		public unsafe void BuildBuffers(ResourceFactory resourceFactory) {
			PositionBuffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 3 * (uint)Position.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			ColorBuffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 4 * (uint)Color.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			TangentBuffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 3 * (uint)Tangent.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			NormalBuffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 3 * (uint)Normal.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			UV1Buffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 2 * (uint)UV1.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			UV2Buffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 2 * (uint)UV2.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			UV3Buffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 2 * (uint)UV3.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			UV4Buffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(float) * 2 * (uint)UV4.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
		}

		public void Dispose() {
			PositionBuffer?.Dispose();
			PositionBuffer = null;
			ColorBuffer?.Dispose();
			ColorBuffer = null;
			TangentBuffer?.Dispose();
			TangentBuffer = null;
			NormalBuffer?.Dispose();
			NormalBuffer = null;
			UV1Buffer?.Dispose();
			UV1Buffer = null;
			UV2Buffer?.Dispose();
			UV2Buffer = null;
			UV3Buffer?.Dispose();
			UV3Buffer = null;
			UV4Buffer?.Dispose();
			UV4Buffer = null;
		}

		public void Resize(int newSize) {
			Array.Resize(ref Position, newSize);
			Array.Resize(ref Color, newSize);
			Array.Resize(ref Tangent, newSize);
			Array.Resize(ref Normal, newSize);
			Array.Resize(ref UV1, newSize);
			Array.Resize(ref UV2, newSize);
			Array.Resize(ref UV3, newSize);
			Array.Resize(ref UV4, newSize);
		}
	}

	public class RhuMesh : IDisposable
	{
		public Renderer Renderer { get; }

		public RhuMesh(Renderer renderer) {
			Renderer = renderer;
		}
		public DeviceBuffer IndexBuffer { get; private set; }
		public uint[] Indexes = new uint[0];
		public MainMeshData[] mainMeshDatas = new MainMeshData[0];

		public DeviceBuffer BonesBuffer { get; private set; }
		public Int4[] Bones = new Int4[0];

		public MainMeshData MainMesh => mainMeshDatas[0];

		public int VertexCount => Bones.Length;

		public VertexInfo GetVertexInfo(int vertex,int mesh = 0) {
			return new VertexInfo {
				Position = mainMeshDatas[mesh].Position[vertex],
				Color = mainMeshDatas[mesh].Color[vertex],
				Tangent = mainMeshDatas[mesh].Tangent[vertex],
				Normal = mainMeshDatas[mesh].Normal[vertex],
				UV1 = mainMeshDatas[mesh].UV1[vertex],
				UV2 = mainMeshDatas[mesh].UV2[vertex],
				UV3 = mainMeshDatas[mesh].UV3[vertex],
				UV4 = mainMeshDatas[mesh].UV4[vertex],
				Bones = Bones[vertex]
			};
		}

		public void LoadMainMesh(uint[] indexes,VertexInfo[] vertexInfos) {
			mainMeshDatas = new MainMeshData[] {
				new MainMeshData(vertexInfos)
			};
			Bones = new Int4[vertexInfos.Length];
			for (var i = 0; i < vertexInfos.Length; i++) {
				Bones[i] = vertexInfos[i].Bones;
			}
			Indexes = indexes;
			BuildBuffers();
		}
		public void UpdateBuffers() {
			Renderer.Logger.Info("Update Mesh Buffers");
			Renderer.MainGraphicsDevice.UpdateBuffer(IndexBuffer, 0, Indexes);
			Renderer.MainGraphicsDevice.UpdateBuffer(BonesBuffer, 0, Bones);
			foreach (var item in mainMeshDatas) {
				item.UpdateBuffers(Renderer.MainGraphicsDevice);
			}
		}

		public unsafe void BuildBuffers() {
			Renderer.Logger.Info("Build Mesh Buffers");
			foreach (var item in mainMeshDatas) {
				item.BuildBuffers(Renderer.MainGraphicsDevice.ResourceFactory);
			}
			IndexBuffer?.Dispose();
			BonesBuffer?.Dispose();
			IndexBuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(sizeof(uint) * (uint)Indexes.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			BonesBuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(sizeof(int) * 4 * (uint)Bones.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			UpdateBuffers();
		}

		public void UpdateBoundingBox() {
			BoundingBox = BoundingBox.Empty;
			for (var i = 0; i < mainMeshDatas.Length; i++) {
				BoundingBox = BoundingBox.CreateMerged(BoundingBox.CreateFromPoints(mainMeshDatas[i].Position), BoundingBox);
			}
		}
		public BoundingBox BoundingBox { get; private set; }

		public void Dispose() {
			IndexBuffer?.Dispose();
			IndexBuffer = null;
			BonesBuffer?.Dispose();
			BonesBuffer = null;
			foreach (var item in mainMeshDatas) {
				item.Dispose();
			}
			mainMeshDatas = Array.Empty<MainMeshData>();
		}
	}
}
