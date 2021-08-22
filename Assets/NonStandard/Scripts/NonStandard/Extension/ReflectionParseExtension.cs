﻿using NonStandard.Data;
using NonStandard.Data.Parse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NonStandard.Extension {
	public static class ReflectionParseExtension {
		/// used by wildcard searches, for member names and enums. dramatically reduces structural typing
		public const char Wildcard = '¤';
		public static bool IsWildcardMatch(string possibility, string n, char wildcard = Wildcard) {
			if (n.Length == 1 && n[0] == wildcard) return true;
			bool startsW = n[n.Length - 1] == (wildcard), endsW = n[0] == (wildcard);
			if (startsW && endsW) { return possibility.Contains(n.Substring(1, n.Length - 2)); }
			if (endsW) { n = n.Substring(1); return possibility.EndsWith(n); }
			if (startsW) { n = n.Substring(0, n.Length - 1); }
			return possibility.StartsWith(n);
		}
		/// <param name="names">a list of names (a haystack)</param>
		/// <param name="n">name to find (the needle in the haystack)</param>
		/// <param name="sorted">if sorted alphabetically, BinarySearch can be used to speed up the search</param>
		/// <param name="wildcard"></param>
		/// <returns></returns>
		public static int FindIndexWithWildcard(string[] names, string n, bool sorted, char wildcard = Wildcard) {
			if (n.Length == 1 && n[0] == wildcard) return 0;
			bool startsW = n[n.Length - 1] == (wildcard), endsW = n[0] == (wildcard);
			if (startsW && endsW) { return Array.FindIndex(names, s => s.Contains(n.Substring(1, n.Length - 2))); }
			if (endsW) { n = n.Substring(1); return Array.FindIndex(names, s => s.EndsWith(n)); }
			if (startsW) { n = n.Substring(0, n.Length - 1); }
			int index = sorted ? Array.BinarySearch(names, n) : (startsW)
				? Array.FindIndex(names, s => s.StartsWith(n)) : Array.IndexOf(names, n);
			if (startsW && index < 0) {
				index = ~index;
				return (index < names.Length && names[index].StartsWith(n)) ? index : -1;
			}
			return index;
		}
		/// <summary>
		/// tries to converts text, which might include a wildcard character, into a proper enum value
		/// </summary>
		public static bool TryConvertEnumWildcard(Type enumType, string enumName, out object value, char wildcard = Wildcard) {
			bool startsWith = enumName[enumName.Length - 1] == (wildcard), endsWidth = enumName[0] == (wildcard);
			if (startsWith || endsWidth) {
				Array a = Enum.GetValues(enumType);
				string[] names = new string[a.Length];
				for (int i = 0; i < a.Length; ++i) { names[i] = a.GetValue(i).ToString(); }
				int index = FindIndexWithWildcard(names, enumName, false, wildcard);
				if (index < 0) { value = null; return false; }
				enumName = names[index];
			}
			value = Enum.Parse(enumType, enumName);
			return true;
		}
		public static object GetValue(object obj, string variableNamePath, object defaultValue, List<object> out_path = null) {
			//Show.Log(variableNamePath);
			IList<object> vars = variableNamePath.Split(".");
			return GetValueFromRawPath(obj, vars, defaultValue, out_path);
		}
		public static object GetValueFromRawPath(object obj, IList<object> rawPath, object defaultValue = null, List<object> out_compiledPath = null) {
			object result = obj;
			for (int i = 0; i < rawPath.Count; ++i) {
				string pathStr = rawPath[i].ToString();
				result = GetValueIndividual(obj, pathStr, out object path, defaultValue);
				//Show.Log(obj+"["+ pathStr + "] = ("+path+") \'"+result+"\'");
				if (out_compiledPath != null) {
					out_compiledPath.Add(path);
				}
				bool done = i == rawPath.Count - 1;
				if (!done) {
					if (result == null) return defaultValue;
				}
				obj = result;
			}
			return result;
		}
		/// <summary>
		/// used when the path is known, having been compiled already by <see cref="GetValueFromRawPath(object, IList{object}, object, List{object})"/>
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="path"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryGetValueCompiled(object scope, object path, out object result) {
			switch (path) {
			case FieldInfo fi: result = fi.GetValue(scope); return true;
			case PropertyInfo pi: result = pi.GetValue(scope); return true;
			case string s: return TryGetValue_Dictionary(scope, ref path, out result);
			}
			result = null;
			return false;
		}
		/// <summary>
		/// used when the path is known, having been compiled already by <see cref="GetValueFromRawPath(object, IList{object}, object, List{object})"/>
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TrySetValueCompiled(object scope, object path, object value) {
			switch (path) {
			case FieldInfo fi:
				if (value != null && !fi.FieldType.IsAssignableFrom(value.GetType())) {
					CodeConvert.Convert(ref value, fi.FieldType);
				}
				fi.SetValue(scope, value);
				return true;
			case PropertyInfo pi:
				if (!pi.CanWrite) return false;
				if (value != null && !pi.PropertyType.IsAssignableFrom(value.GetType())) {
					CodeConvert.Convert(ref value, pi.PropertyType);
				}
				pi.SetValue(scope, value);
				return true;
			case string s: return TrySetValue_Dictionary(scope, ref s, value);
			}
			return false;
		}
		public static bool TryGetValueCompiledPath(object scope, IList<object> alreadyCompiledPath, out object result) {
			object cursor = scope;
			for(int i = 0; i < alreadyCompiledPath.Count; ++i) {
				if(!TryGetValueCompiled(cursor, alreadyCompiledPath[i], out cursor)) {
					result = null;
					return false;
				}
			}
			result = cursor;
			return true;
		}
		public static bool TrySetValueCompiledPath(object scope, IList<object> alreadyCompiledPath, object result) {
			object cursor = scope;
			int last = alreadyCompiledPath.Count - 1;
			for (int i = 0; i < last; ++i) {
				if (!TryGetValueCompiled(cursor, alreadyCompiledPath[i], out cursor)) {
					return false;
				}
			}
			if (!TrySetValueCompiled(cursor, alreadyCompiledPath[last], result)) {
				return false;
			}
			return true;
		}
		public static object GetValueIndividual(object obj, string variableName, out object path, object defaultValue, char wildcard = Wildcard) {
			if(!TryGetValue(obj, variableName, out object value, out path)) {
				return defaultValue;
			}
			return value;
		}

		/// <summary>
		/// trys to get a member variable based on the name requested. the name can include a wildcard
		/// </summary>
		/// <param name="scope">where to get the variable from</param>
		/// <param name="name">the name of the variable from</param>
		/// <param name="value">where to put the variable when it is found</param>
		/// <param name="path">the specific variable retrieved. likely a FieldInfo or PropertyInfo. can be an object (likely a string) in the case of a Dictionary</param>
		/// <returns></returns>
		public static bool TryGetValue(object scope, string name, out object value, out object path) {
			Type scopeType = scope.GetType();
			KeyValuePair<Type, Type> dType = scopeType.GetIDictionaryType();
			bool result;
			if (dType.Key != null) {
				object keyName = name;
				result = TryGetValue_Dictionary(scope, ref keyName, out value, scopeType, dType.Key);
				if (!result) { value = name; path = null; } else { path = keyName; }
				return result;
			}
			result = TryGetValue_Object(scope, name, out value, out MemberInfo mi);
			path = mi;
			return result;
		}
		public static bool TrySetValue(object scope, string keyName, object value, out object path) {
			Type scopeType = scope.GetType();
			KeyValuePair<Type, Type> dType = scopeType.GetIDictionaryType();
			bool result;
			if (dType.Key != null) {
				result = TrySetValue_Dictionary(scope, ref keyName, value, scopeType, dType.Key);
				if (!result) { path = null; } else { path = keyName; }
				return result;
			}
			result = TrySetValue_Object(scope, keyName, value, out MemberInfo mi);
			path = mi;
			return result;
		}
		public static bool TryGetValue_Object(object scope, string name, out object value, out MemberInfo memberInfo, char wildcard = Wildcard) {
			if (name == null) {
				value = null;
				memberInfo = null;
				return false;
			}
			Type scopeType = scope.GetType();
			memberInfo = null;
			if (name.Length > 0 && (name[0] == wildcard || name[name.Length - 1] == wildcard)) {
				FieldInfo[] fields = scopeType.GetFields();
				string[] names = Array.ConvertAll(fields, f => f.Name);
				int index = FindIndexWithWildcard(names, name, false, wildcard);
				if (index >= 0) { memberInfo = fields[index]; value = fields[index].GetValue(scope); return true; }
				PropertyInfo[] props = scopeType.GetProperties();
				names = Array.ConvertAll(props, p => p.Name);
				index = FindIndexWithWildcard(names, name, false, wildcard);
				if (index >= 0) { memberInfo = props[index]; value = props[index].GetValue(scope); return true; }
			} else {
				FieldInfo field = scopeType.GetField(name);
				if (field != null) { memberInfo = field; value = field.GetValue(scope); return true; }
				PropertyInfo prop = scopeType.GetProperty(name);
				if (prop != null) { memberInfo = prop; value = prop.GetValue(scope, null); return true; }
			}
			value = null;
			return false;
		}
		public static bool TrySetValue_Object(object scope, string name, object value, out MemberInfo memberInfo, char wildcard = Wildcard) {
			Type scopeType = scope.GetType();
			memberInfo = null;
			if (name.Length > 0 && (name[0] == wildcard || name[name.Length - 1] == wildcard)) {
				FieldInfo[] fields = scopeType.GetFields();
				string[] names = Array.ConvertAll(fields, f => f.Name);
				int index = FindIndexWithWildcard(names, name, false, wildcard);
				if (index >= 0) { memberInfo = fields[index]; fields[index].SetValue(scope, value); return true; }
				PropertyInfo[] props = scopeType.GetProperties();
				names = Array.ConvertAll(props, p => p.Name);
				index = FindIndexWithWildcard(names, name, false, wildcard);
				if (index >= 0) { memberInfo = props[index]; props[index].SetValue(scope, value); return true; }
			} else {
				FieldInfo field = scopeType.GetField(name);
				if (field != null) { memberInfo = field; field.SetValue(scope, value); return true; }
				PropertyInfo prop = scopeType.GetProperty(name);
				if (prop != null) { memberInfo = prop; prop.SetValue(scope, value); return true; }
			}
			value = null;
			return false;
		}
		/// <param name="scope">the dictionary as an object</param>
		/// <param name="key">can change in the function if there is a wildcard that must be resolved</param>
		/// <param name="value"></param>
		/// <param name="scopeType">optional. used to help figure out what kind of dictionary scope is</param>
		/// <param name="keyType">optional. used to help figure out what kind of value should be pulled out of the scope at the key</param>
		/// <returns>true if the key was found in the given scope</returns>
		public static bool TryGetValue_Dictionary(object scope, ref object key, out object value, Type scopeType = null, Type keyType = null) {
			value = null;
			if (scopeType == null) { scopeType = scope.GetType(); }
			if (keyType == null) { keyType = scopeType.GetIDictionaryType().Key; }
			if (key is string n) { key = ConvertWildcardIntoDictionaryKey(scope, n, scopeType); }
			// how to generically interface with standard Dictionary objects
			IDictionary dict = scope as IDictionary;
			if (dict != null) {
				if (dict.Contains(key)) { value = dict[key]; return true; }
				return false;
			}
			return TryGetValue_DictionaryReflective(scopeType, scope, key, out value);
		}
		public static bool TrySetValue_Dictionary(object scope, ref string name, object value, Type scopeType = null, Type keyType = null) {
			if (scopeType == null) { scopeType = scope.GetType(); }
			if (keyType == null) { keyType = scopeType.GetIDictionaryType().Key; }
			if (keyType == typeof(string)) { name = ConvertWildcardIntoDictionaryKey(scope, name, scopeType); }
			// how to generically interface with standard Dictionary objects
			IDictionary dict = scope as IDictionary;
			if (dict != null) {
				dict[name] = value;
				return true;
			}
			return TrySetValue_DictionaryReflective(scopeType, scope, name, value);
		}
		public static bool TryGetValue_DictionaryReflective(Type scopeType, object scope, object keyName, out object value) {
			MethodInfo mi = scopeType.GetMethod("TryGetValue");
			if (mi == null) {
				Show.Error("couldn't find TryGetValue, need a method to get data out of a dictionary. How about:" +
					scopeType.GetMethods().JoinToString(", ", m => m.Name));
				value = null;
				return false;
			} else {
				object[] parameters = new object[] { keyName, null };
				value = mi.Invoke(scope, parameters);
				if ((bool)value != false) { value = parameters[1]; } else { value = null; return false; }
			}
			return true;
		}
		public static bool TrySetValue_DictionaryReflective(Type scopeType, object scope, string name, object value) {
			KeyValuePair<Type, Type> dTypes = scopeType.GetIDictionaryType();
			MethodInfo mi = scopeType.GetMethod("Add", new Type[] { dTypes.Key, dTypes.Value });
			if (mi == null) {
				Show.Error("couldn't find Add, need a method to add data to a dictionary. How about:" +
					scopeType.GetMethods().JoinToString(", ", m => m.Name));
				value = null;
				return false;
			} else {
				object[] parameters = new object[] { name, value };
				value = mi.Invoke(scope, parameters);
				//if ((bool)value != false) { value = parameters[1]; } else { value = null; return false; }
			}
			return true;
		}
		public static string ConvertWildcardIntoDictionaryKey(object scope, string name, Type scopeType = null, char wildcard = Wildcard) {
			if (scopeType == null) { scopeType = scope.GetType(); }
			if (name[0] == wildcard || name[name.Length - 1] == wildcard) {
				MethodInfo getKey = null;
				IEnumerator en = (IEnumerator)scopeType.GetMethod("GetEnumerator", Type.EmptyTypes).Invoke(scope, new object[] { });
				while (en.MoveNext()) {
					object kvp = en.Current;
					if (getKey == null) { getKey = kvp.GetType().GetProperty("Key").GetGetMethod(); }
					string memberName = getKey.Invoke(kvp, null) as string;
					if (IsWildcardMatch(memberName, name, wildcard)) { return memberName; }
				}
			}
			return name;
		}
	}
}