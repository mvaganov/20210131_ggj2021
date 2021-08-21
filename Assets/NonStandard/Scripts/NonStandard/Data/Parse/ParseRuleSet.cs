using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data.Parse {
	public class ParseRuleSet {
		protected static Dictionary<string, ParseRuleSet> allContexts = new Dictionary<string, ParseRuleSet>();
		public string name = "default";
		protected char[] whitespace;
		protected Delim[] delimiters;
		private List<ParseRuleSet> delimiterFallback = new List<ParseRuleSet>();

		//public static Dictionary<string, ParseRuleSet> GetAllContexts() { return allContexts; }
		public static ParseRuleSet GetContext(string name) { allContexts.TryGetValue(name, out ParseRuleSet value); return value; }
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
			if (defaultDelimiters != null && !defaultDelimiters.IsSorted()) { Array.Sort(defaultDelimiters); }
			if (defaultWhitespace != null && !defaultWhitespace.IsSorted()) { Array.Sort(defaultWhitespace); }
			Delimiters = (defaultDelimiters == null) ? CodeRules.StandardDelimiters : defaultDelimiters;
			Whitespace = (defaultWhitespace == null) ? CodeRules.StandardWhitespace : defaultWhitespace;
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
			Entry e = new Entry { parseRules = this, tokens = tokens, tokenStart = startTokenIndex, sourceMeta = meta };
			e.SetParent(parent);
			return e;
		}
		public class Entry {
			public ParseRuleSet parseRules = null;
			protected Entry parent = null;
			public List<Token> tokens;
			/// <summary>
			/// where in the list of tokens this container pulls its children from. might be non-zero it the token list is borrowed from elsewhere
			/// </summary>
			public int tokenStart;
			/// <summary>
			/// how many tokens from the token list this container is claiming. they will be contiguous following <see cref="tokenStart"/>
			/// </summary>
			public int tokenCount = -1;
			public Delim beginDelim, endDelim;
			public object sourceMeta;
			public int depth { get { Entry p = parent; int n = 0; while (p != null) { p = p.parent; ++n; } return n; } }
			public readonly static Entry None = new Entry();
			public Entry GetParent() { return parent; }
			public void SetParent(Entry p) { parent = p; }
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
			public string TextEnclosed {
				get {
					switch (sourceMeta) {
					case DelimOp d: return TextRaw;
					}
					int start = tokens[tokenStart].index;
					int limit = tokens[tokenStart + tokenCount - 1].GetEndIndex();
					//Show.Log(parseRules.name + " " +sourceMeta.GetType()+" "+start+", "+limit+" "+TextRaw);
					return TextRaw.Substring(start, limit - start);
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
			public object Resolve(TokenErrLog tok, object scope, bool simplify = true, bool fullyResolve = false) {
				DelimOp op = sourceMeta as DelimOp;
				if(op != null) { 
					return op.resolve.Invoke(tok, this, scope);
				}
				if (IsText()) { return Unescape(); }
				return Resolve(tok, scope, tokens, simplify, fullyResolve);
			}
			public static void FindTerms(List<Token> tokens, int start, int length, List<int> found) {
				for(int i = 0; i < length; ++i) {
					Token t = tokens[start + i];
					Entry e = t.GetAsContextEntry();
					if (e != null && t.IsValid) { continue; } // skip entry tokens (count entry sub-lists
					found.Add(i);
				}
			}
			public static List<object> ResolveTerms(TokenErrLog tok, object scope, List<Token> tokens, bool fullyResolve = false) {
				List<object> results = new List<object>();
				ResolveTerms(tok, scope, tokens, 0, tokens.Count, results, fullyResolve);
				return results;
			}
			public static void ResolveTerms(TokenErrLog tok, object scope, List<Token> tokens, int start, int length, List<object> results, bool fullyResolve = false) {
				List<int> found = new List<int>();
				FindTerms(tokens, start, length, found);
				for (int i = 0; i < found.Count; ++i) {
					Token t = tokens[found[i]];
					results.Add(t.Resolve(tok, scope, true, fullyResolve));
				}
			}
			public static object Resolve(TokenErrLog tok, object scope, List<Token> tokens, bool simplify = true, bool fullyResolve = false) {
				List<object> result = ResolveTerms(tok, scope, tokens, fullyResolve);
				if (simplify) { switch (result.Count) { case 0: return null; case 1: return result[0]; } }
				return result;
			}

			//public int FindTerms() { return CountTerms(tokens, tokenStart, tokenCount); }
			public bool IsText() { return parseRules == CodeRules.String || parseRules == CodeRules.Char; }
			public bool IsEnclosure { get { return parseRules == CodeRules.Expression || parseRules == CodeRules.CodeBody || parseRules == CodeRules.SquareBrace; } }
			public bool IsComment() { return parseRules == CodeRules.CommentLine || parseRules == CodeRules.XmlCommentLine || parseRules == CodeRules.CommentBlock; }
			public Token GetBeginToken() { return tokens[tokenStart]; }
			public Token GetEndToken() { return tokens[tokenStart + tokenCount - 1]; }
			public int GetIndexBegin() { return GetBeginToken().GetBeginIndex(); }
			public int GetIndexEnd() { return GetEndToken().GetEndIndex(); }
			public bool IsBegin(Token t) { return t == GetBeginToken(); }
			public bool IsEnd(Token t) { return t == GetEndToken(); }
			public bool IsBeginOrEnd(Token t) { return t == GetBeginToken() || t == GetEndToken(); }
			public int Length { get { return GetIndexEnd() - GetIndexBegin(); } }
			public string Unescape() {
				if (parseRules != CodeRules.String && parseRules != CodeRules.Char) { return TextRaw.Substring(GetIndexBegin(), Length); }
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
