﻿//#define __DEBUG_token_recursion
using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data.Parse {
	public interface ITokenErrLog {
		bool HasError();
		void AddError(ParseError error);
		string GetErrorString();
		IList<int> GetTextRows();
		void ClearErrors();
	}
	public class TokenErrorLog : ITokenErrLog {
		public List<ParseError> errors = new List<ParseError>();
		public IList<int> GetTextRows() => rows;
		/// <summary>
		/// the indexes of where rows end (newline characters), in order.
		/// </summary>
		internal List<int> rows = new List<int>();

		public void ClearErrors() { errors.Clear(); }
		public bool HasError() { return errors.Count > 0; }
		public ParseError AddError(string message) { return AddError(-1, message); }
		public ParseError AddError(int index, string message) {
			ParseError e = new ParseError(index, rows, message); errors.Add(e); return e;
		}
		public ParseError AddError(Token token, string message) { return AddError(token.index, message); }
		public void AddError(ParseError error) { errors.Add(error); }
		public string GetErrorString() { return errors.JoinToString("\n"); }
	}
	public static class TokenizationErrorStorageExtension {
		public static void Log(this ITokenErrLog self) { self.ShowErrorTo(NonStandard.Show.Log); }
		public static ParseError AddError(this ITokenErrLog self, int index, string message) {
			ParseError e = new ParseError(index, self.GetTextRows(), message); self.AddError(e); return e;
		}
		public static ParseError AddError(this ITokenErrLog self, Token token, string message) {
			return AddError(self, token.index, message);
		}
		public static bool ShowErrorTo(this ITokenErrLog self, Show.PrintFunc show) {
			string errStr = self.GetErrorString();
			if (string.IsNullOrEmpty(errStr)) return false;
			show.Invoke(errStr); return true;
		}
	}
	public class Tokenizer : TokenErrorLog {
		/// <summary>
		/// text being tokenized
		/// </summary>
		internal string str;
		/// <summary>
		/// a tree of tokens, where each token might have meta data that is another list of tokens
		/// </summary>
		internal List<Token> tokens = new List<Token>();
		/// <summary>
		/// TODO testme! does this produce all of the tokens in string form?
		/// </summary>
		protected List<string> tokenStrings = new List<string>();
		public int TokenCount { get { return tokens.Count; } }
		/// <param name="i"></param>
		/// <returns>raw token data</returns>
		public Token GetToken(int i) { return tokens[i]; }
		/// <summary>
		/// the method you're looking for if the tokens are doing something fancy, like resolving to objects
		/// </summary>
		/// <param name="i"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		public object GetResolvedToken(int i, object scope = null) { return GetToken(i).Resolve(this, scope); }
		/// <summary>
		/// this is probably the method you're looking for. Get a string from the list of tokens, resolving it if it's a variable.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		public string GetStr(int i, object scope = null) {
			if (i >= tokens.Count) return null;
			object o = GetResolvedToken(i, scope);
			return o != null ? o.ToString() : null;
		}
		public Token PopToken() {
			if(tokens.Count > 0) {
				Token r = tokens[0];
				tokens.RemoveAt(0);
				return r;
			}
			return Token.None;
		}
		public List<Token> PopTokens(int count) {
			if (tokens.Count > 0) {
				List<Token> r = tokens.GetRange(0, count);
				tokens.RemoveRange(0, count);
				return r;
			}
			return null;
		}
		public int IndexOf(Delim delimiter) {
			for(int i = 0; i < tokens.Count; ++i) {
				Delim d = tokens[i].GetAsDelimiter();
				if (d == delimiter) { return i; }
			}
			return -1;
		}
		public Tokenizer() { }
		public Tokenizer(Tokenizer toCopy) {
			this.str = toCopy.str;
			this.tokens = toCopy.tokens.Copy();
			this.tokenStrings = toCopy.tokenStrings.Copy();
			this.rows = toCopy.rows.Copy();
		}
		public Token GetMasterToken() { return GetMasterToken(tokens, str); }
		public static Token GetMasterToken(List<Token> tokens, string str) {
			SyntaxTree syntax = new SyntaxTree(null, tokens, 0, tokens.Count, null);
			Token t = new Token(syntax, 0, str.Length);
			return t;
		}
		public void FilePositionOf(Token token, out int row, out int col) {
			ParseError.FilePositionOf(token, rows, out row, out col);
		}
		public string FilePositionOf(Token token) {
			List<SyntaxTree> traversed = new List<SyntaxTree>();
			while (!token.IsValid) {
				SyntaxTree e = token.GetAsSyntaxNode();
				if (e == null || traversed.IndexOf(e) >= 0) return "???";
				traversed.Add(e);
				token = e.tokens[0];
			}
			return ParseError.FilePositionOf(token, rows);
		}
		/// <param name="str"></param>
		/// <param name="parsingRules"></param>
		/// <param name="condition">allows parsing to exit early, if the early part of the string is sufficient for example</param>
		public void Tokenize(string str, ParseRuleSet parsingRules = null, Func<Tokenizer, bool> condition=null) {
			this.str = str;
			errors.Clear();
			tokens.Clear();
			rows.Clear();
			Tokenize(parsingRules, 0, condition);
		}
		public static Tokenizer Tokenize(string str) {
			Tokenizer t = new Tokenizer(); t.Tokenize(str, null); return t;
		}
		/// <param name="str"></param>
		/// <param name="condition">allows parsing to exit early, if the early part of the string is sufficient for example</param>
		/// <returns></returns>
		public static Tokenizer TokenizeWhile(string str, Func<Tokenizer,bool> condition) {
			Tokenizer t = new Tokenizer(); t.Tokenize(str, null, condition); return t;
		}
		public static string FirstWord(string str) {
			Tokenizer tokenizer = TokenizeWhile(str, t => t.tokens.Count == 0);
			return (tokenizer?.tokens.Count > 0) ? tokenizer.GetStr(0) : null;
		}
		public string DebugPrint(int depth = 0, string indent = "  ", string separator = ", ") {
			return DebugPrint(tokens, depth, indent, separator);
		}
		public static string DebugPrint(IList<Token> tokens, int depth = 0, string indent = "  ", string separator = ", ") {
			StringBuilder sb = new StringBuilder();
			DebugPrint(tokens, depth, indent, separator, sb);
			return sb.ToString();
		}
		public override string ToString() {
			return "["+DebugPrint(indent:null)+"]";
		}
		public static void DebugPrint(IList<Token> tokens, int depth = 0, string indent = "  ", string separator = ", ", StringBuilder sb = null, List<Token> path = null) {
			if(path == null) { path = new List<Token>(); }
			for(int i = 0; i < tokens.Count; ++i) {
				Token t = tokens[i];
				//int r;
				//if (t.IsValid && (r = path.FindIndex(to=>to.index==t.index)) >= 0) {
				//	continue;
				//	string message = "/* recurse " + (path.Count - r) + " " +t+" "+t.index+ " */";
				//	throw new Exception(message);
				//	return;
				//}
				//path.Add(t);
				SyntaxTree e = t.GetAsSyntaxNode();
				if (e != null) {
					//if ((r = path.IndexOf(e.tokens)) >= 0) {
					//	string message = "/* recurse " + (path.Count - r) + " */";
					//	sb.Append(message);
					//} else
					if (e.tokens != tokens) {
						SyntaxTree prevEntry = i > 0 ? tokens[i - 1].GetAsSyntaxNode() : null;
						if (indent != null) {
							if (prevEntry != null && prevEntry.tokens != tokens) {
								sb.Append(indent);
							} else {
								sb.Append("\n").Append(StringExtension.Indentation(depth + 1, indent));
							}
						}
						if (separator != null && i > 0) { sb.Append(separator); }
						DebugPrint(e.tokens, depth + 1, indent, separator, sb, path);
						if (indent != null) {
							sb.Append("\n").Append(StringExtension.Indentation(depth, indent));
						}
					} else {
						if (i == 0) {
							if (separator != null && i > 0) { sb.Append(separator); }
							sb.Append("(").Append(e.beginDelim.ToString().Stringify(showBoundary:false)).Append("(");
						}
						else if (i == tokens.Count-1) {
							if (separator != null && i > 0) { sb.Append(separator); }
							sb.Append(")").Append(e.endDelim.ToString().Stringify(showBoundary: false)).Append(")");
						}
						else { sb.Append(" ").Append(e.sourceMeta).Append(" "); }
					}
				} else {
					if (separator != null && i > 0) { sb.Append(separator); }
					sb.Append(tokens[i].GetAsSmallText().StringifySmall());
				}
			}
		}
		/// <param name="parseRules"></param>
		/// <param name="index"></param>
		/// <param name="condition">allows parsing to exit early, if the early part of the string is sufficient for example</param>
		protected void Tokenize(ParseRuleSet parseRules = null, int index = 0, Func<Tokenizer, bool> condition = null) {
			tokenStrings.Clear();
			if (string.IsNullOrEmpty(str)) return;
			List<SyntaxTree> contextStack = new List<SyntaxTree>();
			if (parseRules == null) {
				parseRules = CodeRules.Default;
			}
			else { contextStack.Add(parseRules.GetEntry(tokens, -1, null)); }
			int tokenBegin = -1;
			ParseRuleSet currentContext = parseRules;
			//Show.Log("parsing \""+str+"\" with ["+currentContext.name+"]");
			int lastIndex = index-1;
			while (index < str.Length && (condition == null || condition.Invoke(this))) {
				if (index <= lastIndex) { throw new Exception("tokenize algorithm problem, the index isn't advancing"); }
				char c = str[index];
				WhatsThis(currentContext, index, tokenBegin, parseRules, out Delim delim, out bool isWhiteSpace);
				if (delim != null) {
					FinishToken(index, ref tokenBegin); // finish whatever token was being read before this delimeter
					HandleDelimiter(delim, ref index, contextStack, ref currentContext, parseRules);
				} else if (!isWhiteSpace) {
					if (tokenBegin < 0) { tokenBegin = index; }
				} else {
					FinishToken(index, ref tokenBegin); // handle whitespace
				}
				if (rows != null && c == '\n') { rows.Add(index); }
				++index;
			}
			FinishToken(index, ref tokenBegin); // add the last token that was still being processed
			FinalTokenCleanup();
			//DebugPrint(-1);
			ApplyOperators();
		}
		private void WhatsThis(ParseRuleSet currentContext, int index, int tokenBegin, ParseRuleSet defaultContext, out Delim delim, out bool isWhiteSpace) {
			char c = str[index];
			isWhiteSpace = (currentContext.Whitespace != null)
				? currentContext.IsWhitespace(c) : (defaultContext.Whitespace != null ) 
				? defaultContext.IsWhitespace(c) 
				: CodeRules.Default.IsWhitespace(c);
			if (isWhiteSpace) { delim = null; return; }
			delim = (currentContext.Delimiters != null)
				? currentContext.GetDelimiterAt(str, index, tokenBegin)
				: defaultContext.GetDelimiterAt(str, index, tokenBegin);
		}


		private bool FinishToken(int index, ref int tokenBegin) {
			if (tokenBegin >= 0) {
				int len = index - tokenBegin;
				if (len > 0) {
					tokens.Add(new Token(str, tokenBegin, len));
					tokenStrings.Add(str.Substring(tokenBegin, len));
				}
				tokenBegin = -1;
				return true;
			}
			return false;
		}
		private void HandleDelimiter(Delim delim, ref int index,  List<SyntaxTree> syntaxStack, 
			ref ParseRuleSet currentContext, ParseRuleSet defaultContext) {
			Token delimToken = new Token(delim, index, delim.text.Length);
			if (delim.parseRule != null) {
				ParseResult pr = delim.parseRule.Invoke(str, index);
				if (pr.IsError && errors != null) {
					pr.error.OffsetBy(delimToken.index, rows);
					errors.Add(pr.error);
				}
				if (pr.replacementValue != null) {
					delimToken.length = pr.lengthParsed;
					delimToken.meta = new TokenSubstitution(str, pr.replacementValue);
				}
				index += pr.lengthParsed - 1;
			} else {
				index += delim.text.Length - 1;
			}
			DelimCtx dcx = delim as DelimCtx;
			SyntaxTree endedSyntax = null;
			if (dcx != null) {
				if (syntaxStack.Count > 0 && dcx.Context == currentContext && dcx.isEnd) {
					endedSyntax = syntaxStack[syntaxStack.Count - 1];
					endedSyntax.endDelim = dcx;
					delimToken.meta = endedSyntax;
					endedSyntax.tokenCount = (tokens.Count - endedSyntax.tokenStart) + 1;
					syntaxStack.RemoveAt(syntaxStack.Count - 1);
					if (syntaxStack.Count > 0) {
						currentContext = syntaxStack[syntaxStack.Count - 1].rules;
					} else {
						currentContext = defaultContext;
					}
				}
				if (endedSyntax == null && dcx.isStart) {
					SyntaxTree parentCntx = (syntaxStack.Count > 0) ? syntaxStack[syntaxStack.Count - 1] : null;
					SyntaxTree newContext = dcx.Context.GetEntry(tokens, tokens.Count, str, parentCntx);
					newContext.beginDelim = dcx;
					currentContext = dcx.Context;
					delimToken.meta = newContext;
					syntaxStack.Add(newContext);
				}
			}
			tokens.Add(delimToken);
			tokenStrings.Add(delim.text);
			if (endedSyntax != null) { ExtractContextAsSubTokenList(endedSyntax); }
		}
		private void FinalTokenCleanup() {
			for (int i = 0; i < tokens.Count; ++i) {
				// any unfinished contexts must end. the last place they could end is the end of this string
				SyntaxTree syntax = tokens[i].GetAsSyntaxNode();
				if (syntax != null && syntax.TokenCount < 0) {
					syntax.tokenCount = tokens.Count - syntax.tokenStart;
					ExtractContextAsSubTokenList(syntax);
					if (syntax.rules != CodeRules.CommentLine) { // this is an error, unless it's a comment
						errors.Add(new ParseError(tokens[i], rows, 
							$"missing closing token after {syntax.GetBeginToken().ToString().StringifySmall()}"));
					}
					// close any unfinished contexts inside of this context too!
					tokens = syntax.tokens;
					i = 0;
				}
			}
#if __DEBUG_token_recursion
			List<Token> allTokens = new List<Token>(tokens);
			int travelIndex = 0;
			BreadthFirstSearch(allTokens, ref travelIndex);
#endif
		}

		protected void BreadthFirstSearch(List<Token> travelLog, ref int index) {
			while(index < travelLog.Count) {
				Token iter = travelLog[index];
				SyntaxTree e = iter.GetAsSyntaxNode();
				if (e != null) {
					for (int i = 0; i < e.tokens.Count; ++i) {
						Token token = e.tokens[i];
						int inTheList = token.IsValid ? travelLog.FindIndex(t=>t==token) : -1;
						if (inTheList >= 0 && inTheList < index && travelLog[inTheList].IsValid) {
							throw new Exception("recursion! " + token.index + " " + token);
						}
						if (inTheList < 0 && token.GetAsSyntaxNode() != e) {
							travelLog.Add(token);
						}
					}
				}
				index++;
			}
		}

		protected void ApplyOperators() {
			int SortByOrderOfOperations(TokenPath a, TokenPath b) {
				int comp;
				comp = b.path.Length.CompareTo(a.path.Length);
				if (comp != 0) { return comp; }
				//Context.Entry e = null;
				Token ta = a.token;//GetTokenAt(tokens, a, ref e);
				Token tb = b.token;//GetTokenAt(tokens, b, ref e);
				DelimOp da = ta.meta as DelimOp;
				DelimOp db = tb.meta as DelimOp;
				comp = da.order.CompareTo(db.order);
				// do the last one in the line first, so future indexes don't get offset as the tokens are combined
				if (comp == 0) { comp = ta.index.CompareTo(tb.index); }
				return comp;
			}
			List<TokenPath> paths = FindTokenPaths(t => t.meta is DelimOp);
			paths.Sort(SortByOrderOfOperations);
			List<int> finishedTokens = new List<int>();
			bool operatorWasLostInTheShuffle;
			do {
				operatorWasLostInTheShuffle = false;
				for (int i = 0; i < paths.Count; ++i) {
					SyntaxTree pathNode = null;
					Token t = GetTokenAt(tokens, paths[i].path, ref pathNode);
					if (t == Token.None) {
						t = pathNode?.GetBeginToken() ?? this.tokens[0];
						AddError(t, "can't resolve "+t.GetAsSmallText()+", missing token");
						return;
					}
					if (paths[i].token.index != t.index) {
						operatorWasLostInTheShuffle = true;
						continue;// break instead?
					}
					DelimOp op = t.meta as DelimOp;
					//if(op == null || pathNode == null || paths == null || paths[i].path == null) {
					//	Show.Log("oof");
					//}
					List<Token> listWhereOpWasFound = pathNode != null ? pathNode.tokens : tokens;
					//Context.Entry opEntry = 
						op.isSyntaxValid.Invoke(this, listWhereOpWasFound, paths[i].path[paths[i].path.Length - 1]);
					if (pathNode != null && pathNode.TokenCount != pathNode.tokens.Count) {
						pathNode.tokenCount = pathNode.tokens.Count;
					}
					finishedTokens.Add(t.index);
				}
				if (operatorWasLostInTheShuffle) {
					paths = FindTokenPaths(t => t.meta is DelimOp && finishedTokens.IndexOf(t.index) < 0);
					paths.Sort(SortByOrderOfOperations);
				}
			} while (operatorWasLostInTheShuffle);
		}
		protected string PrintTokenPaths(IList<int[]> paths) {
			return paths.JoinToString("\n", arr => {
				SyntaxTree e = null;
				Token t = GetTokenAt(tokens, arr, ref e);
				return arr.JoinToString(", ") + ":" + t + " @" + ParseError.FilePositionOf(t, rows);
			});
		}
		Token GetTokenAt(List<Token> currentPath, IList<int> index, ref SyntaxTree lastPathNode) {
			int i = index[0];
			if (i < 0 || i >= currentPath.Count) { return Token.None; }
			Token t = currentPath[i];
			if (index.Count == 1) return t;
			index = index.GetRange(1, index.Count - 1);
			lastPathNode = t.GetAsSyntaxNode();
			return GetTokenAt(lastPathNode.tokens, index, ref lastPathNode);
		}
		struct TokenPath {
			public int[] path; public Token token; public SyntaxTree pathNode;
		}
		List<TokenPath> FindTokenPaths(Func<Token, bool> predicate, bool justOne = false) {
			if (tokens.Count == 0) return new List<TokenPath>();
			List<List<Token>> path = new List<List<Token>>();
			List<int> position = new List<int>();
			List<TokenPath> paths = new List<TokenPath>();
			path.Add(tokens);
			position.Add(0);
			SyntaxTree e = null;
			while (position[position.Count-1] < path[path.Count - 1].Count) {
				List<Token> currentTokens = path[path.Count - 1];
				int currentIndex = position[position.Count - 1];
				Token token = currentTokens[currentIndex];
				if (predicate(token)) { paths.Add(new TokenPath { path = position.ToArray(), token = token, pathNode = e }); }
				e = token.GetAsSyntaxNode();
				bool incremented = false;
				if(e != null) {
					if (currentTokens != e.tokens) {
						position.Add(0);
						path.Add(e.tokens);
						if (justOne) break;
						currentIndex = position[position.Count - 1];
						currentTokens = path[path.Count - 1];
						incremented = true;
					}
				}
				if (!incremented) {
					do {
						position[position.Count - 1] = ++currentIndex;
						if (currentIndex >= currentTokens.Count) {
							position.RemoveAt(position.Count - 1);
							path.RemoveAt(path.Count - 1);
							if (position.Count <= 0) break;
							currentIndex = position[position.Count - 1];
							position[position.Count - 1] = ++currentIndex;
							currentTokens = path[path.Count - 1];
						}
					} while (currentIndex >= currentTokens.Count);
				}
				if (position.Count <= 0) break;
			}
			return paths;
		}
		internal void ExtractContextAsSubTokenList(SyntaxTree entry) {
			if(entry.TokenCount <= 0) { throw new Exception("what just happened?"); }
			int indexWhereItHappens = entry.tokenStart;
			int limit = entry.tokenStart + entry.tokenCount;
			if (entry.tokenStart < 0 || entry.tokenCount < 0 || limit > entry.tokens.Count) {
				Show.Log("oh no! ");
			}
			List<Token> subTokens = null;
			try {
				subTokens = entry.tokens.GetRange(entry.tokenStart, entry.tokenCount);
			} catch {
				Show.Log("oh no! !!!!");
			}
			int index = subTokens.FindIndex(t => t.GetAsSyntaxNode() == entry);
			entry.RemoveTokenRange(entry.tokenStart, entry.tokenCount - 1);
			Token entryToken = subTokens[index];
			entryToken.Invalidate();
			entry.tokens[indexWhereItHappens] = entryToken;
			entry.tokens = subTokens;
			int oldStart = entry.tokenStart;
			entry.tokenStart = 0;
			entry.tokenCount = subTokens.Count;
			// adjust subtoken lists along with this new list
			for(int i = 0; i < subTokens.Count; ++i) {
				SyntaxTree e = subTokens[i].GetAsSyntaxNode();
				if(e != null && e.tokenStart != 0) {
					e.tokens = subTokens;
					e.tokenStart -= oldStart;
				}
			}
		}
	}
}
