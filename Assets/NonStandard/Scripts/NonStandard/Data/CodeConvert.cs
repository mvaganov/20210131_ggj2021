using NonStandard.Data.Parse;
using NonStandard.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data {
	public class CodeConvert {
		public static string Stringify(object obj) {
			return StringifyExtension.Stringify(obj, false, showBoundary: false);
		}
		public static bool TryFill<T>(string text, ref T data, object scope, Tokenizer tokenizer = null) {
			object value = data;
			bool result = TryParseType(typeof(T), text, ref value, tokenizer);
			data = (T)value;
			return result;
		}
		public static bool TryParse<T>(string text, out T data, object scope, Tokenizer tokenizer = null) {
			object value = null;
			bool result = TryParseType(typeof(T), text, ref value, scope, tokenizer);
			data = (T)value;
			return result;
		}
		public static bool TryParse(Token token, TokenErrLog tokenizer, object scriptVariables, out object result) {
			CodeRules.op_ResolveToken(tokenizer, token, scriptVariables, out result, out Type resultType, false);
			return resultType != null;
		}
		public static bool TryParseType(Type type, string text, ref object data, object scope, Tokenizer tokenizer = null) {
			if (text == null || text.Trim().Length == 0) return false;
			try {
				if (tokenizer == null) { tokenizer = new Tokenizer(); }
				tokenizer.Tokenize(text);
			} catch(Exception e){
				tokenizer.AddError("Tokenize: " + e + "\n" + tokenizer.DebugPrint());
				return false;
			}
			//if(tokenizer.errors.Count > 0) { Show.Error(tokenizer.errors.JoinToString("\n")); }
			//Show.Log(Show.GetStack(4));
			//Show.Log(tokenizer.DebugPrint(-1));
			return TryParseTokens(type, tokenizer.tokens, ref data, scope, tokenizer);
		}
		public static bool TryParseTokens(Type type, List<Token> tokens, ref object data, object scope, Tokenizer tokenizer) {
			bool result = false;
			Parser p = new Parser();
			p.Init(type, tokens, data, tokenizer, scope);
			try {
				result = p.TryParse();
				data = p.result;
			} catch (Exception e) {
				tokenizer.AddError("TryParseTokens:" + e + "\n" + p.GetCurrentTokenIndex().JoinToString(", ") + "\n" + tokenizer.DebugPrint());
			}
			return result;
		}

		public static bool IsConvertable(Type typeToGet) {
			switch (Type.GetTypeCode(typeToGet)) {
			case TypeCode.Boolean:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Char:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Single:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Double:
			case TypeCode.String:
				return true;
			}
			return typeToGet.IsEnum;
		}
		/// <summary>
		/// does convert, will throw <see cref="FormatException"/> if convert fails
		/// </summary>
		/// <param name="value"></param>
		/// <param name="typeToConvertTo"></param>
		public static void Convert(ref object value, Type typeToConvertTo) {
			if (!TryConvert(ref value, typeToConvertTo)) {
				throw new FormatException("could not convert \"" + value + "\" to type " + typeToConvertTo);
			}
		}
		public static bool TryConvert(ref object value, Type typeToGet) {
			if (value != null && value.GetType() == typeToGet) return true;
			try {
				if (typeToGet.IsEnum) {
					string str = value as string;
					if (str != null) { return ReflectionParseExtension.TryConvertEnumWildcard(typeToGet, str, out value); }
				}
				switch (Type.GetTypeCode(typeToGet)) {
				case TypeCode.Boolean:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Single:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Double:
				case TypeCode.String:
					value = System.Convert.ChangeType(value, typeToGet);
					break;
				default:
					if (TryConvertIList(ref value, typeToGet)) { 
						return true;
					}
					return false;
				}
			} catch { return false; }
			return true;
		}
		public static bool TryConvertIList(ref object value, Type resultListType, Type resultElementType = null) {
			Type outputListElementType = resultElementType != null ? resultElementType : resultListType.GetIListType();
			if (outputListElementType == null) { return false; }
			IList ilist = (IList)value;
			if (resultListType.IsArray) {
				try {
					Array oArray = Array.CreateInstance(outputListElementType, ilist.Count);
					for (int i = 0; i < ilist.Count; ++i) {
						object element = ilist[i];
						if (outputListElementType.IsAssignableFrom(element.GetType()) || TryConvert(ref element, outputListElementType)) {
							oArray.SetValue(element, i);
						}
					}
					value = oArray;
				} catch (Exception e) {
					Show.Error("array creation:" + e);
					return false;
				}
			} else if (resultListType.IsGenericType) {
				try {
					object result = resultListType.GetNewInstance();
					IList olist = result as IList;
					for (int i = 0; i < ilist.Count; ++i) {
						object element = ilist[i];
						if (outputListElementType.IsAssignableFrom(element.GetType()) || TryConvert(ref element, outputListElementType)) {
							olist.Add(element);
						}
					}
					value = olist;
				} catch (Exception e) {
					Show.Error("List creation:" + e);
					return false;
				}
			}
			return true;
		}

		public static string Format(string format, object scope, Tokenizer tokenizer = null) {
			if (tokenizer == null) { tokenizer = new Tokenizer(); }
			tokenizer.Tokenize(format, CodeRules.CodeInString);
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < tokenizer.tokens.Count; ++i) {
				object obj;
				Type type;
				Token token = tokenizer.tokens[i];
				CodeRules.op_ResolveToken(tokenizer, token, scope, out obj, out type);
				sb.Append(obj.ToString());
			}
			return sb.ToString();
		}
	}
}
