using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NonStandard {
	public static class StringExtension {
		public static string RemoveFromFront(this string str, string trimMe) {
			if (str.StartsWith(trimMe)) { return str.Substring(trimMe.Length); }
			return str;
		}

		public static string Indentation(int depth, string indent = "  ") {
			StringBuilder sb = new StringBuilder();
			while (depth-- > 0) { sb.Append(indent); }
			return sb.ToString();
		}

		public static string IndentLine(this string str, int depth, string indent = "  ") {
			return Indentation(depth, indent) + str;
		}

		/// <summary>
		/// converts a string from it's code to it's compiled form, with processed escape sequences
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Unescape(this string str) {
			StringBuilder sb = new StringBuilder();
			int stringStarted = 0;
			for (int i = 0; i < str.Length; ++i) {
				char c = str[i];
				if (c == '\\') {
					if (stringStarted != i) { sb.Append(str.Substring(stringStarted, i - stringStarted)); }
					++i;
					if (i >= str.Length) { break; }
					sb.Append(Unescape(str[i]));
					stringStarted = i+1;
				}
			}
			sb.Append(str.Substring(stringStarted, str.Length - stringStarted));
			return sb.ToString();
		}
		public static char Unescape(char c) {
			switch (c) {
			case 'a': return '\a';
			case 'b': return '\b';
			case 'n': return '\n';
			case 'r': return '\r';
			case 'f': return '\f';
			case 't': return '\t';
			case 'v': return '\v';
			}
			return c;
		}

		public static string Escape(char c) {
			switch (c) {
			case '\a': return ("\\a");
			case '\b': return ("\\b");
			case '\n': return ("\\n");
			case '\r': return ("\\r");
			case '\f': return ("\\f");
			case '\t': return ("\\t");
			case '\v': return ("\\v");
			case '\'': return ("\\\'");
			case '\"': return ("\\\"");
			case '\\': return ("\\\\");
			}
			return null;
		}
		public static string Escape(this string str) {
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < str.Length; ++i) {
				char c = str[i];
				string escaped = Escape(c);
				if (escaped != null) {
					sb.Append(escaped);
				} else {
					if (c < 32 || (c > 127 && c < 512)) {
						sb.Append("\\").Append(Convert.ToString((int)c, 8));
					} else if (c >= 512) {
						sb.Append("\\u").Append(((int)c).ToString("X4"));
					} else {
						sb.Append(c);
					}
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// stringifies an object using custom NonStandard rules
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="pretty"></param>
		/// <param name="showType">include "=TypeName" if there could be ambiguity because of inheritance</param>
		/// <param name="depth"></param>
		/// <param name="rStack">used to prevent recursion stack overflows</param>
		/// <param name="filter">object0 is the object, object1 is the member, object2 is the value. if it returns null, print as usual. if returns "", skip print.</param>
		/// <returns></returns>
		public static string Stringify(this object obj, bool pretty = true, bool showType = true, bool showNulls = false, bool showFirstBoundary = true, int depth = 0, List<object> rStack = null, Func<object, object, object, string> filter = null) {
			if (obj == null) return showNulls ? "null" : "";
			if (filter != null) { string res = filter.Invoke(obj, null, null); if (res != null) { return res; } }
			Type t = obj.GetType();
			MethodInfo stringifyMethod = t.GetMethod("Stringify", Type.EmptyTypes);
			if (stringifyMethod != null) { return stringifyMethod.Invoke(obj, Array.Empty<object>()) as string; }
			StringBuilder sb = new StringBuilder();
			Type iListElement = t.GetIListType();
			bool showTypeHere = showType; // no need to print type if there isn't type ambiguity
			if (showType) {
				Type b = t.BaseType; // if the parent class is a base class, there isn't any ambiguity
				if (b == typeof(ValueType) || b == typeof(System.Object) || b == typeof(Array) ||
					t.GetCustomAttributes(false).FindIndex(o => o.GetType() == typeof(StringifyHideTypeAttribute)) >= 0) { showTypeHere = false; }
			}
			string s = obj as string;
			if (s != null || t.IsPrimitive || t.IsEnum) {
				if (s != null) {
					if (showFirstBoundary) { sb.Append("\""); }
					sb.Append(s.Escape());
					if (showFirstBoundary) { sb.Append("\""); }
				} else {
					sb.Append(obj.ToString());
				}
				return sb.ToString();
			}
			if (rStack == null) { rStack = new List<object>(); }
			int recursionIndex = rStack.IndexOf(obj);
			if (recursionIndex >= 0) {
				sb.Append("/* recursed " + (rStack.Count - recursionIndex) + " */");
				return sb.ToString();
			}
			rStack.Add(obj);
			if (t.IsArray || iListElement != null) {
				if (showFirstBoundary) sb.Append("[");
				if (showTypeHere) {
					if (pretty) { sb.Append("\n" + StringExtension.Indentation(depth + 1)); }
					sb.Append("=\"" + obj.GetType().ToString() + "\" " + obj.GetType().BaseType);
				}
				IList list = obj as IList;

				for (int i = 0; i < list.Count; ++i) {
					if (!showNulls && list[i] == null) continue;
					if (i > 0) { sb.Append(","); }
					if (pretty && !iListElement.IsPrimitive) { sb.Append("\n" + StringExtension.Indentation(depth + 1)); }
					if (filter == null) {
						sb.Append(Stringify(list[i], pretty, showType, showNulls, true, depth + 1, rStack));
					} else {
						FilterElement(sb, obj, i, list[i], pretty, showType, showNulls, true, depth, rStack, filter);
					}
				}
				if (pretty) { sb.Append("\n" + StringExtension.Indentation(depth)); }
				if (showFirstBoundary) sb.Append("]");
			} else {
				KeyValuePair<Type, Type> kvp = t.GetIDictionaryType();
				bool isDict = kvp.Key != null;
				if (showFirstBoundary) sb.Append("{");
				if (showTypeHere) {
					if (pretty) { sb.Append("\n" + StringExtension.Indentation(depth + 1)); }
					sb.Append("=\"" + obj.GetType().ToString() + "\"");
				}
				if (!isDict) {
					FieldInfo[] fi = t.GetFields();
					for (int i = 0; i < fi.Length; ++i) {
						object val = fi[i].GetValue(obj);
						if (!showNulls && val == null) continue;
						if (i > 0 || showTypeHere) { sb.Append(","); }
						if (pretty) { sb.Append("\n" + StringExtension.Indentation(depth + 1)); }
						if (filter == null) {
							sb.Append(fi[i].Name).Append(pretty ? " : " : ":");
							sb.Append(Stringify(val, pretty, showType, showNulls, true, depth + 1, rStack));
						} else {
							FilterElement(sb, obj, fi[i].Name, val,
								pretty, showType, showNulls, false, depth, rStack, filter);
						}
					}
				} else {
					MethodInfo getEnum = t.GetMethod("GetEnumerator", new Type[] { });
					MethodInfo getKey = null, getVal = null;
					object[] noparams = Array.Empty<object>();
					IEnumerator e = getEnum.Invoke(obj, noparams) as IEnumerator;
					bool printed = false;
					while (e.MoveNext()) {
						object o = e.Current;
						if (getKey == null) { getKey = o.GetType().GetProperty("Key").GetGetMethod(); }
						if (getVal == null) { getVal = o.GetType().GetProperty("Value").GetGetMethod(); }
						if (printed || showTypeHere) { sb.Append(","); }
						if (pretty) { sb.Append("\n" + StringExtension.Indentation(depth + 1)); }
						object k = getKey.Invoke(o, noparams);
						object v = getVal.Invoke(o, noparams);
						if (!showNulls && v == null) { continue; }
						if (filter == null) {
							sb.Append(k).Append(pretty ? " : " : ":");
							sb.Append(Stringify(v, pretty, showType, showNulls, true, depth + 1, rStack));
							printed = true;
						} else {
							printed = FilterElement(sb, obj, k, v, pretty, showType, showNulls, false, depth, rStack, filter);
						}
					}
				}
				if (pretty) { sb.Append("\n" + StringExtension.Indentation(depth)); }
				if (showFirstBoundary) sb.Append("}");
			}
			if (sb.Length == 0) { sb.Append(obj.ToString()); }
			return sb.ToString();
		}

		private static bool FilterElement(StringBuilder sb, object obj, object key, object val,
			bool pretty, bool includeType, bool showNulls, bool isArray, int depth, List<object> recursionStack,
			Func<object, object, object, string> filter = null) {
			bool unfiltered = true;
			if (filter != null) {
				string result = filter.Invoke(obj, key, val);
				unfiltered = result == null;
				if (!unfiltered && result.Length != 0) { sb.Append(result); return true; }
			}
			if (unfiltered) {
				if (!isArray) { sb.Append(key).Append(pretty ? " : " : ":"); }
				sb.Append(Stringify(val, pretty, includeType, showNulls, true, depth + 1, recursionStack));
				return true;
			}
			return false;
		}
	}
	public class StringifyHideTypeAttribute : System.Attribute { }
}