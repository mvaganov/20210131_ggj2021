using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data.Parse {
	public class ParseRuleSet {
		public static Dictionary<string, ParseRuleSet> allContexts = new Dictionary<string, ParseRuleSet>();
		public string name = "default";
		protected char[] whitespace;
		protected Delim[] delimiters;
		private List<ParseRuleSet> delimiterFallback = new List<ParseRuleSet>();

		public char[] Whitespace {
			get => whitespace;
			set {
				whitespace = value;
				if(whitespace == null || whitespace.Length == 0) {
					minWhitespace = maxWhitespace = (char)0;
				} else {
					minWhitespace = whitespace.Min();
					maxWhitespace = whitespace.Max();
				}
			}
		}
		public Delim[] Delimiters {
			get => delimiters;
			set {
				delimiters = value;
				SetDelimiters(delimiters);
			}
		}
		private char minWhitespace = char.MinValue, maxWhitespace = char.MaxValue;
		public bool IsWhitespace(char c) {
			return (c < minWhitespace || c > maxWhitespace) ? false : whitespace.IndexOf(c) >= 0;
		}
		/// <summary>
		/// data used to make delimiter searching very fast
		/// </summary>
		private char minDelim = char.MaxValue, maxDelim = char.MinValue; private int[] textLookup;
		public ParseRuleSet(string name, Delim[] defaultDelimiters = null, char[] defaultWhitespace = null) {
			this.name = name;
			allContexts[name] = this;
			Delimiters = (defaultDelimiters == null) ? CodeRules.StandardDelimiters : defaultDelimiters;
			Whitespace = (defaultWhitespace == null) ? CodeRules.WhitespaceDefault : defaultWhitespace;
		}
		/// <summary>
		/// set the delimiters of this Context, also calculating a simple lookup table
		/// </summary>
		/// <param name="delims"></param>
		public void SetDelimiters(Delim[] delims) {
			if(delims == null || delims.Length == 0) {
				minDelim = maxDelim = (char)0;
				textLookup = new int[] { -1 };
				return;
			}
			char c, last = delims[0].text[0];
			for (int i = 0; i < delims.Length; ++i) {
				c = delims[i].text[0];
				if (c < last) { Array.Sort(delims); SetDelimiters(delims); return; }
				if (c < minDelim) minDelim = c;
				if (c > maxDelim) maxDelim = c;
			}
			textLookup = new int[maxDelim + 1 - minDelim];
			for (int i = 0; i < textLookup.Length; ++i) { textLookup[i] = -1; }
			for (int i = 0; i < delims.Length; ++i) {
				c = delims[i].text[0];
				int lookupIndex = c - minDelim; // where in the delimiters list this character can be found
				if (textLookup[lookupIndex] < 0) { textLookup[lookupIndex] = i; }
			}
		}
		public int IndexOfDelimeterAt(string str, int index) {
			char c = str[index];
			if (c < minDelim || c > maxDelim) return -1;
			int i = textLookup[c - minDelim];
			if (i < 0) return -1;
			while (i < delimiters.Length) {
				if (delimiters[i].text[0] != c) break;
				if (delimiters[i].IsAt(str, index)) return i;
				++i;
			}
			return -1;
		}
		public Delim GetDelimiterAt(string str, int index) {
			int i = IndexOfDelimeterAt(str, index);
			if (i < 0) {
				if (delimiterFallback != null) {
					for(i = 0; i < delimiterFallback.Count; ++i) {
						Delim d = delimiterFallback[i].GetDelimiterAt(str, index);
						if (d != null) {
							return d;
						}
					}
				}
				return null;
			}
			return delimiters[i];
		}
		public void AddDelimiterFallback(ParseRuleSet ruleSet) {
			delimiterFallback.Add(ruleSet);
			List<ParseRuleSet> stack = new List<ParseRuleSet>();
			if (RecursionFound(stack)) {
				delimiterFallback.Remove(ruleSet);
				throw new Exception("can't add " + ruleSet.name + " as fallback to " + name + ", recursion: " +
					stack.JoinToString("->", rs => rs.name));
			}
		}
		private bool RecursionFound(List<ParseRuleSet> stack = null) {
			if (stack == null) { stack = new List<ParseRuleSet>(); }
			if (stack.Contains(this)) { return true; }
			if (delimiterFallback != null && delimiterFallback.Count > 0) {
				stack.Add(this);
				for (int i = 0; i < delimiterFallback.Count; ++i) {
					if (delimiterFallback[i].RecursionFound(stack)) { return true; }
				}
				stack.Remove(this);
			}
			return false;
		}
		public Entry GetEntry(List<Token> tokens, int startTokenIndex, object meta, ParseRuleSet.Entry parent = null) {
			Entry e = new Entry { context = this, tokens = tokens, tokenStart = startTokenIndex, sourceMeta = meta, parent = parent };
			return e;
		}
		public class Entry {
			public ParseRuleSet context = null;
			public Entry parent = null;
			public List<Token> tokens;
			public int tokenStart, tokenCount = -1;
			public Delim beginDelim, endDelim;
			public object sourceMeta;
			public int depth { get { Entry p = parent; int n = 0; while (p != null) { p = p.parent; ++n; } return n; } }
			public readonly static Entry None = new Entry();
			public static string PrintAll(List<Token> tokens) {
				StringBuilder sb = new StringBuilder();
				List<List<Token>> stack = new List<List<Token>>();
				PrintAll(tokens, sb, stack);
				return sb.ToString();
			}
			protected static void PrintAll(List<Token> tokens, StringBuilder sb, List<List<Token>> stack) {
				int recurse = stack.IndexOf(tokens);
				stack.Add(tokens);
				if (recurse >= 0) { sb.Append("/* recurse " + stack.Count + " */"); return; }
				for(int i = 0; i < tokens.Count; ++i) {
					Token t = tokens[i];
					Entry e = t.GetAsContextEntry();
					if (e != null && !t.IsValid) {
						PrintAll(e.tokens, sb, stack);
					} else {
						sb.Append(t);
					}
				}
			}
			public string TextRaw { 
				get {
					Entry e = this; string str;
					Delim d;
					do {
						str = e.sourceMeta as string;
						if (str == null) { d = e.sourceMeta as Delim; if (d != null) { str = d.text; } }
						if (str == null) { e = e.sourceMeta as Entry; }
					} while (str == null && e != null);
					return (str != null) ? str : null;
				}
			}
			public string GetText() { return Unescape(); }
			public object Resolve(Tokenizer tok, object scope, bool simplify = true) {
				DelimOp op = sourceMeta as DelimOp;
				if(op != null) { 
					return op.resolve.Invoke(tok, this, scope);
				}
				if (IsText()) { return Unescape(); }
				return Resolve(tok, scope, tokens, simplify);
			}
			public static void FindTerms(List<Token> tokens, int start, int length, List<int> found) {
				for(int i = 0; i < length; ++i) {
					Token t = tokens[start + i];
					Entry e = t.GetAsContextEntry();
					if (e != null && t.IsValid) { continue; } // skip entry tokens (count entry sub-lists
					found.Add(i);
				}
			}
			public static List<object> ResolveTerms(Tokenizer tok, object scope, List<Token> tokens) {
				List<object> results = new List<object>();
				ResolveTerms(tok, scope, tokens, 0, tokens.Count, results);
				return results;
			}
			public static void ResolveTerms(Tokenizer tok, object scope, List<Token> tokens, int start, int length, List<object> results) {
				List<int> found = new List<int>();
				FindTerms(tokens, start, length, found);
				for (int i = 0; i < found.Count; ++i) {
					Token t = tokens[found[i]];
					results.Add(t.Resolve(tok, scope));
				}
			}
			public static object Resolve(Tokenizer tok, object scope, List<Token> tokens, bool simplify = true) {
				List<object> result = ResolveTerms(tok, scope, tokens);
				if (simplify) { switch (result.Count) { case 0: return null; case 1: return result[0]; } }
				return result;
			}

			//public int FindTerms() { return CountTerms(tokens, tokenStart, tokenCount); }
			public bool IsText() { return context == CodeRules.String || context == CodeRules.Char; }
			public bool IsEnclosure { get { return context == CodeRules.Expression || context == CodeRules.CodeBody || context == CodeRules.SquareBrace; } }
			public bool IsComment() { return context == CodeRules.CommentLine || context == CodeRules.XmlCommentLine || context == CodeRules.CommentBlock; }
			public Token GetBeginToken() { return tokens[tokenStart]; }
			public Token GetEndToken() { return tokens[tokenStart + tokenCount - 1]; }
			public int GetIndexBegin() { return GetBeginToken().GetBeginIndex(); }
			public int GetIndexEnd() { return GetEndToken().GetEndIndex(); }
			public bool IsBegin(Token t) { return t == GetBeginToken(); }
			public bool IsEnd(Token t) { return t == GetEndToken(); }
			public bool IsBeginOrEnd(Token t) { return t == GetBeginToken() || t == GetEndToken(); }
			public int Length { get { return GetIndexEnd() - GetIndexBegin(); } }
			public string Unescape() {
				if (context != CodeRules.String && context != CodeRules.Char) { return TextRaw.Substring(GetIndexBegin(), Length); }
				StringBuilder sb = new StringBuilder();
				for (int i = tokenStart + 1; i < tokenStart + tokenCount - 1; ++i) {
					sb.Append(tokens[i].ToString());
				}
				return sb.ToString();
			}
			public void RemoveTokenRange(int index, int count) {
				if (count <= 0) return;
				List<Token> tok = tokens as List<Token>;
				if (tok != null) { tok.RemoveRange(index, count); }
				else {
					Token[] tArr = new Token[tokens.Count-count];
					int end = index + count, length = tokens.Count - end;
					for (int i = 0; i < index; ++i) { tArr[i] = tokens[i]; }
					for (int i = 0; i < length; ++i) { tArr[index + i] = tokens[end + i]; }
					tokens = new List<Token>(tArr);
				}
				tokenCount -= count;
			}
		}
	}
}
