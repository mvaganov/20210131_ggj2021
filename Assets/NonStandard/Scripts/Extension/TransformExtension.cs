using System.Text;
using UnityEngine;

public static class TransformExtention {
	public static string HierarchyPath(this Transform t, string separator = "/") {
		StringBuilder sb = new StringBuilder();
		sb.Append(t.name);
		t = t.parent;
		while (t != null) {
			sb.Insert(0, t.name + separator);
			t = t.parent;
		}
		return sb.ToString();
	}
}