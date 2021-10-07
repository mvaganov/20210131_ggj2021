using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NonStandard.Data.Parse {
	public delegate bool ResolvedEnoughDelegate(object currentStateOfData);
	public class ParseRuleSet {
		protected static Dictionary<string, ParseRuleSet> allContexts = new Dictionary<string, ParseRuleSet>();
		public string name = "default";
		protected char[] whitespace;
		internal Delim[] delimiters;
		private List<ParseRuleSet> delimiterFallback = new List<ParseRuleSet>();
		/// <summary>
		/// an optional function to simplify results
		/// </summary>
		public Func<List<object>, object> Simplify;

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
		private char minDelim = char.MaxValue, maxDelim = char.MinValue; private int[] delimTextLookup;
		public ParseRuleSet(string name, Delim[] defaultDelimiters = null, char[] defaultWhitespace = null) {
			this.name = name;
			allContexts[name] = this;
			if (defaultDelimiters != null && !defaultDelimiters.IsSorted()) { Array.Sort(defaultDelimiters); }
			if (defaultWhitespace != null && !defaultWhitespace.IsSorted()) { Array.Sort(defaultWhitespace); }
			Delimiters = defaultDelimiters;
			Whitespace = defaultWhitespace;
		}

		public Delim[] GetDefaultDelimiters(ParseRuleSet possibleRuleSet) {
			return possibleRuleSet != null ? possibleRuleSet.Delimiters : CodeRules.Default.Delimiters;
		}
		public char[] GetDefaultWhitespace(ParseRuleSet possibleRuleSet) {
			return possibleRuleSet != null ? possibleRuleSet.Whitespace : CodeRules.Default.Whitespace;
		}

		/// <summary>
		/// set the delimiters of this Context, also calculating a simple lookup table
		/// </summary>
		/// <param name="delims"></param>
		public void SetDelimiters(Delim[] delims) {
			if(delims == null || delims.Length == 0) {
				minDelim = maxDelim = (char)0;
				delimTextLookup = new int[] { -1 };
				return;
			}
			char c, last = delims[0].text[0];
			for (int i = 0; i < delims.Length; ++i) {
				c = delims[i].text[0];
				if (c < last) { Array.Sort(delims); SetDelimiters(delims); return; }
				if (c < minDelim) minDelim = c;
				if (c > maxDelim) maxDelim = c;
			}
			delimTextLookup = new int[maxDelim + 1 - minDelim];
			for (int i = 0; i < delimTextLookup.Length; ++i) { delimTextLookup[i] = -1; }
			for (int i = 0; i < delims.Length; ++i) {
				c = delims[i].text[0];
				int lookupIndex = c - minDelim; // where in the delimiters list this character can be found
				if (delimTextLookup[lookupIndex] < 0) { delimTextLookup[lookupIndex] = i; }
			}
		}
		public int IndexOfDelimeterAt(string str, int index) {
			char c = str[index];
			if (c < minDelim || c > maxDelim) return -1;
			int i = delimTextLookup[c - minDelim];
			if (i < 0) return -1;
			while (i < delimiters.Length) {
				if (delimiters[i].text[0] != c) break;
				if (delimiters[i].IsAt(str, index)) return i;
				++i;
			}
			return -1;
		}
		public Delim GetDelimiter(string str) { return GetDelimiterAt(str, 0, -1); }
		public Delim GetDelimiterAt(string str, int index, int currentTokenStartedAt) {
			int i = IndexOfDelimeterAt(str, index);
			Delim delim = (delimiters != null && i >= 0) ? delimiters[i] : null;
			// if this is a non-breaking delimeter...
			if (delim != null && !delim.breaking) { // TODO put this body in a nicely named function
				//Show.Log(delim.text);
				// ...that has been found within a non-delimiter token
				if (currentTokenStartedAt >= 0) {
					delim = null; // nope, not a delimeter
				} else {
					int nextIndex = index + delim.text.Length;
					if (str.Length > nextIndex) {
						bool whitespaceIsNext = IsWhitespace(str[nextIndex]);
						// ...that has a non-breaking delimiter immediately after it
						//Show.Log("checking after " + delim.text + ": " + str.Substring(index + delim.text.Length));
						Delim nextPossibleDelim = GetDelimiterAt(str, nextIndex, index);
						if (!whitespaceIsNext && (nextPossibleDelim == null || !nextPossibleDelim.breaking)) {
							delim = null; // nope, not a delimiter
						}
					}
				}
			}
			// if a delimiter could not be found and there are fall-back delimiter parsers to check
			if (delim == null && delimiterFallback != null && delimiterFallback.Count > 0) {
				for(i = 0; i < delimiterFallback.Count; ++i) {
					Delim d = delimiterFallback[i].GetDelimiterAt(str, index, currentTokenStartedAt);
					if (d != null) {
						return d;
					}
				}
			}
			return delim;
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
			public object Resolve(ITokenErrLog tok, object scope, ResolvedEnoughDelegate isItResolvedEnough = null) {
				DelimOp op = sourceMeta as DelimOp;
				if(op != null) { 
					return op.resolve.Invoke(tok, this, scope, isItResolvedEnough);
				}
				List<object> finalTerms = ResolveTerms(tok, scope, tokens, isItResolvedEnough);
				object result = finalTerms;
				if(parseRules != null && parseRules.Simplify != null) {
					if (isItResolvedEnough != null && isItResolvedEnough.Invoke(result)) { return result; }
					result = parseRules.Simplify.Invoke(finalTerms);
				}
				return result;
			}
			public static List<object> ResolveTerms(ITokenErrLog tok, object scope, List<Token> tokens, ResolvedEnoughDelegate isItResolvedEnough = null) {
				List<object> results = new List<object>();
				ResolveTerms(tok, scope, tokens, 0, tokens.Count, results, isItResolvedEnough);
				return results;
			}
			public static void ResolveTerms(ITokenErrLog tok, object scope, List<Token> tokens, int start, int length, List<object> results, ResolvedEnoughDelegate isItResolvedEnough = null) {
				List<int> found = new List<int>();
				FindTerms(tokens, start, length, found);
				for (int i = 0; i < found.Count; ++i) {
					Token t = tokens[found[i]];
					object result = t.Resolve(tok, scope, isItResolvedEnough);
					results.Add(result);
					// if this token resolves to a string, and the immediate next one resolves to a list of some kind
					string funcName = GetMethodCall(result, i, tokens, found);
					if (funcName != null && TryExecuteFunction(scope, funcName, tokens[found[++i]], out object funcResult, tok, isItResolvedEnough)) {
						results[results.Count - 1] = funcResult;
					}
				}
			}
			public static void FindTerms(List<Token> tokens, int start, int length, List<int> found) {
				for(int i = 0; i < length; ++i) {
					Token t = tokens[start + i];
					Entry e = t.GetAsContextEntry();
					if (e != null && t.IsValid) { continue; } // skip entry tokens (count entry sub-lists
					found.Add(i);
				}
			}
			private static string GetMethodCall(object result, int i, List<Token> tokens, List<int> found) {
				if (result is string funcName && i < found.Count - 1 && found[i] + 1 == found[i + 1]) {
					Token argsToken = tokens[found[i + 1]];
					Entry e = argsToken.GetAsContextEntry();
					if (e != null && e.IsEnclosure) {
						return funcName;
					}
				}
				return null;
			}
			private static bool TryExecuteFunction(object scope, string funcName, Token argsToken, out object result, ITokenErrLog tok, ResolvedEnoughDelegate isItResolvedEnough) {
				result = null;
				if (!DeterminePossibleMethods(scope, funcName, out List<MethodInfo> possibleMethods, tok, argsToken)) { return false; }
				List<object> args = ResolveFunctionArgumentList(argsToken, scope, tok, isItResolvedEnough);
				if (!DetermineValidMethods(funcName, argsToken, possibleMethods, out List<ParameterInfo[]> validParams, args, tok)) { return false; }
				if (!DetermineMethod(args, possibleMethods, validParams, out MethodInfo mi, out object[] finalArgs, tok, argsToken)) { return false; }
				return ExecuteMethod(scope, mi, finalArgs, out result, tok, argsToken);
			}
			private static bool DeterminePossibleMethods(object scope, string funcName, out List<MethodInfo> possibleMethods, ITokenErrLog tok, Token argsToken) {
				if (scope == null) {
					tok.AddError(argsToken, $"can't execute function \'{funcName}\' without scope");
					possibleMethods = null;
					return false;
				}
				possibleMethods = scope.GetType().GetMethods().FindAll(m => m.Name == funcName);
				if (possibleMethods.Count == 0) {
					tok.AddError(argsToken, $"missing function \'{funcName}\' in {scope.GetType()}");
					return false;
				}
				return true;
			}
			private static List<object> ResolveFunctionArgumentList(Token argsToken, object scope, ITokenErrLog tok, ResolvedEnoughDelegate isItResolvedEnough) {
				object argsRaw = argsToken.Resolve(tok, scope, isItResolvedEnough);
				if (argsRaw == null) { argsRaw = new List<object>(); }
				List<object> args = argsRaw as List<object>;
				if (args == null) {
					args = new List<object> { argsRaw };
				}
				// remove commas if they are comma tokens before and after being parsed
				Entry beforeParse = argsToken.GetAsContextEntry();
				for (int i = args.Count - 1; i >= 0; --i) {
					if ((args[i] as string) == "," && beforeParse.tokens[i + 1].StringifySmall() == ",") { args.RemoveAt(i); }
				}
				return args;
			}
			private static bool DetermineValidMethods(string funcName, Token argsToken, List<MethodInfo> possibleMethods, out List<ParameterInfo[]> validParams, IList<object> args, ITokenErrLog tok) {
				ParameterInfo[] pi;
				validParams = new List<ParameterInfo[]>();
				List<ParameterInfo[]> invalidParams = new List<ParameterInfo[]>();
				for (int i = possibleMethods.Count - 1; i >= 0; --i) {
					pi = possibleMethods[i].GetParameters();
					if (pi.Length != args.Count) {
						possibleMethods.RemoveAt(i);
						invalidParams.Add(pi);
						continue;
					}
					validParams.Add(pi);
				}
				// check arguments. start with the argument count
				if (possibleMethods.Count == 0) {
					tok.AddError(argsToken, $"'{funcName}' needs {invalidParams.JoinToString(" or ", par => par.Length.ToString())} arguments, not {args.Count} from {args.StringifySmall()}");
					return false;
				}
				return true;
			}
			private static bool DetermineMethod(List<object> args, List<MethodInfo> possibleMethods, List<ParameterInfo[]> validParams, out MethodInfo mi, out object[] finalArgs, ITokenErrLog tok, Token argsToken) {
				mi = null;
				finalArgs = new object[args.Count];
				for (int paramSet = 0; paramSet < validParams.Count; ++paramSet) {
					bool typesOk = true;
					ParameterInfo[] pi = validParams[paramSet];
					int a;
					if((a = TryConvertArgs(args, finalArgs, pi, tok, argsToken)) != args.Count
					// it's only a problem if there are no other options
					&& paramSet == validParams.Count - 1) {
						tok.AddError(argsToken, $"can't convert \'{args[a]}\' to {pi[a].ParameterType} for {possibleMethods[paramSet].Name}{argsToken.Stringify()}");
					}
					if (typesOk) {
						mi = possibleMethods[paramSet];
						return true;
					}
				}
				return false;
			}
			private static int TryConvertArgs(IList<object> args, IList<object> finalArgs, ParameterInfo[] pi, ITokenErrLog tok, Token argsToken) {
				for (int i = 0; i < args.Count; ++i) {
					try {
						finalArgs[i] = Convert.ChangeType(args[i], pi[i].ParameterType);
					} catch (Exception) {
						return i;
					}
				}
				return args.Count;
			}
			private static bool ExecuteMethod(object scope, MethodInfo mi, object[] finalArgs, out object result, ITokenErrLog tok, Token argsToken) {
				try {
					result = mi.Invoke(scope, finalArgs);
				} catch (Exception e) {
					result = null;
					tok.AddError(argsToken, e.ToString());
					return false;
				}
				return true;
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
