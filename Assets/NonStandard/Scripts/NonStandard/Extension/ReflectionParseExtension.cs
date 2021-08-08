using System;
using System.Collections.Generic;
using System.Reflection;

namespace NonStandard.Extension {
	public static class ReflectionParseExtension {
		public static object GetMemberValueUsingReflection(this object obj, IList<MemberInfo> path) {
			object r = obj;
			for(int i = 0; i < path.Count; ++i) {
				switch (path[i]) {
				case FieldInfo fi: r = fi.GetValue(r); break;
				case PropertyInfo pi: r = pi.GetValue(r); break;
				}
			}
			return r;
		}
		public static object GetValue(this Type type, object obj, string variableNamePath, List<MemberInfo> out_path = null,
		BindingFlags bindAttr = BindingFlags.Public | BindingFlags.Instance) {
			//Show.Log(variableNamePath);
			string[] vars = variableNamePath.Split(".");
			Type t = type;
			object result = obj;
			for (int i = 0; i < vars.Length; ++i) {
				result = GetValueIndividual(t, result, vars[i], out MemberInfo path, bindAttr);
				if(out_path != null) { out_path.Add(path); }
				bool done = i == vars.Length - 1;
				if (!done) {
					if (result == null) return null;
					t = result.GetType();
				}
			}
			return result;
		}
		private static object GetValueIndividual(this Type type, object obj, string variableName, out MemberInfo path, 
		BindingFlags bindAttr = BindingFlags.Public | BindingFlags.Instance) {
			FieldInfo fi = type.GetField(variableName, bindAttr);
			if(fi != null) { path = fi; return fi.GetValue(obj); }
			PropertyInfo pi = type.GetProperty(variableName, bindAttr);
			if (pi != null) { path = pi; return pi.GetValue(obj); }
			return path = null;
		}
	}
}