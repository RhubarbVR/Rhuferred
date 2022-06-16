using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Veldrid;
using Veldrid.Utilities;

namespace RhuFerred
{
	public class RenderedMesh : IDisposable
	{

		public RhuMesh Mesh { get; private set; }

		public void UpdateMesh(RhuMesh rhuMesh) {
			Mesh = rhuMesh;
		}

		public Matrix4x4 WorldPos { get; set; }


		private void MitUpdate() {

		}
		public BoundingBox BoundingBox { get; private set; }

		public unsafe RenderedMesh(Renderer renderer) {
			Renderer = renderer;
			renderer.RenderedMeshes.Add(this);
		}

		public Renderer Renderer { get; }

		public void Dispose() {
			if (_enabled) {
				Renderer.RenderedMeshes.Remove(this);
			}
		}

		private bool _enabled = true;

		public bool Enabled
		{
			get => _enabled;
			set {
				if (value != _enabled) {
					if (value) {
						Renderer.RenderedMeshes.Add(this);
					}
					else {
						Renderer.RenderedMeshes.Remove(this);
					}
				}
				_enabled = value;
			}
		}


		private readonly List<RhuMaterial> _rhuMaterials = new();

		public RhuMaterial GetMaterial(int index) {
			return _rhuMaterials[index];
		}

		public void AddMaterial(params RhuMaterial[] mits) {
			AddMaterials(mits);
		}
		public void AddMaterials(IEnumerable<RhuMaterial> mits) {
			foreach (var m in mits) {
				_rhuMaterials.Add(m);
			}
			MitUpdate();
		}

		public void UpdateMaterials(IEnumerable<RhuMaterial> mits) {
			_rhuMaterials.Clear();
			AddMaterials(mits);
		}


		public void Render(CommandList commandList,Camera camera) {
			if (Mesh is null) {
				return;
			}
			for (var i = 0; i < _rhuMaterials.Count; i++) {
				var item = _rhuMaterials[i];
				if (item.MitLoaded) {
					commandList.SetPipeline(item.MainPipeline);
					commandList.SetVertexBuffer(0, Mesh.MainMesh.PositionBuffer);
					commandList.SetVertexBuffer(1, Mesh.MainMesh.ColorBuffer);
					commandList.SetVertexBuffer(2, Mesh.MainMesh.TangentBuffer);
					commandList.SetVertexBuffer(3, Mesh.MainMesh.UV1Buffer);
					commandList.SetVertexBuffer(4, Mesh.MainMesh.UV2Buffer);
					commandList.SetVertexBuffer(5, Mesh.MainMesh.UV3Buffer);
					commandList.SetVertexBuffer(6, Mesh.MainMesh.UV4Buffer);
					commandList.SetVertexBuffer(7, Mesh.BonesBuffer);
					commandList.SetIndexBuffer(Mesh.IndexBuffer, IndexFormat.UInt32);
					item.UpdateUbo(commandList, camera, WorldPos, (uint)i);
					commandList.SetComputeResourceSet(0, item.MainResourceSet);
					commandList.DrawIndexed((uint)Mesh.Indexes.Length);
				}
			}
		}
	}
}
