﻿using NonStandard.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NonStandard.Data.Parse {
	class CodeRules {

		public static ParseRuleSet
			String, Char, Number, Hexadecimal, Expression, SquareBrace, GenericArgs, CodeBody,
			CodeInString, Sum, Difference, Product, Quotient, Modulus, Power, LogicalAnd, LogicalOr,
			Assignment, Equal, LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual,
			NotEqual, XmlCommentLine, CommentLine, CommentBlock, Default;

		public static Delim[] _string_delimiter = new Delim[] { new DelimCtx("\"", ctx: "string", s: true, e: true), };
		public static Delim[] _char_delimiter = new Delim[] { new DelimCtx("\'", ctx: "char", s: true, e: true), };
		public static Delim[] _char_escape_sequence = new Delim[] { new Delim("\\", parseRule: StringExtension.UnescapeStringSequenceAt) };
		public static Delim[] _expression_delimiter = new Delim[] { new DelimCtx("(", ctx: "()", s: true), new DelimCtx(")", ctx: "()", e: true) };
		public static Delim[] _code_body_delimiter = new Delim[] { new DelimCtx("{", ctx: "{}", s: true), new DelimCtx("}", ctx: "{}", e: true) };
		public static Delim[] _string_code_body_delimiter = new Delim[] {
			//new DelimCtx("\"", ctx: "codeInString", s: true, e: true), 
			new Delim("{{",parseRule:(str,i)=>new ParseResult(2,"{")),
			new Delim("}}",parseRule:(str,i)=>new ParseResult(2,"}")),
			new DelimCtx("{", ctx: "{}", s: true), 
			new DelimCtx("}", ctx: "{}", e: true) };
		public static Delim[] _square_brace_delimiter = new Delim[] { new DelimCtx("[", ctx: "[]", s: true), new DelimCtx("]", ctx: "[]", e: true) };
		public static Delim[] _triangle_brace_delimiter = new Delim[] { new DelimCtx("<", ctx: "<>", s: true), new DelimCtx(">", ctx: "<>", e: true) };
		public static Delim[] _ternary_operator_delimiter = new Delim[] { "?", ":", "??" };
		public static Delim[] _instruction_finished_delimiter = new Delim[] { ";" };
		public static Delim[] _list_item_delimiter = new Delim[] { "," };
		public static Delim[] _membership_operator = new Delim[] { new Delim(".", "member"), new Delim("->", "pointee"), new Delim("::", "scope resolution"), new Delim("?.", "null conditional") };
		public static Delim[] _prefix_unary_operator = new Delim[] { "++", "--", "!", "-", "~" };
		public static Delim[] _postfix_unary_operator = new Delim[] { "++", "--" };
		public static Delim[] _binary_operator = new Delim[] { "&", "|", "<<", ">>", "^" };
		// https://en.wikipedia.org/wiki/Order_of_operations#:~:text=In%20mathematics%20and%20computer%20programming,evaluate%20a%20given%20mathematical%20expression.
		public static Delim[] _binary_logic_operatpor = new DelimOp[] {
			new DelimOp("==",syntax:CodeRules.opinit_equ,resolve:CodeRules.op_equ, order:70),
			new DelimOp("!=",syntax:CodeRules.opinit_neq,resolve:CodeRules.op_neq, order:71),
			new DelimOp("<", syntax:CodeRules.opinit_lt_,resolve:CodeRules.op_lt_, order:60),
			new DelimOp(">", syntax:CodeRules.opinit_gt_,resolve:CodeRules.op_gt_, order:61),
			new DelimOp("<=",syntax:CodeRules.opinit_lte,resolve:CodeRules.op_lte, order:62),
			new DelimOp(">=",syntax:CodeRules.opinit_gte,resolve:CodeRules.op_gte, order:63),
			new DelimOp("&&",syntax:CodeRules.opinit_and,resolve:CodeRules.op_and, order:110),
			new DelimOp("||",syntax:CodeRules.opinit_or_,resolve:CodeRules.op_or_, order:120)
		};
		public static Delim[] _assignment_operator = new Delim[] { "+=", "-=", "*=", "/=", "%=", "|=", "&=", "<<=", ">>=", "??=", "=" };
		public static Delim[] _lambda_operator = new Delim[] { "=>" };
		public static Delim[] _math_operator = new DelimOp[] {
			new DelimOp("+", syntax:CodeRules.opinit_add,resolve:CodeRules.op_add, order:40),
			new DelimOp("-", syntax:CodeRules.opinit_dif,resolve:CodeRules.op_dif, order:41),
			new DelimOp("*", syntax:CodeRules.opinit_mul,resolve:CodeRules.op_mul, order:30),
			new DelimOp("/", syntax:CodeRules.opinit_div,resolve:CodeRules.op_div, order:31),
			new DelimOp("%", syntax:CodeRules.opinit_mod,resolve:CodeRules.op_mod, order:32),
			new DelimOp("^^",syntax:CodeRules.opinit_pow,resolve:CodeRules.op_pow, order:20),
		};
		public static Delim[] _hex_number_prefix = new Delim[] { new DelimCtx("0x", ctx: "0x", parseRule: StringExtension.HexadecimalParse) };
		public static Delim[] _number = new Delim[] {
			new DelimCtx("-",ctx:"number",parseRule:StringExtension.NumericParse,addReq:IsNextCharacterBase10NumericOrDecimal),
			new DelimCtx(".",ctx:"number",parseRule:StringExtension.NumericParse,addReq:IsNextCharacterBase10Numeric),
			new DelimCtx("0",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("1",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("2",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("3",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("4",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("5",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("6",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("7",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("8",ctx:"number",parseRule:StringExtension.NumericParse),
			new DelimCtx("9",ctx:"number",parseRule:StringExtension.NumericParse) };
		public static Delim[] _block_comment_delimiter = new Delim[] { new DelimCtx("/*", ctx: "/**/", s: true), new DelimCtx("*/", ctx: "/**/", e: true) };
		public static Delim[] _line_comment_delimiter = new Delim[] { new DelimCtx("//", ctx: "//", s: true) };
		public static Delim[] _XML_line_comment_delimiter = new Delim[] { new DelimCtx("///", ctx: "///", s: true) };
		public static Delim[] _end_of_line_comment = new Delim[] { new DelimCtx("\n", ctx: "//", e: true, printable: false), new DelimCtx("\r", ctx: "//", e: true,printable:false) };
		public static Delim[] _erroneous_end_of_string = new Delim[] { new DelimCtx("\n", ctx: "string", e: true, printable: false), new DelimCtx("\r", ctx: "string", e: true, printable: false) };
		public static Delim[] _end_of_XML_line_comment = new Delim[] { new DelimCtx("\n", ctx: "///", e: true, printable: false), new DelimCtx("\r", ctx: "///", e: true, printable: false) };
		public static Delim[] _line_comment_continuation = new Delim[] { new Delim("\\", parseRule: CommentEscape) };
		public static Delim[] _data_keyword = new Delim[] { "null", "true", "false", "bool", "int", "short", "string", "long", "byte",
			"float", "double", "uint", "ushort", "sbyte", "char", "if", "else", "void", "var", "new", "as", };
		public static Delim[] _data_c_sharp_keyword = new Delim[] {
			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
			"const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
			"explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
			"implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
			"object", "operator", "out", "override", "params", "private", "protected", "public", "readonly",
			"ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct",
			"switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
			"ushort", "using", "virtual", "void", "volatile", "while"
		};
		public static Delim[] None = new Delim[] { };
		public static char[] WhitespaceDefault = new char[] { ' ', '\t', '\n', '\r' };
		public static char[] WhitespaceNone = new char[] { };

		public static Delim[] CharLiteralDelimiters = CombineDelims(_char_escape_sequence, _char_delimiter);
		public static Delim[] StringLiteralDelimiters = CombineDelims(_char_escape_sequence, _string_delimiter, _erroneous_end_of_string);
		public static Delim[] StandardDelimiters = CombineDelims(_string_delimiter, _char_delimiter,
			_expression_delimiter, _code_body_delimiter, _square_brace_delimiter, _ternary_operator_delimiter,
			_instruction_finished_delimiter, _list_item_delimiter, _membership_operator, _prefix_unary_operator,
			_binary_operator, _binary_logic_operatpor, _assignment_operator, _lambda_operator, _math_operator,
			_block_comment_delimiter, _line_comment_delimiter, _number);
		public static Delim[] LineCommentDelimiters = CombineDelims(_line_comment_continuation, _end_of_line_comment);
		public static Delim[] XmlCommentDelimiters = CombineDelims(_line_comment_continuation,
			_end_of_XML_line_comment);
		public static Delim[] CommentBlockDelimiters = CombineDelims(_block_comment_delimiter);

		static CodeRules() {
			Default = new ParseRuleSet("default");
			String = new ParseRuleSet("string");
			Char = new ParseRuleSet("char");
			Number = new ParseRuleSet("number");
			Hexadecimal = new ParseRuleSet("0x");
			Expression = new ParseRuleSet("()");
			SquareBrace = new ParseRuleSet("[]");
			GenericArgs = new ParseRuleSet("<>");
			CodeBody = new ParseRuleSet("{}");
			CodeInString = new ParseRuleSet("codeInString");
			Sum = new ParseRuleSet("sum", CodeRules.None);
			Difference = new ParseRuleSet("difference", CodeRules.None);
			Product = new ParseRuleSet("product", CodeRules.None);
			Quotient = new ParseRuleSet("quotient", CodeRules.None);
			Modulus = new ParseRuleSet("modulus", CodeRules.None);
			Power = new ParseRuleSet("power", CodeRules.None);
			LogicalAnd = new ParseRuleSet("logical and", CodeRules.None);
			LogicalOr = new ParseRuleSet("logical or", CodeRules.None);
			Assignment = new ParseRuleSet("assignment", CodeRules.None);
			Equal = new ParseRuleSet("equal", CodeRules.None);
			LessThan = new ParseRuleSet("less than", CodeRules.None);
			GreaterThan = new ParseRuleSet("greater than", CodeRules.None);
			LessThanOrEqual = new ParseRuleSet("less than or equal", CodeRules.None);
			GreaterThanOrEqual = new ParseRuleSet("greater than or equal", CodeRules.None);
			NotEqual = new ParseRuleSet("not equal", CodeRules.None);
			XmlCommentLine = new ParseRuleSet("///");
			CommentLine = new ParseRuleSet("//");
			CommentBlock = new ParseRuleSet("/**/");

			CodeInString.Delimiters = CombineDelims(_string_code_body_delimiter);
			CodeInString.Whitespace = CodeRules.WhitespaceNone;
			XmlCommentLine.Delimiters = CodeRules.XmlCommentDelimiters;
			CommentLine.Delimiters = CodeRules.LineCommentDelimiters;
			CommentBlock.Delimiters = CodeRules.CommentBlockDelimiters;
			CommentLine.Whitespace = CodeRules.WhitespaceNone;
			String.Whitespace = CodeRules.WhitespaceNone;
			String.Delimiters = CodeRules.StringLiteralDelimiters;
			//Char.whitespace = CodeRules.WhitespaceNone;
			//Char.delimiters = CodeRules.StringLiteralDelimiters;
			Number.Whitespace = CodeRules.WhitespaceNone;

			Type t = typeof(CodeRules);
			MemberInfo[] mInfo = t.GetMembers();
			for (int i = 0; i < mInfo.Length; ++i) {
				MemberInfo mi = mInfo[i];
				FieldInfo fi = mi as FieldInfo;
				if (fi != null && mi.Name.StartsWith("_") && fi.FieldType == typeof(Delim[]) && fi.IsStatic) {
					Delim[] delims = fi.GetValue(null) as Delim[];
					GiveDesc(delims, fi.Name.Substring(1).Replace('_', ' '));
				}
			}

			//StringBuilder sb = new StringBuilder();
			//for (int i = 0; i < CodeRules.StandardDelimiters.Length; ++i) {
			//	sb.Append(CodeRules.StandardDelimiters[i].text + " " + CodeRules.StandardDelimiters[i].description).Append("\n");
			//}
			//Show.Log(sb);
		}
		public static bool IsNextCharacterBase10Numeric(string str, int index) {
			const int numberBase = 10;
			if (index < -1 || index + 1 >= str.Length) return false;
			int i = str[index + 1].ToNumericValue(numberBase); return (i >= 0 && i < numberBase);
		}
		public static bool IsNextCharacterBase10NumericOrDecimal(string str, int index) {
			const int numberBase = 10;
			if (index < -1 || index + 1 >= str.Length) return false;
			char c = str[index + 1]; if (c == '.') return true;
			int i = c.ToNumericValue(numberBase); return (i >= 0 && i < numberBase);
		}
		public static ParseResult CommentEscape(string str, int index) { return str.UnescapeStringSequenceAt(index); }

		private static void GiveDesc(Delim[] delims, string desc) {
			for (int i = 0; i < delims.Length; ++i) { if (delims[i].description == null) { delims[i].description = desc; } }
		}
		public static Delim[] CombineDelims(params Delim[][] delimGroups) {
			List<Delim> delims = new List<Delim>();
			for (int i = 0; i < delimGroups.Length; ++i) { delims.AddRange(delimGroups[i]); }
			delims.Sort();
			return delims.ToArray();
		}

		public static ParseRuleSet.Entry opinit_Binary(List<Token> tokens, Tokenizer tok, int index, string contextName) {
			Token t = tokens[index];
			ParseRuleSet.Entry e = tokens[index].GetAsContextEntry();
			if (e != null) {
				if (e.context.name != contextName) { throw new Exception(tok.AddError(t,
					"expected context: "+contextName+", found "+e.context.name).ToString()); }
				return e;
			}
			if (index - 1 < 0) { tok.AddError(t, "missing left operand"); return null; }
			if (index + 1 >= tokens.Count) { tok.AddError(t, "missing right operand"); return null; }
			ParseRuleSet foundContext; ParseRuleSet.allContexts.TryGetValue(contextName, out foundContext);
			if (foundContext == null) { throw new Exception(tok.AddError(t, "context '" + contextName + "' does not exist").ToString()); }
			ParseRuleSet.Entry parent = null; int pIndex;
			for (pIndex = 0; pIndex < tokens.Count; ++pIndex) {
				e = tokens[pIndex].GetAsContextEntry();
				if(e != null && e.tokens == tokens) { parent = e; break; }
			}
			if (pIndex == index) { throw new Exception(tok.AddError(t,"parent context recursion").ToString()); }
			e = foundContext.GetEntry(tokens, index - 1, t.meta, parent);
			e.tokenCount = 3;
			t.meta = e;
			tokens[index] = t;
			tok.ExtractContextAsSubTokenList(e);
			return e;
		}
		public static ParseRuleSet.Entry opinit_add(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "sum"); }
		public static ParseRuleSet.Entry opinit_dif(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "difference"); }
		public static ParseRuleSet.Entry opinit_mul(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "product"); }
		public static ParseRuleSet.Entry opinit_div(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "quotient"); }
		public static ParseRuleSet.Entry opinit_mod(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "modulus"); }
		public static ParseRuleSet.Entry opinit_pow(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "power"); }
		public static ParseRuleSet.Entry opinit_and(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "logical and"); }
		public static ParseRuleSet.Entry opinit_or_(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "logical or"); }
		public static ParseRuleSet.Entry opinit_asn(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "assign"); }
		public static ParseRuleSet.Entry opinit_equ(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "equal"); }
		public static ParseRuleSet.Entry opinit_neq(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "not equal"); }
		public static ParseRuleSet.Entry opinit_lt_(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "less than"); }
		public static ParseRuleSet.Entry opinit_gt_(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "greater than"); }
		public static ParseRuleSet.Entry opinit_lte(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "less than or equal"); }
		public static ParseRuleSet.Entry opinit_gte(Tokenizer tok, List<Token> tokens, int index) { return opinit_Binary(tokens, tok, index, "greater than or equal"); }

		public static void op_ResolveToken(Tokenizer tok, Token token, object scope, out object value, out Type type) {
			value = token.Resolve(tok, scope);
			type = (value != null) ? value.GetType() : null;
			if (scope == null || type == null) { return; } // no scope, or no data, easy. we're done.
			string name = value as string;
			if (name == null) {  // data not a string (can't be a reference from scope), also easy. done.
				List<object> args = value as List<object>;
				if(args != null) {
					for(int i = 0; i < args.Count; ++i) {
						bool remove = false;
						op_ResolveToken(tok, new Token(args[i], -1, -1), scope, out value, out type);
						switch (value as string) { case ",": remove = true; break; }
						if (remove) { args.RemoveAt(i--); } else { args[i] = value; }
					}
					value = args;
					type = args.GetType();
				}
				return;
			}
			ParseRuleSet.Entry e = token.GetAsContextEntry();
			if (e != null && e.IsText()) { return; } // data is explicitly meant to be a string, done.
			switch (name) {
			case "null": value = null; type = null; return;
			case "true": value = true; type = typeof(bool); return;
			case "false": value = false; type = typeof(bool); return;
			}
			// otherwise, we search for the data within the given context
			Type scopeType = scope.GetType();
			KeyValuePair<Type, Type> dType = scopeType.GetIDictionaryType();
			if(dType.Key != null) {
				if (dType.Key == typeof(string) && 
					(name[0]==(Parser.Wildcard) || name[name.Length-1]==(Parser.Wildcard))) {
					MethodInfo getKey = null;
					IEnumerator en = (IEnumerator)scopeType.GetMethod("GetEnumerator", Type.EmptyTypes).Invoke(scope,new object[] { });
					while(en.MoveNext()) {
						object kvp = en.Current;
						if (getKey == null) {
							getKey = kvp.GetType().GetProperty("Key").GetGetMethod();
						}
						string memberName = getKey.Invoke(kvp, null) as string;
						if(Parser.IsWildcardMatch(memberName, name)) {
							name = memberName;
							break;
						}
					}
				}
				// how to generically interface with standard Dictionary objects
				IDictionary dict = scope as IDictionary;
				if (dict != null) {
					if (dict.Contains(name)) { value = dict[name]; }
				} else {
					// how to generically interface with a non standard dictionary
					MethodInfo mi = scopeType.GetMethod("ContainsKey", new Type[] { dType.Key });
					bool hasIt = false;
					if (mi == null) {
						Show.Error("couldn't find ContainsKey, how about:" + scopeType.GetMethods().JoinToString(", ", m => m.Name));
					} else {
						//Show.Log("~~~#$params: " + mi.GetParameters().JoinToString(", ", p => p.ParameterType.Name));
						//Show.Log("~~~#$return: " + mi.ReturnType.Name+ " " + mi.DeclaringType + "."+mi.Name);
						try {
							hasIt = (bool)mi.Invoke(scope, new object[] { name });
							//Show.Log("~~~#$W COULD seek " + name);
						} catch (Exception) {
							//Show.Log("~~~#$X couldn't find "+name+" in "+scopeType);
							hasIt = false;
						}
					}
					if (hasIt) {
						mi = scopeType.GetMethod("Get", new Type[] { dType.Key });
						if (mi == null) {
							Show.Error("couldn't find Get, how about:" + scopeType.GetMethods().JoinToString(", ", m => m.Name));
							value = null;
						} else {
							value = mi.Invoke(scope, new object[] { name });
						}
					}
				}
				type = (value != null) ? value.GetType() : null;
				return;
			}
			if (name.Length > 0 && (name[0] == Parser.Wildcard || name[name.Length - 1] == Parser.Wildcard)) {
				FieldInfo[] fields = scopeType.GetFields();
				string[] names = Array.ConvertAll(fields, f => f.Name);
				int index = Parser.FindIndexWithWildcard(names, name, false);
				if (index >= 0) {
					//Show.Log(name+" "+scopeType+" :"+index + " " + names.Join(", ") + " " +fields.Join(", "));
					value = fields[index].GetValue(scope);
					type = (value != null) ? value.GetType() : null;
					return;
				}
				PropertyInfo[] props = scopeType.GetProperties();
				names = Array.ConvertAll(props, p => p.Name);
				index = Parser.FindIndexWithWildcard(names, name, false);
				if (index >= 0) {
					value = props[index].GetValue(scope,null);
					type = (value != null) ? value.GetType() : null;
					return;
				}
			} else {
				FieldInfo field = scopeType.GetField(name);
				if (field != null) {
					value = field.GetValue(scope);
					type = (value != null) ? value.GetType() : null;
					return;
				}
				PropertyInfo prop = scopeType.GetProperty(name);
				if (prop != null) {
					value = prop.GetValue(scope,null);
					type = (value != null) ? value.GetType() : null;
					return;
				}
			}
		}
		public static void op_BinaryArgs(Tokenizer tok, ParseRuleSet.Entry e, object scope, out object left, out object right, out Type lType, out Type rType) {
			op_ResolveToken(tok, e.tokens[0], scope, out left, out lType);
			op_ResolveToken(tok, e.tokens[2], scope, out right, out rType);
			// upcast to double. all the math operations expect doubles only, for algorithm simplicity
			if (lType != typeof(string) && lType != typeof(double) && CodeConvert.IsConvertable(lType)) {
				CodeConvert.TryConvert(ref left, typeof(double)); lType = typeof(double);
			}
			if (rType != typeof(string) && rType != typeof(double) && CodeConvert.IsConvertable(rType)) {
				CodeConvert.TryConvert(ref right, typeof(double)); rType = typeof(double);
			}
		}
		public static object op_asn(Tokenizer tok, ParseRuleSet.Entry e, object scope) { return "="; }
		public static object op_mul(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			do {
				bool lString = lType == typeof(string);
				bool rString = rType == typeof(string);
				// if one of them is a string, there is some string multiplication logic to do!
				if (lString != rString) {
					string meaningfulString;
					double meaningfulNumber;
					if (lString) {
						if(!CodeConvert.IsConvertable(rType)) { break; }
						meaningfulString = left.ToString();
						CodeConvert.TryConvert(ref right, typeof(double));
						meaningfulNumber = (double)right;
					} else {
						if (!CodeConvert.IsConvertable(lType)) { break; }
						meaningfulString = right.ToString();
						CodeConvert.TryConvert(ref left, typeof(double));
						meaningfulNumber = (double)left;
					}
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < meaningfulNumber; ++i) {
						sb.Append(meaningfulString);
					}
					meaningfulNumber -= (int)meaningfulNumber;
					int count = (int)(meaningfulString.Length * meaningfulNumber);
					if (count > 0) {
						sb.Append(meaningfulString.Substring(0, count));
					}
					return sb.ToString();
				}
				if (CodeConvert.IsConvertable(lType) && CodeConvert.IsConvertable(rType)) {
					CodeConvert.TryConvert(ref left, typeof(double));
					CodeConvert.TryConvert(ref right, typeof(double));
					return ((double)left) * ((double)right);
				}
			} while (false);
			tok.AddError(e.tokens[1], "unable to multiply " + lType + " and " + rType);
			return e;
		}
		public static object op_add(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			if (lType == typeof(string) || rType == typeof(string)) { return left.ToString() + right.ToString(); }
			if (CodeConvert.IsConvertable(lType) && CodeConvert.IsConvertable(rType)) {
				CodeConvert.TryConvert(ref left, typeof(double));
				CodeConvert.TryConvert(ref right, typeof(double));
				return ((double)left) + ((double)right);
			}
			tok.AddError(e.tokens[1], "unable to add " + lType + " and " + rType + " : " + left + " + " + right);
			return e;
		}
		public static object op_dif(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			do {
				if (lType == typeof(string) || rType == typeof(string)) { break; }
				if (CodeConvert.IsConvertable(lType) && CodeConvert.IsConvertable(rType)) {
					CodeConvert.TryConvert(ref left, typeof(double));
					CodeConvert.TryConvert(ref right, typeof(double));
					return ((double)left) - ((double)right);
				}
			} while (false);
			tok.AddError(e.tokens[1], "unable to subtract " + lType + " and " + rType + " : " + left + " - " + right);
			return e;
		}
		public static object op_div(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			do {
				if (lType == typeof(string) || rType == typeof(string)) { break; }
				if (CodeConvert.IsConvertable(lType) && CodeConvert.IsConvertable(rType)) {
					CodeConvert.TryConvert(ref left, typeof(double));
					CodeConvert.TryConvert(ref right, typeof(double));
					return ((double)left) / ((double)right);
				}
			} while (false);
			tok.AddError(e.tokens[1], "unable to divide " + lType + " and " + rType + " : " + left + " / " + right);
			return e;
		}
		public static int FindEndOfNextToken(Tokenizer tok, int startI, string str, int index, out int started, out int tokenId) {
			started = -1;
			tokenId = -1;
			for (int i = index; i < str.Length; ++i) {
				char c = str[i];
				switch (c) {
				case '{':
					if (i + 1 >= str.Length) {
						tok.AddError(startI+i, "unexpected end of format token"+
							(tokenId<0?"": (" "+tokenId.ToString())));
						return -1;
					}
					if (str[i + 1] != '{') {
						if (started >= 0) {
							tok.AddError(startI + i, "unexpected beginning of new format token" +
								(tokenId < 0 ? "" : (" " + tokenId.ToString())));
							return -1;
						} else {
							started = i;
							ParseResult pr = StringExtension.IntegerParse(str, i + 1);
							if (pr.IsError) {
								pr.error.OffsetBy(startI + i, tok.rows);
								tok.AddError(pr.error);
							} else {
								tokenId = (int)(long)pr.replacementValue;
							}
						}
					} else {
						++i;
					}
					break;
				case '}':
					if (started>=0) {
						if (tokenId < 0) {
							tok.AddError(startI + i, "token missing leading base 10 integer index" +
								(tokenId < 0 ? "" : (" " + tokenId.ToString())));
						}
						return i + 1;
					}
					break;
				}
			}
			if (started >= 0) {
				tok.AddError(startI+started, "expected end of format token" +
					(tokenId < 0 ? "" : (" " + tokenId.ToString())));
			}
			return -1;
		}
		public static string Format(string format, List<object> args, object scope, Tokenizer tok, int tIndex) {
			StringBuilder sb = new StringBuilder();
			int index = 0, start, end, tokenId;
			do {
				end = FindEndOfNextToken(tok, tIndex, format, index, out start, out tokenId);
				if (end < 0) {
					sb.Append(format.Substring(index));
					index = format.Length;
				} else {
					if (tokenId < 0 || tokenId >= args.Count) {
						ParseError err = tok.AddError(tIndex, "invalid format token index (limit " +
							args.Count + ")" + (tokenId < 0 ? "" : (" " + tokenId.ToString()))); err.col += start;
						sb.Append(format.Substring(index, end - index));
					} else {
						if (index != start) { sb.Append(format.Substring(index, start - index)); }
						string str = format.Substring(start, end - start);
						str = str.Replace("{" + tokenId, "{" + 0);
						sb.AppendFormat(str, args[tokenId]);
					}
					index = end;
				}
			} while (index < format.Length);
			return sb.ToString();
		}
		public static object op_mod(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			do {
				if (lType == typeof(string)) {
					string format = left as string;
					List<object> args;
					if(rType != typeof(List<object>)) {
						args = new List<object>();
						args.Add(right);
					} else {
						args = right as List<object>;
					}
					return Format(format, args, scope, tok, e.tokens[0].index);
				}
				if (lType == typeof(string) || rType == typeof(string)) { break; }
				if (CodeConvert.IsConvertable(lType) && CodeConvert.IsConvertable(rType)) {
					CodeConvert.TryConvert(ref left, typeof(double));
					CodeConvert.TryConvert(ref right, typeof(double));
					return ((double)left) % ((double)right);
				}
			} while (false);
			tok.AddError(e.tokens[1], "unable to modulo " + lType + " and " + rType + " : " + left + " % " + right);
			return e;
		}
		public static object op_pow(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			do {
				if (lType == typeof(string) || rType == typeof(string)) { break; }
				if (CodeConvert.IsConvertable(lType) && CodeConvert.IsConvertable(rType)) {
					CodeConvert.TryConvert(ref left, typeof(double));
					CodeConvert.TryConvert(ref right, typeof(double));
					return Math.Pow((double)left, (double)right);
				}
			} while (false);
			tok.AddError(e.tokens[1], "unable to exponent " + lType + " and " + rType + " : " + left + " ^^ " + right);
			return e;
		}
		public static bool op_reduceToBoolean(object obj, Type type) {
			if (obj == null) return false;
			if (type == typeof(string)) return ((string)obj).Length != 0;
			if(!CodeConvert.TryConvert(ref obj, typeof(double))) { return true; }
			double d = (double)obj;
			return d != 0;
		}
		public static object op_and(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			return op_reduceToBoolean(left, lType) && op_reduceToBoolean(right, rType);
		}
		public static object op_or_(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			return op_reduceToBoolean(left, lType) || op_reduceToBoolean(right, rType);
		}
		// spaceship operator
		public static bool op_Compare(Tokenizer tok, ParseRuleSet.Entry e, object scope, out int compareValue) {
			object left, right; Type lType, rType;
			op_BinaryArgs(tok, e, scope, out left, out right, out lType, out rType);
			if (lType == rType) { return lType.TryCompare(left, right, out compareValue); }
			compareValue = 0;
			tok.AddError(e.tokens[1].index, "can't operate ("+lType+")"+left+" "+e.tokens[1]+" ("+rType+")"+right);
			return false;
		}
		public static object op_equ(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			int comp; if (op_Compare(tok, e, scope, out comp)) { return comp == 0; }
			return e;
		}
		public static object op_neq(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			int comp; if (op_Compare(tok, e, scope, out comp)) { return comp != 0; }
			return e;
		}
		public static object op_lt_(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			int comp; if (op_Compare(tok, e, scope, out comp)) { return comp < 0; }
			return e;
		}
		public static object op_gt_(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			int comp; if (op_Compare(tok, e, scope, out comp)) { return comp > 0; }
			return e;
		}
		public static object op_lte(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			int comp; if (op_Compare(tok, e, scope, out comp)) { return comp <= 0; }
			return e;
		}
		public static object op_gte(Tokenizer tok, ParseRuleSet.Entry e, object scope) {
			int comp; if (op_Compare(tok, e, scope, out comp)) { return comp >= 0; }
			return e;
		}
	}
}
