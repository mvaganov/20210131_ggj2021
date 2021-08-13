using NonStandard.Data.Parse;
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
		public static object GetValue(this Type type, object obj, string variableNamePath, object defaultValue, List<object> out_path = null) {
			//Show.Log(variableNamePath);
			string[] vars = variableNamePath.Split(".");
			Type t = type;
			object result = obj;
			for (int i = 0; i < vars.Length; ++i) {
				result = GetValueIndividual(t, result, vars[i], out object path, defaultValue);
				if (out_path != null) { out_path.Add(path); }
				bool done = i == vars.Length - 1;
				if (!done) {
					if (result == null) return null;
					t = result.GetType();
				}
			}
			return result;
		}
		//public static bool SetValueIndividual(this Type type, object obj, string variableName, object value) {
		//	FieldInfo fi = type.GetField(variableName, bindAttr);
		//	if (fi != null) { fi.SetValue(obj, value); return true; }
		//	PropertyInfo pi = type.GetProperty(variableName, bindAttr);
		//	if (pi != null) { pi.SetValue(obj, value); return true; }

		//	//Show.Log("needa find "+variableName+" from "+obj);
		//	//bool foundIt = CodeRules.op_SearchForMember(variableName, out object value, out Type _, obj);
		//	//if (!foundIt) {
		//		//Show.Log("ASSIGNING DEFAULT VALUE "+defaultValue+" "+defaultValue.GetType());
		//		//value = defaultValue;
		//	//}
		//	//path = variableName;
		//	//return value;
		//	return false;
		//}
		public static object GetValueIndividual(this Type type, object obj, string variableName, out object path, object defaultValue) {
			if(!CodeRules.TryGetValue(obj, variableName, out object value, out path)) {
				return defaultValue;
			}
			return value;
		}
		private static void SetValue(this Type type, object obj, IList<string> path, object value) {
			object cursor = obj;
			Type t = type;
			for (int i = 0; i < path.Count; ++i) {
				obj = t.GetValueIndividual(cursor, path[i], out object step, null);

			}
		}
	}
}