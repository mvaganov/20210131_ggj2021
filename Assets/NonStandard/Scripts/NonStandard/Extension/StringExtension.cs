using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NonStandard.Data.Parse;

namespace NonStandard.Extension {
	public static class StringExtension {
		public static bool IsSubstringAt(this string str, string substring, int index) {
			if (index < 0 || index + substring.Length > str.Length) { return false; }
			for (int i = 0; i < substring.Length; ++i) {
				if (str[index + i] != substring[i]) { return false; }
			}
			return true;
		}
		public static string RemoveFromFront(this string str, string trimMe) {
			if (str.StartsWith(trimMe)) { return str.Substring(trimMe.Length); }
			return str;
		}
		public static string DefaultIndentation = "  ";

		/// <param name="indent">if null, use <see cref="DefaultIndentation"/></param>
		/// <param name="depth"></param>
		public static string Indentation(int depth, string indent = null) {
			if (indent == null) { indent = DefaultIndentation; }
			StringBuilder sb = new StringBuilder();
			while (depth-- > 0) { sb.Append(indent); }
			return sb.ToString();
		}

		/// <param name="str">a string with one or more lines, separated by '\n'</param>
		/// <param name="indent">if null, use <see cref="DefaultIndentation"/></param>
		/// <param name="depth"></param>
		public static string Indent(this string str, string indent = null, int depth = 1) {
			string indentation = Indentation(depth, indent);
			return ForEachBetween(str, s => indentation + s);
		}

		/// <param name="str"></param>
		/// <param name="indent">if null, use <see cref="DefaultIndentation"/></param>
		/// <param name="depth"></param>
		public static string Unindent(this string str, string indent = null, int depth = 1) {
			string indentation = Indentation(depth, indent);
			return ForEachBetween(str, s => RemoveFromFront(s, indentation));
		}

