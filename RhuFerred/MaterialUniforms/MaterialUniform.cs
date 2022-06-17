using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Veldrid;


namespace RhuFerred.MaterialUniforms
{
	public static class StringHelper
	{
		public static IEnumerable<string> GetStrings(this string str) {
			var section = "";
			foreach (var item in str) {
				if (item == ',') {
					yield return section;
					section = "";
				}
				else {
					if (item != '(') {
						section += item;
					}
				}
			}
		}
		public static T GetValue<T>(string value) {
			return (T)Convert.ChangeType(value, typeof(T));
		}
	}
	public static class ShaderUniformHelper {
		public static Type[] Uniforms = new Type[] {
			null,
			typeof(MaterialUniform_Texture1D),
			typeof(MaterialUniform_Texture2D),
			typeof(MaterialUniform_Texture3D),
			typeof(MaterialUniform_ColorR),
			typeof(MaterialUniform_ColorRG),
			typeof(MaterialUniform_ColorGB),
			typeof(MaterialUniform_ColorRGB),
			typeof(MaterialUniform_ColorRGBA),
			typeof(MaterialUniformGen1<float>),
			typeof(MaterialUniformGen2<float>),
			typeof(MaterialUniformGen3<float>),
			typeof(MaterialUniformGen4<float>),
			typeof(MaterialUniformGen1<double>),
			typeof(MaterialUniformGen2<double>),
			typeof(MaterialUniformGen3<double>),
			typeof(MaterialUniformGen4<double>),
			typeof(MaterialUniformGen1<Matrix2x2>),
			typeof(MaterialUniformGen1<Matrix2x3>),
			typeof(MaterialUniformGen1<Matrix2x4>),
			typeof(MaterialUniformGen1<Matrix3x2>),
			typeof(MaterialUniformGen1<Matrix3x3>),
			typeof(MaterialUniformGen1<Matrix3x4>),
			typeof(MaterialUniformGen1<Matrix4x2>),
			typeof(MaterialUniformGen1<Matrix4x3>),
			typeof(MaterialUniformGen1<Matrix4x4>),
			typeof(MaterialUniformGen1<int>),
			typeof(MaterialUniformGen2<int>),
			typeof(MaterialUniformGen3<int>),
			typeof(MaterialUniformGen4<int>),
			typeof(MaterialUniformGen1<uint>),
			typeof(MaterialUniformGen2<uint>),
			typeof(MaterialUniformGen3<uint>),
			typeof(MaterialUniformGen4<uint>),
			typeof(MaterialUniformGen1<short>),
			typeof(MaterialUniformGen2<short>),
			typeof(MaterialUniformGen3<short>),
			typeof(MaterialUniformGen4<short>),
			typeof(MaterialUniformGen1<ushort>),
			typeof(MaterialUniformGen2<ushort>),
			typeof(MaterialUniformGen3<ushort>),
			typeof(MaterialUniformGen4<ushort>),
			typeof(MaterialUniformGen1<bool>),
			typeof(MaterialUniformGen2<bool>),
			typeof(MaterialUniformGen3<bool>),
			typeof(MaterialUniformGen4<bool>),
		};

		public static MaterialUniform GetShaderUniform(UniformType uniformType) {
			var unitType = Uniforms[(int)uniformType];
			return (MaterialUniform)Activator.CreateInstance(unitType);
		}
	}

	public unsafe class MaterialUniformGen4<T> : MaterialUniform where T : unmanaged
	{
		public T Value1 { get; set; }
		public T Value2 { get; set; }
		public T Value3 { get; set; }
		public T Value4 { get; set; }
		public override int SizeInBytes => sizeof(T) * 4;

		public override object GetData() {
			return (Value1,Value2,Value3,Value4);
		}

		public override void SetData(object value) {
			(Value1, Value2, Value3, Value4) = (ValueTuple<T,T,T,T>)value;
			UpdateBuffer();
		}

		public override void SetDefaults(string defaults) {
			var data = StringHelper.GetStrings(defaults).GetEnumerator();
			data.MoveNext();
			Value1 = StringHelper.GetValue<T>(data.Current);
			data.MoveNext();
			Value2 = StringHelper.GetValue<T>(data.Current);
			data.MoveNext();
			Value3 = StringHelper.GetValue<T>(data.Current);
			data.MoveNext();
			Value4 = StringHelper.GetValue<T>(data.Current);
		}

