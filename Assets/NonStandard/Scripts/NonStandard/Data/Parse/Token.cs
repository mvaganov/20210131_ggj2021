using System;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data.Parse {
	public struct Token : IEquatable<Token>, IComparable<Token> {
		public int index, length; // 32 bits x2
		public object meta; // 64 bits
		public Token(object meta, int i, int len) { this.meta = meta; index = i; length = len; }
		public static Token None = new Token(null, -1, -1);
		public int GetBeginIndex() { return index; }
		public int GetEndIndex() { return index + length; }
		public string ToString(string s) { return s.Substring(index, length); }
		public override string ToString() {
			ParseRuleSet.Entry pce = meta as ParseRuleSet.Entry;
			if (pce != null) {
				Delim d = pce.sourceMeta as Delim;
				if(d != null) { return d.ToString(); }
				if(IsValid) return ToString(pce.TextRaw);
				string output = pce.parseRules.name;
				if (pce.IsText()) {
					output += "(" + pce.GetText() + ")";
				}
				return output;
			}
			return Resolve(null,null).ToString();
		}
		public object Resolve(TokenErrLog tok, object scope, bool simplify = true, bool fullyResolve = false) {
			if (index == -1 && length == -1) return meta;
			if (meta == null) throw new NullReferenceException();
			switch (meta) {
			case string s: {
				string str = ToString(s);
				//Show.Log("@@@  "+str+" "+scope);
				if (scope != null && fullyResolve) {
					CodeRules.op_SearchForMember(str, out object value, out Type type, scope);
					//Show.Log(str+" "+foundIt+" "+value);
					return value;
				}
				return str;
			}
			case TokenSubstitution ss: return ss.value;
			case Delim d: return d.text;
			case ParseRuleSet.Entry pce: return pce.Resolve(tok, scope, simplify, fullyResolve);
			}
			//TokenSubstitution ss = meta as TokenSubstitution; if (ss != null) return ss.value;
			//Delim d = meta as Delim; if (d != null) return d.text;
			//ParseRuleSet.Entry pce = meta as ParseRuleSet.Entry; if (pce != null) return pce.Resolve(tok, scope, simplify, fullyResolve);
			throw new DecoderFallbackException();
		}
		public string GetAsSmallText() {
			ParseRuleSet.Entry e = GetAsContextEntry();
			if (e != null) {
				if (IsContextBeginning()) { return e.beginDelim.ToString(); }
				if (IsContextEnding()) { return e.endDelim.ToString(); }
			}
			return ToString();
		}
		public string GetAsBasicToken() { if (meta is string) { return ((string)meta).Substring(index, length); } return null; }
		public Delim GetAsDelimiter() { return meta as Delim; }
		public ParseRuleSet.Entry GetAsContextEntry() { return meta as ParseRuleSet.Entry; }
		public List<Token> GetTokenSublist() {
			ParseRuleSet.Entry e = GetAsContextEntry();
			if(e != null) {
				return e.tokens;
			}
			return null;
		}
		public bool IsContextBeginning() {
			ParseRuleSet.Entry ctx = GetAsContextEntry(); if (ctx != null) { return ctx.GetBeginToken() == this; }
			return false;
		}
		public bool IsContextEnding() {
			ParseRuleSet.Entry ctx = GetAsContextEntry(); if (ctx != null) { return ctx.GetEndToken() == this; }
			return false;
		}
		public bool IsContextBeginningOrEnding() {
			ParseRuleSet.Entry ctx = GetAsContextEntry();
			if (ctx != null) { return ctx.GetEndToken() == this || ctx.GetBeginToken() == this; }
			return false;
		}
		public bool IsValid { get { return index >= 0 && length >= 0; } }
		public bool IsSimpleString { get { return index >= 0 && length >= 0 && 
					(meta is string || meta is ParseRuleSet.Entry pce && pce.TextRaw != null); } }
		public bool IsDelim { get { return meta is Delim; } }
		public void Invalidate() { length = -1; }
		public bool Equals(Token other) { return index == other.index && length == other.length && meta == other.meta; }
		public override bool Equals(object obj) { if (obj is Token) return Equals((Token)obj); return false; }
		public override int GetHashCode() { return meta.GetHashCode() ^ index ^ length; }
		public int CompareTo(Token other) {
			int comp = index.CompareTo(other.index);
			if (comp != 0) return comp;
			return -length.CompareTo(other.length); // bigger one should go first
		}
		public static bool operator ==(Token lhs, Token rhs) { return lhs.Equals(rhs); }
		public static bool operator !=(Token lhs, Token rhs) { return !lhs.Equals(rhs); }
	}
	public class TokenSubstitution {
		public string origMeta; public object value;
		public TokenSubstitution(string o, object v) { origMeta = o; value = v; }
	}
}
