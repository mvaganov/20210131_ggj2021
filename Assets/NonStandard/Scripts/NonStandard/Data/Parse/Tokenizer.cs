using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data.Parse {
	public interface TokenErrLog {
		bool HasError();
		void AddError(ParseError error);
		string ErrorString();
		IList<int> TextRows();
	}
	public static class TokenizationErrorStorageExtension {
		public static ParseError AddError(this TokenErrLog self, int index, string message) {
			ParseError e = new ParseError(index, self.TextRows(), message); self.AddError(e); return e;
		}
		public static ParseError AddError(this TokenErrLog self, Token token, string message) {
			return AddError(self, token.index, message);
		}
		public static bool ShowErrorTo(this TokenErrLog self, Show.PrintFunc show) {
			string errStr = self.ErrorString();
			if (string.IsNullOrEmpty(errStr)) return false;
			show.Invoke(errStr); return true;
		}
	}
	public class Tokenizer : TokenErrLog {
		internal string str;
		internal List<Token> tokens = new List<Token>(); // actually a tree. each token can point to more token lists
		/// <summary>
		/// TODO testme! does this produce all of the tokens in string form?
		/// </summary>
		protected List<string> tokenStrings = new List<string>();
		public List<ParseError> errors = new List<ParseError>();
		/// <summary>
		/// the indexes of where rows end (newline characters), in order.
		/// </summary>
		internal List<int> rows = new List<int>();
		public IList<int> TextRows() => rows;
		public bool HasError() { return errors.Count > 0; }
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
		public object GetResolvedToken(int i, object scope = null) { return GetToken(i).Resolve(this, scope, true, true); }
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
		public void FilePositionOf(Token token, out int row, out int col) {
			ParseError.FilePositionOf(token, rows, out row, out col);
		}
		public string FilePositionOf(Token token) {
			List<ParseRuleSet.Entry> traversed = new List<ParseRuleSet.Entry>();
			while (!token.IsValid) {
				ParseRuleSet.Entry e = token.GetAsContextEntry();
				if (e == null || traversed.IndexOf(e) >= 0) return "???";
				traversed.Add(e);
				token = e.tokens[0];
			}
			return ParseError.FilePositionOf(token, rows);
		}
		public ParseError AddError(string message) { return AddError(-1, message); }
		public ParseError AddError(int index, string message) {
			ParseError e = new ParseError(index, rows, message); errors.Add(e); return e;
		}
		public ParseError AddError(Token token, string message) { return AddError(token.index, message); }
		public void AddError(ParseError error) { errors.Add(error); }
		public string ErrorString() { return errors.JoinToString("\n"); }
		public bool ShowErrorTo(Show.PrintFunc show) {
			if (errors.Count == 0) return false;
			show.Invoke(ErrorString()); return true;
		}
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
				ParseRuleSet.Entry e = t.GetAsContextEntry();
				if (e != null) {
					//if ((r = path.IndexOf(e.tokens)) >= 0) {
					//	string message = "/* recurse " + (path.Count - r) + " */";
					//	sb.Append(message);
					//} else
					if (e.tokens != tokens) {
						ParseRuleSet.Entry prevEntry = i > 0 ? tokens[i - 1].GetAsContextEntry() : null;
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
		protected void Tokenize(ParseRuleSet parseRules = null, int index = 0, Func<Tokenizer, bool> condition = null) {
			tokenStrings.Clear();
			if (string.IsNullOrEmpty(str)) return;
			List<ParseRuleSet.Entry> contextStack = new List<ParseRuleSet.Entry>();
			if (parseRules == null) parseRules = CodeRules.Default;
			else { contextStack.Add(parseRules.GetEntry(tokens, -1, null)); }
			int tokenBegin = -1;
			ParseRuleSet currentContext = parseRules;
			while (index < str.Length && (condition == null || condition.Invoke(this))) {
				char c = str[index];
				Delim delim = currentContext.GetDelimiterAt(str, index);
				if (delim != null) {
					FinishToken(index, ref tokenBegin); // finish whatever token was being read before this delimeter
					HandleDelimiter(delim, ref index, contextStack, ref currentContext, parseRules);
				} else if (!currentContext.IsWhitespace(c)) {
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
		private void HandleDelimiter(Delim delim, ref int index,  List<ParseRuleSet.Entry> contextStack, 
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
			ParseRuleSet.Entry endedContext = null;
			if (dcx != null) {
				if (contextStack.Count > 0 && dcx.Context == currentContext && dcx.isEnd) {
					endedContext = contextStack[contextStack.Count - 1];
					endedContext.endDelim = dcx;
					delimToken.meta = endedContext;
					endedContext.tokenCount = (tokens.Count - endedContext.tokenStart) + 1;
					contextStack.RemoveAt(contextStack.Count - 1);
					if (contextStack.Count > 0) {
						currentContext = contextStack[contextStack.Count - 1].parseRules;
					} else {
						currentContext = defaultContext;
					}
				}
				if (endedContext == null && dcx.isStart) {
					ParseRuleSet.Entry parentCntx = (contextStack.Count > 0) ? contextStack[contextStack.Count - 1] : null;
					ParseRuleSet.Entry newContext = dcx.Context.GetEntry(tokens, tokens.Count, str, parentCntx);
					newContext.beginDelim = dcx;
					currentContext = dcx.Context;
					delimToken.meta = newContext;
					contextStack.Add(newContext);
				}
			}
			tokens.Add(delimToken);
			tokenStrings.Add(delim.text);
			if (endedContext != null) { ExtractContextAsSubTokenList(endedContext); }
		}
		private void FinalTokenCleanup() {
			for (int i = 0; i < tokens.Count; ++i) {
				// any unfinished contexts must end. the last place they could end is the end of this string
				ParseRuleSet.Entry e = tokens[i].GetAsContextEntry();
				if (e != null && e.tokenCount < 0) {
					e.tokenCount = tokens.Count - e.tokenStart;
					ExtractContextAsSubTokenList(e);
					if (e.parseRules != CodeRules.CommentLine) { // this is an error, unless it's a comment
						errors.Add(new ParseError(tokens[i], rows, 
							$"missing closing token after {e.GetBeginToken().ToString().StringifySmall()}"));
					}
					// close any unfinished contexts inside of this context too!
					tokens = e.tokens;
					i = 0;
				}
			}
			// remove this code eventually TODO
			List<Token> allTokens = new List<Token>(tokens);
			int travelIndex = 0;
			BreadthFirstSearch(allTokens, ref travelIndex);
		}

		protected void BreadthFirstSearch(List<Token> travelLog, ref int index) {
			while(index < travelLog.Count) {
				Token iter = travelLog[index];
				ParseRuleSet.Entry e = iter.GetAsContextEntry();
				if (e != null) {
					for (int i = 0; i < e.tokens.Count; ++i) {
						Token token = e.tokens[i];
						int inTheList = token.IsValid ? travelLog.FindIndex(t=>t==token) : -1;
						if (inTheList >= 0 && inTheList < index && travelLog[inTheList].IsValid) {
							throw new Exception("recursion! " + token.index + " " + token);
						}
						if (inTheList < 0 && token.GetAsContextEntry() != e) {
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
					ParseRuleSet.Entry pathNode = null;
					Token t = GetTokenAt(tokens, paths[i].path, ref pathNode);
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
					if (pathNode != null && pathNode.tokenCount != pathNode.tokens.Count) {
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
				ParseRuleSet.Entry e = null;
				Token t = GetTokenAt(tokens, arr, ref e);
				return arr.JoinToString(", ") + ":" + t + " @" + ParseError.FilePositionOf(t, rows);
			});
		}
		Token GetTokenAt(List<Token> currentPath, IList<int> index, ref ParseRuleSet.Entry lastPathNode) {
			Token t = currentPath[index[0]];
			if (index.Count == 1) return t;
			index = index.GetRange(1, index.Count - 1);
			lastPathNode = t.GetAsContextEntry();
			return GetTokenAt(lastPathNode.tokens, index, ref lastPathNode);
		}
		struct TokenPath {
			public int[] path; public Token token; public ParseRuleSet.Entry pathNode;
		}
		List<TokenPath> FindTokenPaths(Func<Token, bool> predicate, bool justOne = false) {
			if (tokens.Count == 0) return new List<TokenPath>();
			List<List<Token>> path = new List<List<Token>>();
			List<int> position = new List<int>();
			List<TokenPath> paths = new List<TokenPath>();
			path.Add(tokens);
			position.Add(0);
			ParseRuleSet.Entry e = null;
			while (position[position.Count-1] < path[path.Count - 1].Count) {
				List<Token> currentTokens = path[path.Count - 1];
				int currentIndex = position[position.Count - 1];
				Token token = currentTokens[currentIndex];
				if (predicate(token)) { paths.Add(new TokenPath { path = position.ToArray(), token = token, pathNode = e }); }
				e = token.GetAsContextEntry();
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
		internal void ExtractContextAsSubTokenList(ParseRuleSet.Entry entry) {
			if(entry.tokenCount <= 0) { throw new Exception("what just happened?"); }
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
			int index = subTokens.FindIndex(t => t.GetAsContextEntry() == entry);
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
				ParseRuleSet.Entry e = subTokens[i].GetAsContextEntry();
				if(e != null && e.tokenStart != 0) {
					e.tokens = subTokens;
					e.tokenStart -= oldStart;
				}
			}
		}
	}
}
