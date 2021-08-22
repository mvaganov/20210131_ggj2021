using System;
using System.Text;
using UnityEngine;

namespace NonStandard.Extension {
	public static class TransformExtention {
		public static string HierarchyPath(this Transform t, string separator = "/") {
			StringBuilder sb = new StringBuilder();
			sb.Append(t.name);
			t = t.parent;
			while (t != null) {
				string str = t.name;
				if (str.Contains("/")) { str = "\""+str.Escape()+"\""; }
				sb.Insert(0, str + separator);
				t = t.parent;
			}
			return sb.ToString();
		}

		public static int IndexOfChild(this Transform t, Func<Transform, bool> predicate) {
			for(int i = 0; i < t.childCount; ++i) {
				Transform c = t.GetChild(i);
				if (c != null && predicate.Invoke(c)) { return i; }
			}
			return -1;
		}
	}
}