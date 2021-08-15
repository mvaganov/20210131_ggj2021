using NonStandard.Extension;
using System;
using System.Collections.Generic;

namespace NonStandard.Data.Parse {
	public class DelimCtx : Delim {
		public ParseRuleSet Context {
			get {
				return foundContext != null ? foundContext : foundContext = ParseRuleSet.GetContext(contextName);
			}
		}
		private ParseRuleSet foundContext = null;
		public string contextName;
		public bool isStart, isEnd;
		public DelimCtx(string delim, string name = null, string desc = null, ParseRule parseRule = null,
			string ctx = null, bool s = false, bool e = false, SyntaxRequirement addReq = null, bool printable = true)
			: base(delim, name, desc, parseRule, addReq, printable) {
			contextName = ctx; isStart = s; isEnd = e;
		}
	}
	public class DelimOp : Delim {
		public delegate object TokenResolver(TokenErrLog errLog, ParseRuleSet.Entry ruleContext, object variableContext);
		public delegate ParseRuleSet.Entry SyntaxContextGetter(Tokenizer tokenizer, List<Token> tokens, int index);

		public int order;
		public SyntaxContextGetter isSyntaxValid = null;
		public TokenResolver resolve = null;
		public DelimOp(string delim, string name = null, string desc = null, ParseRule parseRule = null, SyntaxRequirement addReq = null, int order = 100, SyntaxContextGetter syntax = null, TokenResolver resolve = null)
			: base(delim, name, desc, parseRule, addReq) {
			this.order = order; isSyntaxValid = syntax; this.resolve = resolve;
		}
	}
	public class Delim : IComparable<Delim> {
		public delegate bool SyntaxRequirement(string text, int index);
		public delegate ParseResult ParseRule(string text, int index);

		public string text, name, description;
		public ParseRule parseRule = null;
		public SyntaxRequirement extraReq = null;
		public bool printable = true;
		public Delim(string delim, string name = null, string desc = null, ParseRule parseRule = null, SyntaxRequirement addReq = null, bool printable = true) {
			text = delim; this.name = name; description = desc; this.parseRule = parseRule; extraReq = addReq; this.printable = printable;
		}
		/// <summary>
		/// checks if this delimiter is found in the given string, at the given index
		/// </summary>
		/// <param name="str"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool IsAt(string str, int index) {
			if (!str.IsSubstringAt(text, index)) { return false; }
			if (extraReq != null) { return extraReq.Invoke(str, index); }
			return true;
		}
		public override string ToString() { return printable?text:""; }
		public static implicit operator Delim(string s) { return new Delim(s); }

		public int CompareTo(Delim other) {
			int len = Math.Min(text.Length, other.text.Length);
			for (int i = 0; i < len; ++i) { int comp = text[i] - other.text[i]; if (comp != 0) return comp; }
			if (text.Length > other.text.Length) return -1;
			if (text.Length < other.text.Length) return 1;
			if (extraReq != null && other.extraReq == null) return -1;
			if (extraReq == null && other.extraReq != null) return 1;
			return 0;
		}
	}
}
