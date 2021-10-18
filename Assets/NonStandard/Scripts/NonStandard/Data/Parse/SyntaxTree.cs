
using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NonStandard.Data.Parse {
	/// <summary>
	/// a node in a NonStadard syntax tree
	/// </summary>
	public class SyntaxTree {
		public ParseRuleSet rules = null;
		protected SyntaxTree parent = null;
		public List<Token> tokens;
		/// <summary>
		/// where in the list of tokens this container pulls its children from. might be non-zero it the token list is borrowed from elsewhere
		/// </summary>
		internal int tokenStart;
		/// <summary>
		/// how many tokens from the token list this container is claiming. they will be contiguous following <see cref="tokenStart"/>
		/// </summary>
		internal int tokenCount = -1;
		public SyntaxTree(List<Token> tokenList, int indexStart, int count) {
			tokens = tokenList; tokenStart = indexStart; tokenCount = count;
		}
		public SyntaxTree(ParseRuleSet rule, List<Token> tokenList, int indexStart, int count, object meta) {
			rules = rule; tokens = tokenList; tokenStart = indexStart; tokenCount = count; sourceMeta = meta;
		}
		public Token GetToken(int index) { return tokens[tokenStart + index]; }
		public Delim beginDelim, endDelim;
		/// <summary>
		/// what this <see cref="SyntaxTree"/> is describing
		/// </summary>
		public object sourceMeta;
		public int TokenCount => tokenCount;
		public int Depth { get { SyntaxTree p = parent; int n = 0; while (p != null) { p = p.parent; ++n; } return n; } }
		public readonly static SyntaxTree None = new SyntaxTree(null, 0, -1);
		public SyntaxTree GetParent() { return parent; }
		public void SetParent(SyntaxTree p) { parent = p; }
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
			for (int i = 0; i < tokens.Count; ++i) {
				Token t = tokens[i];
				SyntaxTree e = t.GetAsSyntaxNode();
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
				SyntaxTree e = this; string str;
				Delim d;
				do {
					str = e.sourceMeta as string;
					if (str == null) { d = e.sourceMeta as Delim; if (d != null) { str = d.text; } }
					if (str == null) { e = e.sourceMeta as SyntaxTree; }
				} while (str == null && e != null);
				return (str != null) ? str : null;
			}
		}
		public string GetText() { return Unescape(); }
		public object Resolve(ITokenErrLog tok, object scope, ResolvedEnoughDelegate isItResolvedEnough = null) {
			DelimOp op = sourceMeta as DelimOp;
			if (op != null) {
				return op.resolve.Invoke(tok, this, scope, isItResolvedEnough);
			}
			List<object> finalTerms = ResolveTerms(tok, scope, tokens, isItResolvedEnough);
			object result = finalTerms;
			if (rules != null && rules.Simplify != null) {
				if (isItResolvedEnough != null && isItResolvedEnough.Invoke(result)) { return result; }
				result = rules.Simplify.Invoke(finalTerms);
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
				//string funcName = GetMethodCall(result, i, tokens, found);
				//if (funcName != null && TryExecuteFunction(scope, funcName, tokens[found[++i]], out object funcResult, tok, isItResolvedEnough)) {
				//	results[results.Count - 1] = funcResult;
				//}
				// if this token is a method call, or there are arguments immediately after this token
				CodeRules.MethodCall mc = result as CodeRules.MethodCall;
				if (mc != null || (i < found.Count - 1 && found[i + 1] == found[i] + 1)) {
					object target = mc != null ? mc.target : scope;
					object methodName = mc != null ? mc.methodName : result;
					Token methodArgs = tokens[found[i + 1]];
					if (TryExecuteFunction(target, methodName, methodArgs, out result, tok, isItResolvedEnough)) {
						++i;
						results[results.Count - 1] = result;
					}
				}
			}
		}
		public static bool TryExecuteFunction(object scope, object resolvedFunctionIdentifier, Token arguments, out object funcResult, ITokenErrLog errLog, ResolvedEnoughDelegate isItResolvedEnough = null) {
			string funcName = GetMethodCall(resolvedFunctionIdentifier, arguments);
			if (funcName != null && TryExecuteFunction(scope, funcName, arguments, out funcResult, errLog, isItResolvedEnough)) {
				return true;
			}
			funcResult = null;
			return false;
		}
		public static void FindTerms(List<Token> tokens, int start, int length, List<int> found) {
			for (int i = 0; i < length; ++i) {
				Token t = tokens[start + i];
				if (t.IsSyntaxBoundary) { continue; } // skip entry tokens (count sub-syntax trees)
				found.Add(i);
			}
		}
		private static string GetMethodCall(object result, Token args) { //int i, List<Token> tokens, List<int> found) {
			if (result is string funcName) {
				SyntaxTree e = args.GetAsSyntaxNode();
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
			SyntaxTree beforeParse = argsToken.GetAsSyntaxNode();
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
				if ((a = TryConvertArgs(args, finalArgs, pi, tok, argsToken)) != args.Count
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
		public bool IsTextLiteral() { return rules == CodeRules.String || rules == CodeRules.Char; }
		public bool IsEnclosure { get { return rules == CodeRules.Expression || rules == CodeRules.CodeBody || rules == CodeRules.SquareBrace; } }
		public bool IsComment() { return rules == CodeRules.CommentLine || rules == CodeRules.XmlCommentLine || rules == CodeRules.CommentBlock; }
		public Token GetBeginToken() { return tokens[tokenStart]; }
		public Token GetEndToken() { return tokens[tokenStart + tokenCount - 1]; }
		public int GetIndexBegin() { return GetBeginToken().GetBeginIndex(); }
		public int GetIndexEnd() { return GetEndToken().GetEndIndex(); }
		public bool IsBegin(Token t) { return t == GetBeginToken(); }
		public bool IsEnd(Token t) { return t == GetEndToken(); }
		public bool IsBeginOrEnd(Token t) { return t == GetBeginToken() || t == GetEndToken(); }
		public int Length { get { return GetIndexEnd() - GetIndexBegin(); } }
		public string Unescape() {
			if (rules != CodeRules.String && rules != CodeRules.Char) { return TextRaw.Substring(GetIndexBegin(), Length); }
			StringBuilder sb = new StringBuilder();
			for (int i = tokenStart + 1; i < tokenStart + tokenCount - 1; ++i) {
				sb.Append(tokens[i].ToString());
			}
			return sb.ToString();
		}
		public void RemoveTokenRange(int index, int count) {
			if (count <= 0) return;
			List<Token> tok = tokens as List<Token>;
			if (tok != null) { tok.RemoveRange(index, count); } else {
				Token[] tArr = new Token[tokens.Count - count];
				int end = index + count, length = tokens.Count - end;
				for (int i = 0; i < index; ++i) { tArr[i] = tokens[i]; }
				for (int i = 0; i < length; ++i) { tArr[index + i] = tokens[end + i]; }
				tokens = new List<Token>(tArr);
			}
			tokenCount -= count;
		}
	}
}