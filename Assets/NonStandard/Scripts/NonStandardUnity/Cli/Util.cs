using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace NonStandard.Cli {
	public static class Util {
		/// <param name="layer">what Unity layer to set the given object, and all child objects, recursive</param>
		public static void SetLayerRecursive(GameObject go, int layer) {
			go.layer = layer;
			for (int i = 0; i < go.transform.childCount; ++i) {
				Transform t = go.transform.GetChild(i);
				if (t != null) {
					SetLayerRecursive(t.gameObject, layer);
				}
			}
		}
		public static string[] singleTagsTMP = { "br", "page" };
		public static string[] tagsTMP = { "align", "alpha", "b", "br", "color", "cspace", "font", "i", "indent", "line-height", "line-indent", "link", "lowercase", "margin", "mark", "mspace", "noparse", "nobr", "page", "pos", "size", "space", "sprite", "s", "smallcaps", "style", "sub", "sup", "u", "uppercase", "voffset", "width" };
		public static string[] tagsTMPallowed = { "alpha", "b", "br", "color", "font", "i", "link", "lowercase", "mark", "noparse", "nobr", "page", "sprite", "s", "style", "u", "uppercase" };

		//public static string ColorToHexCode(Color c) {
		//	int r = (int)(255 * c.r), g = (int)(255 * c.g), b = (int)(255 * c.b), a = (int)(255 * c.a);
		//	return r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + ((c.a != 1) ? a.ToString("X2") : "");
		//}
		public static readonly char[] QUOTES = { '\'', '\"', '`' }, WHITESPACE = { ' ', '\t', '\n', '\b', '\r' };
		public static readonly char[] BRACE_OPEN = { '(', '[', '{' };
		public static readonly char[] BRACE_CLOSE = { ')', ']', '}' };

		/// <returns>index of the end of the token that starts at the given index 'i'</returns>
		public static int FindEndArgumentToken(string str, int i) {
			bool isWhitespace;
			int startToken = i;
			List<char> startMark = null, endMark = null;
			// ignore whitespace before tokens
			do {
				isWhitespace = System.Array.IndexOf(WHITESPACE, str[i]) >= 0;
				if (isWhitespace) { ++i; }
			} while (isWhitespace && i < str.Length);
			while (i < str.Length) {
				bool closingToken = endMark != null && endMark.Count > 0 && str[i] == endMark[endMark.Count - 1];
				if (closingToken) {
					//Debug.Log("closing " + startMark[startMark.Count - 1] + " with " + endMark[endMark.Count - 1] + " at " + i + " in " + str.Substring(startToken, i + 1 - startToken));
					startMark.RemoveAt(startMark.Count - 1); endMark.RemoveAt(endMark.Count - 1);
					if (startMark.Count == 0) { i++; break; } // once all tokens are complete, stop. that's all we need.
					else { ++i; continue; }
				}
				int markFound = System.Array.IndexOf(QUOTES, str[i]);
				if (markFound >= 0) {
					if (startMark == null) { startMark = new List<char>(); endMark = new List<char>(); }
					startMark.Add(QUOTES[markFound]); endMark.Add(QUOTES[markFound]);
				} else {
					markFound = System.Array.IndexOf(BRACE_OPEN, str[i]);
					if (markFound >= 0) {
						if (startMark == null) { startMark = new List<char>(); endMark = new List<char>(); }
						startMark.Add(BRACE_OPEN[markFound]); endMark.Add(BRACE_CLOSE[markFound]);
					}
				}
				if (markFound >= 0) { ++i; continue; }
				if (str[i] == '\\') {
					i++; // skip the next character for an escape sequence. just leave it there.
				} else if(endMark == null || endMark.Count == 0) {
					isWhitespace = System.Array.IndexOf(WHITESPACE, str[i]) >= 0;
					if (isWhitespace) { break; }
				}
				i++;
			}
			if (i >= str.Length) { i = str.Length; }
			if (endMark != null && endMark.Count > 0) {
				Debug.LogError("uh oh... didn't find closing tokens: " +string.Join(", ", endMark)+" token:"+str.Substring(startToken, i - startToken));
			}
			return i;
		}
		/// <returns>split command-line arguments</returns>
		public static List<string> ParseArguments(string commandLineInput)
		{
			int index = 0;
			List<string> tokens = new List<string>();
			while (index < commandLineInput.Length)
			{
				int end = FindEndArgumentToken(commandLineInput, index);
				if (index != end)
				{
					string token = commandLineInput.Substring(index, end - index).TrimStart(WHITESPACE);
					token = Unescape(token);
					int qi = System.Array.IndexOf(QUOTES, token[0]);
					if (qi >= 0 && token[token.Length - 1] == QUOTES[qi])
					{
						token = token.Substring(1, token.Length - 2);
					}
					tokens.Add(token);
				}
				index = end;
			}
			return tokens;
		}
		/* https://msdn.microsoft.com/en-us/library/aa691087(v=vs.71).aspx */
		private static readonly SortedDictionary<char, char> EscapeMap = new SortedDictionary<char, char> {
		{ '0','\0' }, { 'a','\a' }, { 'b','\b' }, { 'f','\f' }, { 'n','\n' }, { 'r','\r' }, { 't','\t' }, { 'v','\v' } };
		/// <summary>convenience method to un-escape standard escape sequence strings</summary>
		/// <param name="escaped">Escaped.</param>
		public static string Unescape(string escaped)
		{
			if (escaped == null) { return escaped; }
			StringBuilder sb = new StringBuilder();
			bool inEscape = false;
			int startIndex = 0;
			for (int i = 0; i < escaped.Length; i++)
			{
				if (!inEscape)
				{
					inEscape = escaped[i] == '\\';
				} else
				{
					char c;
					if (!EscapeMap.TryGetValue(escaped[i], out c))
					{
						c = escaped[i]; // unknown escape sequences are literals
					}
					sb.Append(escaped.Substring(startIndex, i - startIndex - 1));
					sb.Append(c);
					startIndex = i + 1;
					inEscape = false;
				}
			}
			sb.Append(escaped.Substring(startIndex));
			return sb.ToString();
		}
	}
}