		public override void UpdateBuffer() {
			var e = GCHandle.ToIntPtr(GCHandle.Alloc(Value1));
			GD.UpdateBuffer((DeviceBuffer)TargetResource, 0, e, (uint)SizeInBytes);
		}
	}


	public unsafe class MaterialUniformGen3<T> : MaterialUniform where T : unmanaged
	{
		public T Value1 { get; set; }
		public T Value2 { get; set; }
		public T Value3 { get; set; }
		public override int SizeInBytes => sizeof(T) * 3;

		public override object GetData() {
			return (Value1, Value2, Value3);
		}

		public override void SetData(object value) {
			(Value1, Value2, Value3) = (ValueTuple<T, T, T>)value;
			UpdateBuffer();
		}

		public override void SetDefaults(string defaults) {
			var data = StringHelper.GetStrings(defaults).GetEnumerator();
			data.MoveNext();
			Value1 = StringHelper.GetValue<T>(data.Current);
			data.MoveNext();
			Value2 = StringHelper.GetValue<T>(data.Current);
			data.MoveNext();
			Value3 = StringHelper.GetValue<T>(data.Current);
		}

		public override void UpdateBuffer() {
			var e = GCHandle.ToIntPtr(GCHandle.Alloc(Value1));
			GD.UpdateBuffer((DeviceBuffer)TargetResource, 0, e, (uint)SizeInBytes);
		}
	}


	public unsafe class MaterialUniformGen2<T> : MaterialUniform where T : unmanaged
	{
		public T Value1 { get; set; }
		public T Value2 { get; set; }
		public override int SizeInBytes => sizeof(T) * 2;

		public override object GetData() {
			return (Value1, Value2);
		}

		public override void SetData(object value) {
			(Value1, Value2) = (ValueTuple<T, T>)value;
			UpdateBuffer();
		}

		public override void SetDefaults(string defaults) {
			var data = StringHelper.GetStrings(defaults).GetEnumerator();
			data.MoveNext();
			Value1 = StringHelper.GetValue<T>(data.Current);
			data.MoveNext();
			Value2 = StringHelper.GetValue<T>(data.Current);
		}

		public override void UpdateBuffer() {
			var e = GCHandle.ToIntPtr(GCHandle.Alloc(Value1));
			GD.UpdateBuffer((DeviceBuffer)TargetResource, 0, e, (uint)SizeInBytes);
		}
	}

	public unsafe class MaterialUniformGen1<T>:MaterialUniform where T : unmanaged
	{
		public T Value { get; set; }

		public override int SizeInBytes => sizeof(T);

		public override object GetData() {
			return Value;
		}

		public override void SetData(object value) {
			Value = (T)value;
			UpdateBuffer();
		}

		public override void SetDefaults(string defaults) {
			Value = StringHelper.GetValue<T>(defaults.Substring(1,defaults.Length - 2));
		}


		public override void UpdateBuffer() {
			var e = GCHandle.ToIntPtr(GCHandle.Alloc(Value));
			GD.UpdateBuffer((DeviceBuffer)TargetResource, 0, e, (uint)SizeInBytes);
		}
	}
	public abstract class MaterialUniform : IDisposable
	{
		public GraphicsDevice GD => RhuMaterial.Renderer.MainGraphicsDevice;

		public BindableResource TargetResource;

		public virtual BindableResource Resource => TargetResource;

		public abstract void UpdateBuffer();

		public virtual void CreateDeviceResource() {
			if (TargetResource != null) {
				return;
			}
			TargetResource = RhuMaterial.Renderer.MainGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
			UpdateBuffer();
		}

		public RhuMaterial RhuMaterial { get; private set; }
		public void BindToMaterial(RhuMaterial rhuMaterial) {
			RhuMaterial = rhuMaterial;
		}

		public abstract int SizeInBytes { get; }
		public MaterialUniform() {
			var uniIndex = Array.IndexOf(ShaderUniformHelper.Uniforms, this);
			UniType = (UniformType)uniIndex;
		}
		public UniformType UniType { get; private set; }

		public abstract object GetData();

		public abstract void SetData(object value);

		public abstract void SetDefaults(string defaults);

		public virtual bool CanDispose => true;

		public void Dispose() {
			if (!CanDispose) {
				return;
			}
			if (TargetResource is null) {
				return;
			}
			try {
				((IDisposable)TargetResource).Dispose();
			}
			catch { 
			}
		}
	}
}
