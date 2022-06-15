using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhuFerred;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred.Tests
{
	[TestClass()]
	public class RhuShaderParserTests
	{
		public const string TEST_CODE =
@"
Shader   ""This is a test shader""          {
	Options{
		DeferredKey(""Standard"")
	}
	Uniforms {
		_Textue(""This Is Main Texture"",texture2d) = ""white"";
		_Color(""Color"",ColorRGB) = (1,1,1)
		_Shininess(""This Is Float Test"",Float) = 10;
	}
";
		public const string TEST_CODE_NOSPACES =
@"
Shader""This is a test shader""{
Uniforms{
		_Textue(""This Is Main Texture"",texture2d)=""white""
		_Color(""Color"",ColorRGB)=(1,1,1)
		_Shininess(""This Is Float Test"",Float)=10
	}
";

		public const string TEST_SHADERCODE =
@"
	MainFragShaderCode {The ParseData}
}
";
		public const string TEST_MULTISHADERCODE =
@"
	MainFragShaderCode
	{The Par{{}}seData}

	MainVertShaderCode {The{} ParseData 10}
}
";

		private void TestUniforms(RhuRawShaderData rhuRawShaderData) {
			Assert.AreEqual("Standard", rhuRawShaderData.DeferedKey);
			Assert.AreEqual(3, rhuRawShaderData.shaderUniforms.Length);
			var shaderUniform = new ShaderUniform[] {
				new ShaderUniform {
					Defaults = "\"white\"",
					Name = "This Is Main Texture",
					FieldName = "_Textue",
					Type = UniformType.Texture2D
				},
				new ShaderUniform {
					Defaults = "(1,1,1)",
					Name = "Color",
					FieldName = "_Color",
					Type = UniformType.ColorRGB
				},
				new ShaderUniform {
					Defaults = "10",
					Name = "This Is Float Test",
					FieldName = "_Shininess",
					Type = UniformType.Float
				},
			};
			for (var i = 0; i < shaderUniform.Length; i++) {
				var uniform = shaderUniform[i];
				var newData = rhuRawShaderData.shaderUniforms[i];
				Assert.AreEqual(uniform.Name, newData.Name);
				Assert.AreEqual(uniform.FieldName, newData.FieldName);
				Assert.AreEqual(uniform.Type, newData.Type);
				Assert.AreEqual(uniform.Defaults, newData.Defaults);
			}
		}

		private void TestSingleCode(RhuRawShaderData rhuRawShaderData) {
			Assert.AreEqual(rhuRawShaderData.MainFragShaderCode, "The ParseData");
			Assert.AreEqual(rhuRawShaderData.MainVertShaderCode, null);
			Assert.AreEqual(rhuRawShaderData.ShadowFragShaderCode, null);
			Assert.AreEqual(rhuRawShaderData.ShadowVertShaderCode, null);
		}

		private void TestMultiCode(RhuRawShaderData rhuRawShaderData) {
			Assert.AreEqual(rhuRawShaderData.MainFragShaderCode, "The Par{{}}seData");
			Assert.AreEqual(rhuRawShaderData.MainVertShaderCode, "The{} ParseData 10");
			Assert.AreEqual(rhuRawShaderData.ShadowFragShaderCode, null);
			Assert.AreEqual(rhuRawShaderData.ShadowVertShaderCode, null);
		}

		[TestMethod()]
		public void TestParseExample() {
			var newShaderCode =
@"
Shader""This is a test shader""{
	Options{
		DeferredKey(""Standard"")
	}
	Uniforms {
		_Textue(""This Is Main Texture"",texture2d) = ""white""
		_Color(""Color"",ColorRGB) = (1,1,1)
		_Shininess(""This Is Float Test"",Float) = 10
	}
	MainFragShaderCode
	{The Par{{}}seData}

	MainVertShaderCode 
	{The{} ParseData 10}
}
";
			var shaderData = RhuShaderParser.ParseShaderCode(newShaderCode);
			TestUniforms(shaderData);
			TestMultiCode(shaderData);
		}


		[TestMethod()]
		public void TestParseOne() {
			var shaderData = RhuShaderParser.ParseShaderCode(TEST_CODE + TEST_SHADERCODE);
			TestUniforms(shaderData);
			TestSingleCode(shaderData);
		}

		[TestMethod()]
		public void TestParseTwo() {
			var shaderData = RhuShaderParser.ParseShaderCode(TEST_CODE + TEST_MULTISHADERCODE);
			TestUniforms(shaderData);
			TestMultiCode(shaderData);
		}
		[TestMethod()]
		public void TestParseThree() {
			var shaderData = RhuShaderParser.ParseShaderCode(TEST_CODE_NOSPACES + TEST_SHADERCODE);
			TestUniforms(shaderData);
			TestSingleCode(shaderData);
		}
		[TestMethod()]
		public void TestParseFour() {
			var shaderData = RhuShaderParser.ParseShaderCode(TEST_CODE_NOSPACES + TEST_MULTISHADERCODE);
			TestUniforms(shaderData);
			TestMultiCode(shaderData);
		}
	}
}