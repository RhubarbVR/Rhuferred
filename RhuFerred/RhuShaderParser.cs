using System;
using System.Collections.Generic;
using System.Text;

using Veldrid;

namespace RhuFerred
{
	public static class RhuShaderParser
	{

		public static string CleanString(string code) {
			var data = "";
			var isIn = false;
			foreach (var item in code) {
				if (item == '\"') {
					isIn = !isIn;
				}
				if (item == '\'') {
					isIn = !isIn;
				}
				if (isIn) {
					data += item;
				}
				else {
					if (item == ' ') {
						continue;
					}
					if (item == '\n') {
						continue;
					}
					if (item == '\r') {
						continue;
					}
					data += item;
				}
			}
			return data;
		}

		public static string RemoveFirstInClapsing(string code) {
			var start = code.IndexOf('{') + 1;
			var end = code.LastIndexOf('}');
			return code.Substring(start, end - start);
		}

		public static int FindClosing(int startIndex,string code,char open = '{', char close = '}') {
			var depth = 0;
			for (var i = startIndex; i < code.Length; i++) {
				var currentchar = code[i];
				if(currentchar == open) {
					depth++;
				}
				if (currentchar == close) {
					depth--;
				}
				if (depth == 0) {
					return i;
				}
			}
			return -1;
		}

		public static string GetClapsing(string field, string lower, string code) {
			var fieldIndex = lower.IndexOf(field.ToLower());
			if (fieldIndex < 0) {
				return null;
			}
			var start = code.IndexOf('{', fieldIndex);
			if (start < 0) {
				return null;
			}
			var end = FindClosing(start,code);
			start++;
			return end < start ? null : end < 0 ? null : code.Substring(start, end - start);
		}

		public static string[] StringSplit(string code) {
			if (code == null) {
				return Array.Empty<string>();
			}
			var tempNewCode = code.Replace('\r', ';').Replace('\t', ';').Replace('\n',';');
			var strings = tempNewCode.Split(';', StringSplitOptions.RemoveEmptyEntries);
			var newStrings = new string[strings.Length];
			var newIndex = 0;
			for (var i = 0; i < strings.Length; i++) {
				var e = strings[i].Replace(' ', '\0');
				if (!string.IsNullOrEmpty(e)) {
					newStrings[newIndex] = strings[i];
					newIndex++;
				}
			}
			Array.Resize(ref newStrings, newIndex);
			return newStrings;
		}

		public static RhuRawShaderData ParseShaderCode(string code) {
			var codeClean = CleanString(code);
			var cleanCodeLower = codeClean.ToLower();
			var codeLower = code.ToLower();
			if (!cleanCodeLower.StartsWith("shader")) {
				throw new Exception("Not a shader");
			}
			var startOfNameShader = code.IndexOf('"');
			var endOfNameShader = code.IndexOf('"', startOfNameShader + 1);
			startOfNameShader++;
			var shaderdisplayName = code.Substring(startOfNameShader, endOfNameShader - startOfNameShader);
			code = RemoveFirstInClapsing(code);
			codeLower = RemoveFirstInClapsing(codeLower);
			var Options = StringSplit(GetClapsing("Options", codeLower, code));
			var defferedKey = "Standard";
			for (var i = 0; i < Options.Length; i++) {
				try {
					var optionLower = Options[i].ToLower();
					var startOfOption = optionLower.IndexOf('"');
					var endOfOption = optionLower.IndexOf('"', startOfOption + 1);
					startOfOption++;
					var opt = Options[i].Substring(startOfOption, endOfOption - startOfOption);
					if (optionLower.StartsWith("deferredkey")) {
						defferedKey = opt;
					}
				}
				catch { }
			}
			var Uniforms = StringSplit(GetClapsing("Uniforms", codeLower, code));
			var MainFragShaderCode = GetClapsing("MainFragShaderCode", codeLower, code);
			var MainVertShaderCode = GetClapsing("MainVertShaderCode", codeLower, code);
			var ShadowFragShaderCode = GetClapsing("ShadowFragShaderCode", codeLower, code);
			var ShadowVertShaderCode = GetClapsing("ShadowVertShaderCode", codeLower, code);
			var rawUniforms = new ShaderUniform[Uniforms.Length];
			for (var i = 0; i < Uniforms.Length; i++) {
				var uni = Uniforms[i].Replace('\n', '\0').Replace('\t', '\0').Replace('\r', '\0');
				var start = uni.IndexOf('(');
				var startOfName = uni.IndexOf('"');
				var endOfName = uni.IndexOf('"',startOfName + 1);
				var newName = uni.Remove(start);
				startOfName++;
				var displayName = uni.Substring(startOfName, endOfName - startOfName);
				var end = uni.IndexOf(')');
				var mid = uni.LastIndexOf(',',end);
				mid++;
				var type = uni.Substring(mid, end - mid);
				var notClean = uni.Remove(0, uni.IndexOf('=') + 1);
				var defalt = notClean.Replace(" ", "");
				rawUniforms[i] = new ShaderUniform {
					Defaults = defalt,
					FieldName = newName,
					Name = displayName,
					Type = Enum.Parse<UniformType>(type, true)
				};
			}
			var newSaderData = new RhuRawShaderData {
				shaderUniforms = rawUniforms,
				ShaderName = shaderdisplayName,
				MainFragShaderCode = MainFragShaderCode,
				MainVertShaderCode = MainVertShaderCode,
				ShadowFragShaderCode = ShadowFragShaderCode,
				ShadowVertShaderCode = ShadowVertShaderCode,
				DeferedKey = defferedKey
			};
			return newSaderData;
		}
	}
}
