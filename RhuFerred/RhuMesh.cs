using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Veldrid;

using Vortice.Mathematics;

namespace RhuFerred
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexInfo
	{
		public Vector3 Position;
		public Vector4 Color;
		public Vector3 Tangent;
		public Vector3 Normal;
		public Vector2 UV1;
		public Vector2 UV2;
		public Vector2 UV3;
		public Vector2 UV4;
		public Int4 Bones;

		public VertexInfo(Vector3 pos) {
			Position = pos;
			Color = RgbaFloat.White.ToVector4();
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
			Color = color.ToVector4();
			Tangent = Vector3.UnitY;
			Normal = Vector3.UnitZ;
			UV1 = Vector2.Zero;
			UV2 = Vector2.Zero;
			UV3 = Vector2.Zero;
			UV4 = Vector2.Zero;
			Bones = Int4.Zero;
		}
	}

	public class RhuMesh : IDisposable
	{
		public Renderer Renderer { get; }

		public RhuMesh(Renderer renderer) {
			Renderer = renderer;
		}
		public DeviceBuffer VertBuffer { get; private set; }
		public DeviceBuffer IndexBuffer { get; private set; }
		public uint[] Indexes = new uint[0];
		public VertexInfo[] Verts = new VertexInfo[0];

		public int VertexCount => Verts.Length;

		public VertexInfo GetVertexInfo(int vertex) {
			return Verts[vertex];
		}

		public void LoadMainMesh(uint[] indexes,VertexInfo[] vertexInfos) {
			Verts = vertexInfos;
			Indexes = indexes;
			BuildBuffers();
		}
		public void UpdateBuffers() {
			Renderer.Logger.Info("Update Mesh Buffers");
			Renderer.MainGraphicsDevice.UpdateBuffer(IndexBuffer, 0, Indexes);
			Renderer.MainGraphicsDevice.UpdateBuffer(VertBuffer, 0, Verts);
		}

		public unsafe void BuildBuffers() {
			Renderer.Logger.Info("Build Mesh Buffers");
			IndexBuffer?.Dispose();
			VertBuffer?.Dispose();
			IndexBuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(sizeof(uint) * (uint)Indexes.Length, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
			VertBuffer = Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(VertexInfo) * (uint)Verts.Length, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
			UpdateBuffers();
		}

		public void UpdateBoundingBox() {
			BoundingBox = BoundingBox.Empty;
			var poses = new Vector3[Verts.Length];
			for (var i = 0; i < Verts.Length; i++) {
				poses[i] = Verts[i].Position;
			}
			BoundingBox = BoundingBox.CreateMerged(BoundingBox.CreateFromPoints(poses), BoundingBox);
		}
		public BoundingBox BoundingBox { get; private set; }

		public void Dispose() {
			IndexBuffer?.Dispose();
			IndexBuffer = null;
			VertBuffer?.Dispose();
			VertBuffer = null;
		}
	}
}
