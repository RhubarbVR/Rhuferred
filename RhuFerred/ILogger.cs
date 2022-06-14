using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	public class BasicLogger : ILogger
	{
		public void Error(string message) {
			Console.WriteLine(message);
		}

		public void Info(string message) {
			Console.WriteLine(message);
		}

		public void Log(string message) {
			Console.WriteLine(message);
		}

		public void Warn(string message) {
			Console.WriteLine(message);
		}
	}

	public interface ILogger
	{
		public void Info(string message);

		public void Log(string message);
		public void Warn(string message);

		public void Error(string message);
	}
}
