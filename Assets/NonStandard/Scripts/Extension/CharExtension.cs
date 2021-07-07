
using System.Collections.Generic;

namespace NonStandard {
	public static class CharExtension {
		public const string Base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789​+/=";
		private static Dictionary<char, int> _Base64Characters = null;
		public static int ToNumericValue(this char c, int numberBase = 10) {
			if (numberBase < 36) {
				if (c >= '0' && c <= '9') return c - '0';
				if (c >= 'A' && c <= 'Z') return (c - 'A') + 10;
				if (c >= 'a' && c <= 'z') return (c - 'a') + 10;
			} else if (numberBase <= 64) {
				if (_Base64Characters == null) {
					_Base64Characters = new Dictionary<char, int>();
					for (int i = 0; i < Base64Characters.Length; ++i) { _Base64Characters[Base64Characters[i]] = i; }
				}
				return _Base64Characters[c];
			}
			return -1;
		}
		public static bool IsValidNumber(this char c, int numberBase) {
			int h = ToNumericValue(c);
			return h >= 0 && h < numberBase;
		}
		public static char LiteralUnescape(this char c) {
			switch (c) {
			case 'a': return '\a';
			case 'b': return '\b';
			case 'n': return '\n';
			case 'r': return '\r';
			case 'f': return '\f';
			case 't': return '\t';
			case 'v': return '\v';
			}
			return c;
		}

		public static string LiteralEscape(this char c) {
			switch (c) {
			case '\a': return ("\\a");
			case '\b': return ("\\b");
			case '\n': return ("\\n");
			case '\r': return ("\\r");
			case '\f': return ("\\f");
			case '\t': return ("\\t");
			case '\v': return ("\\v");
			case '\'': return ("\\\'");
			case '\"': return ("\\\"");
			case '\\': return ("\\\\");
			}
			return null;
		}

	}
}
