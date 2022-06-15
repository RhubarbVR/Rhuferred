using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

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

		public RenderedMesh(Renderer renderer) {
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
	}
}