		public static string ForEachBetween(this string str, Func<string, string> manipulateSubstring, string delimiter = "\n") {
			string[] lines = str.Split(delimiter);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < lines.Length; ++i) {
				if (i > 0) sb.Append(delimiter);
				sb.Append(manipulateSubstring(lines[i]));
			}
			return sb.ToString();
		}
		public static string[] Split(this string str, string delimiter = "\n") {
			List<int> lineEndings = GenerateIndexTable(delimiter);
			string[] lines = new string[lineEndings.Count + 1];
			int lineStart = 0;
			for (int i = 0; i < lineEndings.Count; ++i) {
				lines[i] = str.Substring(lineStart, lineEndings[i]);
				lineStart = lineEndings[i] + delimiter.Length;
			}
			lines[lineEndings.Count] = str.Substring(lineStart, str.Length);
			return lines;
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
					//sb.Append(str[i].LiteralUnescape());
					ParseResult parseResult = str.UnescapeStringSequenceAt(i);
					if (!parseResult.IsError) { sb.Append(parseResult.replacementValue.ToString()); } else { throw new FormatException(parseResult.error.ToString()); }
					stringStarted = i + 1;
				}
			}
			sb.Append(str.Substring(stringStarted, str.Length - stringStarted));
			return sb.ToString();
		}
		public static string Escape(this string str) {
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < str.Length; ++i) {
				char c = str[i];
				string escaped = c.LiteralEscape();
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
		public static int CountNumericCharactersAt(this string str, int index, int numberBase, bool includeNegativeSign, bool includeSingleDecimal) {
			return CountNumericCharactersAt(str, index, numberBase, includeNegativeSign, includeSingleDecimal, out int _);
		}
		public static int CountNumericCharactersAt(this string str, int index, int numberBase, bool includeNegativeSign, bool includeSingleDecimal, out int foundDecimal) {
			int numDigits = 0;
			foundDecimal = -1;
			while (index + numDigits < str.Length) {
				char c = str[index + numDigits];
				bool stillGood = false;
				if (c.IsValidNumber(numberBase)) {
					stillGood = true;
				} else {
					if (includeNegativeSign && numDigits == 0 && c == '-') {
						stillGood = true;
					} else if (includeSingleDecimal && c == '.' && foundDecimal < 0) {
						foundDecimal = index;
						stillGood = true;
					}
				}
				if (stillGood) { numDigits++; } else break;
			}
			return numDigits;
		}
		public static ParseResult NumberParse(this string str, int index, int characterCount, int numberBase, bool includeDecimal) {
			ParseResult pr = new ParseResult(0, null);
			long sum = 0;
			char c = str[index];
			bool isNegative = c == '-';
			if (isNegative) { ++index; }
			bool isDecimal = c == '.';
			int numDigits;
			if (!isDecimal) {
				numDigits = str.CountNumericCharactersAt(index, numberBase, false, false);
				int b = 1, onesPlace = index + numDigits - 1;
				for (int i = 0; i < numDigits; ++i) {
					sum += str[onesPlace - i].ToNumericValue() * b;
					b *= numberBase;
				}
				if (isNegative) sum *= -1;
				pr.replacementValue = (sum < int.MaxValue) ? (int)sum : sum;
				index += numDigits;
			}
			++index;
			double fraction = 0;
			if (includeDecimal && index < str.Length && str[index - 1] == '.') {
				numDigits = str.CountNumericCharactersAt(index, numberBase, false, false);
				if (numDigits == 0) { pr.SetError("decimal point with no subsequent digits", index, 1, index); }
				long b = numberBase;
				for (int i = 0; i < numDigits; ++i) {
					fraction += str[index + i].ToNumericValue() / (double)b;
					b *= numberBase;
				}
				if (isNegative) fraction *= -1;
				pr.replacementValue = (sum + fraction);
			}
			pr.lengthParsed = characterCount;
			return pr;
		}
		public static ParseResult HexadecimalParse(this string str, int index) { return NumberParse(str, index + 2, 16, false); }
		public static ParseResult NumericParse(this string str, int index) { return NumberParse(str, index, 10, true); }
		public static ParseResult IntegerParse(this string str, int index) { return NumberParse(str, index, 10, false); }
		public static ParseResult NumberParse(this string str, int index, int numberBase, bool includeDecimal) {
			return str.NumberParse(index, CountNumericCharactersAt(str, index, numberBase, true, true), numberBase, includeDecimal);
		}
		public static ParseResult UnescapeStringSequenceAt(this string str, int index) {
			ParseResult r = new ParseResult(0, null); // by default, nothing happened
			if (str.Length <= index) { return r.SetError("invalid arguments"); }
			if (str[index] != '\\') { return r.SetError("expected escape sequence starting with '\\'"); }
			if (str.Length <= index + 1) { return r.SetError("unable to parse escape sequence at end of string", 1, 0, 1); }
			char c = str[index + 1];
			switch (c) {
			case '\n': return new ParseResult(index + 2, "");
			case '\r':
				if (str.Length <= index + 2 || str[index + 2] != '\n') {
					return new ParseResult(index, "", "expected windows line ending", 2, 0, 2);
				}
				return new ParseResult(index + 3, "");
			case 'a': return new ParseResult(2, "\a");
			case 'b': return new ParseResult(2, "\b");
			case 'e': return new ParseResult(2, ((char)27).ToString());
			case 'f': return new ParseResult(2, "\f");
			case 'r': return new ParseResult(2, "\r");
			case 'n': return new ParseResult(2, "\n");
			case 't': return new ParseResult(2, "\t");
			case 'v': return new ParseResult(2, "\v");
			case '\\': return new ParseResult(2, "\\");
			case '\'': return new ParseResult(2, "\'");
			case '\"': return new ParseResult(2, "\"");
			case '?': return new ParseResult(2, "?");
			case 'x': return str.NumberParse(index + 2, 2, 16, false).AddToLength(2).ForceCharSubstitute();
			case 'u': return str.NumberParse(index + 2, 4, 16, false).AddToLength(2).ForceCharSubstitute();
			case 'U': return str.NumberParse(index + 2, 8, 16, false).AddToLength(2).ForceCharSubstitute();
			case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': {
				int digitCount = 1;
				do {
					if (str.Length <= index + digitCount + 1) break;
					c = str[index + digitCount + 1];
					if (c < '0' || c > '7') break;
					++digitCount;
				} while (digitCount < 3);
				return str.NumberParse(index + 1, digitCount, 8, false).AddToLength(1).ForceCharSubstitute();
			}
			}
			return r.SetError("unknown escape sequence", 1, 0, 1);
		}

		public static string StringifyTypeOfDictionary(Type t) { return "=\"" + t.ToString() + "\""; }

		/// <summary>
		/// stringifies an object using custom NonStandard rules
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="pretty"></param>
		/// <param name="showType">include "=TypeName" if there could be ambiguity because of inheritance</param>
		/// <param name="depth"></param>
		/// <param name="rStack">used to prevent recursion stack overflows</param>
		/// <param name="filter">object0 is the object, object1 is the member, object2 is the value. if it returns null, print as usual. if returns "", skip print.</param>
		/// <param name="howToMarkStrongType">if null, use <see cref="StringifyTypeOfDictionary"/>, which is expected by <see cref="NonStandard.Data.CodeConvert.TryParse"/></param>
		/// <returns></returns>
		public static string Stringify(this object obj, bool pretty = true, bool showType = true, bool showNulls = false, bool showFirstBoundary = true, int depth = 0, List<object> rStack = null, Func<object, object, object, string> filter = null, Func<Type,string> howToMarkStrongType = null) {
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
					if (howToMarkStrongType == null) {
						howToMarkStrongType = StringifyTypeOfDictionary;
					}
					sb.Append(howToMarkStrongType(obj.GetType()));
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

		public static bool TryParseEnum<T>(this string value, out T result) where T : struct, IConvertible {
			try {
				result = (T)Enum.Parse(typeof(T), value, true);
				return true;
			} catch (Exception) {
				//string[] names = Enum.GetNames(typeof(T));
				//string errorMessage = $"failed conversion \"{value}\" into {typeof(T)}\nvalid values: {names.JoinToString()}\n" + e;
				//Show.Error(errorMessage);
				result = default;
				return false;
			}
		}

		/// <returns>the indexes of the given substring in this string</returns>
		public static List<int> GenerateIndexTable(this string haystack, string needle = "\n") {
			List<int> found = new List<int>();
			int limit = haystack.Length - needle.Length;
			for(int i = 0; i < limit; ++i) {
				if(haystack.IsSubstringAt(needle, i)) { found.Add(i); }
			}
			return found;
		}

		/// <summary>
		/// searches for the given file name in the exe's directory, as well as parent directories
		/// </summary>
		/// <param name="filePathAndName"></param>
		/// <param name="numDirectoriesBackToLook">how many parent directories backward to look</param>
		public static string StringFromFile(string filePathAndName, int numDirectoriesBackToLook = 3) {
			string text = null;
			int directoriesSearched = 0;
			//string originalDir = System.IO.Directory.GetCurrentDirectory();
			do {
				try {
					text = System.IO.File.ReadAllText(filePathAndName);
				} catch (System.IO.FileNotFoundException e) {
					if (directoriesSearched < numDirectoriesBackToLook) {
						System.IO.Directory.SetCurrentDirectory("..");
						directoriesSearched++;
					} else {
						//System.Console.WriteLine($"{filePathAndName} not found @{originalDir}");
						throw e;
					}
				}
			} while (text == null);
			text = text.Replace("\r\n", "\n");
			text = text.Replace("\n\r", "\n");
			text = text.Replace("\r", "\n");
			return text;
		}
	}
	public class StringifyHideTypeAttribute : System.Attribute { }
}