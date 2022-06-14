using System;
using System.Collections.Generic;
using System.Text;

namespace RhuFerred
{
	public class SafeHashSet<T>
	{
		public HashSet<T> values = new();

		public void Add(T val) {
			lock (values) {
				values.Add(val);
			}
		}
		public void Remove(T val) {
			lock (values) {
				values.Add(val);
			}
		}
		public void ForEach(Action<T> action) {
			lock (values) {
				foreach (var item in values) {
					action.Invoke(item);
				}
			}
		}
	}
}
