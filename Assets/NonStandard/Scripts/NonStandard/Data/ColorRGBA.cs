
using System;
using System.Collections.Generic;
using System.Text;
using NonStandard.Extension;

namespace NonStandard.Data {
	public partial struct ColorRGBA {
		public byte r, g, b, a;
		public byte R => r;
		public byte G => g;
		public byte B => b;
		public byte A => a;
		public byte GetComponent(int index) {
			switch (index) { case 0: return r; case 1: return g; case 2: return b; case 3: return a; } return 0;
		}
		public void SetComponent(int index, byte value) {
			switch (index) { case 0: r = value; break; case 1: g = value; break; case 2: b = value; break; case 3: a = value; break; }
		}
		public byte this[int key] {
			get => GetComponent(key);
			set => SetComponent(key, value);
		}
		public ColorRGBA(byte r, byte g, byte b, byte a) {
			this.r = r; this.g = g; this.b = b; this.a = a;
		}
		public ColorRGBA(byte r, byte g, byte b) {
			this.r = r; this.g = g; this.b = b; this.a = 255;
		}
		public static ColorRGBA Lerp(ColorRGBA color0, ColorRGBA color1, float t) {
			t = t.Clamp01();
			return new ColorRGBA(
				(byte)(color0.r + ((int)color1.r - (int)color0.r) * t),
				(byte)(color0.g + ((int)color1.g - (int)color0.g) * t),
				(byte)(color0.b + ((int)color1.b - (int)color0.b) * t),
				(byte)(color0.a + ((int)color1.a - (int)color0.a) * t));
		}
		public static ColorRGBA operator *(ColorRGBA c, float f) {
			return new ColorRGBA( (byte)(c.r * f).Clamp0(255), (byte)(c.g * f).Clamp0(255), (byte)(c.b * f).Clamp0(255), (byte)(c.a * f).Clamp0(255));
		}

		public static readonly ColorRGBA Black = new ColorRGBA(0, 0, 0); // #000
		public static readonly ColorRGBA Darkblue = new ColorRGBA(0, 0, 128); // #008
		public static readonly ColorRGBA Darkgreen = new ColorRGBA(0, 128, 0); // #080
		public static readonly ColorRGBA Darkcyan = new ColorRGBA(0, 128, 128); // #088
		public static readonly ColorRGBA Darkred = new ColorRGBA(128, 0, 0); // #800
		public static readonly ColorRGBA Darkmagenta = new ColorRGBA(128, 0, 128); // #808
		public static readonly ColorRGBA Darkyellow = new ColorRGBA(136, 136, 0); // #880
		public static readonly ColorRGBA Gray = new ColorRGBA(136, 136, 136); // #888
		public static readonly ColorRGBA Grey = new ColorRGBA(136, 136, 136); // #888
		public static readonly ColorRGBA Darkgray = new ColorRGBA(68, 68, 68); // #444
		public static readonly ColorRGBA Blue = new ColorRGBA(0, 0, 255); // #00f
		public static readonly ColorRGBA Green = new ColorRGBA(0, 255, 0); // #0f0
		public static readonly ColorRGBA Cyan = new ColorRGBA(0, 255, 255); // #0ff
		public static readonly ColorRGBA Red = new ColorRGBA(255, 0, 0); // #f00
		public static readonly ColorRGBA Magenta = new ColorRGBA(255, 0, 255); // #f0f
		public static readonly ColorRGBA Yellow = new ColorRGBA(255, 255, 0); // #ff0
		public static readonly ColorRGBA White = new ColorRGBA(255, 255, 255); // #fff
		public static readonly ColorRGBA Lightgray = new ColorRGBA(204, 204, 204); // #ccc
		public static readonly ColorRGBA Clear = new ColorRGBA(0, 0, 0, 0); // #0000

		public static ColorRGBA[] defaultColors = {
			Black,Darkblue,Darkgreen,Darkcyan,Darkred,Darkmagenta,Darkyellow,Lightgray,
			Gray,Blue,Green,Cyan,Red,Magenta,Yellow,White,Darkgray,Clear };

		public static implicit operator ColorRGBA(ConsoleColor c) { return defaultColors[(int)c]; }
		public static implicit operator ConsoleColor(ColorRGBA c) { return c.ClosestConsoleColor(); }

		public static int Closest(ColorRGBA needle, ColorRGBA[] haystack) {
			int best = -1;
			if (haystack == null || haystack.Length == 0) return best;
			best = 0;
			int bestDistance = needle.ManhattanDistance(haystack[0]);
			for(int i = 1; i < haystack.Length; ++i) {
				int dist = needle.ManhattanDistance(haystack[i]);
				if(dist < bestDistance) { bestDistance = dist; best = i; }
			}
			return best;
		}
		public ConsoleColor ClosestConsoleColor() {
			return (ConsoleColor)Closest(this, defaultColors);
		}

		public byte grayscale => (byte)(255 * 0.29899999499321 * r + 0.587000012397766 * g + 57.0 / 500.0 * b);

		public override bool Equals(object c) {
			if (GetType() != c.GetType()) { return false; }
			return Equals((ColorRGBA)c);
		}

		public bool Equals(ColorRGBA c) => r == c.r && g == c.g && b == c.b && a == c.a;

		public override int GetHashCode() => (int)ToUInt32();

		public static bool operator ==(ColorRGBA lhs, ColorRGBA rhs) => lhs.Equals(rhs);

		public static bool operator !=(ColorRGBA lhs, ColorRGBA rhs) => !(lhs == rhs);

		public static ColorRGBA itocBGRA(uint color) {
			return new ColorRGBA(
			  (byte)((color >> 16) & 0xff),
			  (byte)((color >> 8) & 0xff),
			  (byte)((color >> 0) & 0xff),
			  (byte)((color >> 24) & 0xff));
		}
		public static ColorRGBA itocRGBA(uint color) {
			return new ColorRGBA(
			  (byte)((color >> 0) & 0xFF),
			  (byte)((color >> 8) & 0xFF),
			  (byte)((color >> 16) & 0xFF),
			  (byte)((color >> 24) & 0xFF));
		}

		public static uint ctoiBGRA(ColorRGBA c) {
			return (uint)(c.b << 0) | (uint)(c.g << 8) | (uint)(c.r << 16) | (uint)(c.a << 24);
		}
		public static uint ctoiRGBA(ColorRGBA c) {
			return (uint)(c.r << 0) | (uint)(c.g << 8) | (uint)(c.b << 16) | (uint)(c.a << 24);
		}

		public uint ToUInt32() => ctoiBGRA(this);
		public uint ToUInt32rgba() => ctoiRGBA(this);
		public uint ToUInt32bgra() => ctoiBGRA(this);

		public string ToHexString(bool alwaysIncludeAlpha = false) {
			StringBuilder sb = new StringBuilder();
			sb.Append(r.ToString("X2")).Append(g.ToString("X2")).Append(b.ToString("X2"));
			if (alwaysIncludeAlpha || a != 255) { sb.Append(a.ToString("X2")); }
			return sb.ToString();
		}
		public string ToHexStringRGB() {
			StringBuilder sb = new StringBuilder();
			sb.Append(r.ToString("X2")).Append(g.ToString("X2")).Append(b.ToString("X2"));
			return sb.ToString();
		}
		public string ToHexStringMaybeShort(bool alwaysIncludeAlpha = false) {
			StringBuilder sb = new StringBuilder();
			string _r = r.ToString("X2"), _g = g.ToString("X2"), _b = b.ToString("X2"), _a = a.ToString("X2");
			bool canBeShort = _r[0] == _r[1] && _g[0] == _g[1] && _b[0] == _b[1] && _a[0] == _a[1];
			if (canBeShort) {
				sb.Append(_r[0]).Append(_g[0]).Append(_b[0]);
			} else {
				sb.Append(_r).Append(_g).Append(_b);
			}
			if (alwaysIncludeAlpha || a != 255) { sb.Append(_a); }
			return sb.ToString();
		}
		public enum ColorStringFormat { TripleFloat, TripleRGB, HexRGBA, HexRGBMaybeShort }
		public string ToString(ColorStringFormat colorOutput = ColorStringFormat.HexRGBMaybeShort, bool alwaysIncludeAlpha = false) {
			switch (colorOutput) {
			case ColorStringFormat.TripleFloat: return ToTripletFloats(alwaysIncludeAlpha);
			case ColorStringFormat.TripleRGB: return ToTriplet(alwaysIncludeAlpha);
			case ColorStringFormat.HexRGBA: return ToHexString(alwaysIncludeAlpha);
			case ColorStringFormat.HexRGBMaybeShort: return ToHexStringMaybeShort(alwaysIncludeAlpha);
			}
			return ToString();
		}
		public string ToTriplet(bool alwaysIncludeAlpha = false) {
			StringBuilder sb = new StringBuilder();
			sb.Append('(');
			sb.Append(r.ToString()).Append(',').Append(g.ToString()).Append(',').Append(b.ToString());
			if (alwaysIncludeAlpha || a != 255) { sb.Append(',').Append(a.ToString()); }
			sb.Append(')');
			return sb.ToString();
		}
		public string ToTripletFloats(bool alwaysIncludeAlpha = false) {
			StringBuilder sb = new StringBuilder();
			sb.Append('(');
			bool needDecimal = (r == 0 || r == 255) && (g == 0 || g == 255) && (b == 0 || b == 255) && (a == 0 || a == 255);
			string[] c = new string[] { (r / 255f).ToString(), (g / 255f).ToString(), (b / 255f).ToString(), (a / 255f).ToString() };
			// chop off useless precision from colors. anything past the 1000ths place will be meaningless.
			const int idealCharCount = 1 + 3; // decimal point + 3 points of precision
			for (int i = 0; i < c.Length; ++i) {
				if (c[i].StartsWith("0.")) {
					int limit = Math.Min(c[i].Length - 1, idealCharCount); // also chop off the useless leading zero
					c[i] = c[i].Substring(1, limit);
				}
			}
			if (needDecimal) { c[0] += ".0"; }
			sb.Append(c[0]).Append(',').Append(c[1]).Append(',').Append(c[2]);
			if (alwaysIncludeAlpha || a != 255) { sb.Append(',').Append(c[3]); }
			sb.Append(')');
			return sb.ToString();
		}

		/// <param name="text">hex: #aaeeff, short hex: aef, number list: (170, 238, 255). 4th alpha value optional</param>
		/// <param name="color"></param>
		public static bool TryParseString(string text, out ColorRGBA color) {
			color = Magenta;
			if (string.IsNullOrEmpty(text)) return false;
			if (text[0] == '#') { return TryParseString(text.Substring(1), out color); }
			float[] numbers = new float[4];
			bool usingDecimals = false;
			int numbersRead = 0;
			bool readingNumbersInList = false;
			for(int i = 0; i < text.Length && numbersRead < 4; ++i) {
				char c = text[i];
				switch (c) {
				case '(': case '[': case '{': readingNumbersInList = true; break;
				case ')': case ']': case '}': readingNumbersInList = false; break;
				}
				if (readingNumbersInList) {
					int numDigits = text.CountNumericCharactersAt(i, 16, false, true, out int decimalIndex);
					if (numDigits > 0) {
						if (decimalIndex >= 0) { usingDecimals = true; }
						Parse.ParseResult result = StringExtension.NumberParse(text, i, 10, usingDecimals);
						switch (result.replacementValue) {
						case int ni: numbers[numbersRead] = (float)ni; break;
						case long nl: numbers[numbersRead] = (float)nl; break;
						case double nd: numbers[numbersRead] = (float)nd; break;
						default: throw new Exception("failed to parse number: "+result);
						}
						++numbersRead;
						i += result.lengthParsed;
					}
				} else {
					Parse.ParseResult result = StringExtension.NumberParse(text, i, 16, false);
					uint resultNumber;
					switch (result.replacementValue) {
					case int ni: resultNumber = (uint)ni; break;
					case long nl: resultNumber = (uint)nl; break;
					case double nd: resultNumber = (uint)nd; break;
					default: throw new Exception("failed to parse number: " + result);
					}
					color = itocRGBA(resultNumber);
					return true;
				}
			}
			color = Black;
			for(int i = 0; i < numbersRead; ++i) {
				color[i] = (byte)(usingDecimals ? (255 * numbers[i]).Clamp0(255) : numbers[i]);
			}
			return true;
		}

		public static bool TryConvert(object o, out ColorRGBA result) {
			if (o == null) { throw new Exception("cannot convert null to color"); }
			switch (o) {
			case int n: result = itocRGBA((uint)n); return true;
			case uint n: result = itocBGRA((uint)n); return true;
			case byte n: result = itocBGRA((uint)n); return true;
			case short n: result = itocBGRA((uint)n); return true;
			case float n: result = itocBGRA((uint)n); return true;
			case double n: result = itocBGRA((uint)n); return true;
			case string s: result = TryParseString(s, out ColorRGBA c) ? c : Magenta; return true;
			}
			throw new Exception($"cannot convert {o.GetType()} to ColorRGBA");
		}

		public int ManhattanDistance(ColorRGBA other) {
			int dr = Math.Abs((int)r - (int)other.r);
			int dg = Math.Abs((int)g - (int)other.g);
			int db = Math.Abs((int)b - (int)other.b);
			int da = Math.Abs((int)a - (int)other.a);
			return dr + dg + db + da;
		}

		/// <returns>value between 0 and 255, with most weight given to green and least to blue</returns>
		public float GetColorBrightnessEstimate() {
			return ((0.21f * r) + (0.72f * g) + (0.07f * b));
		}
		public static ColorRGBA LightAverage(IList<ColorRGBA> colors, int count) {
			if (count == 1) return colors[0];
			float r = 0, g = 0, b = 0, a = 0;
			for (int i = 0; i < count; ++i) {
				r += colors[i].r * colors[i].r;
				g += colors[i].g * colors[i].g;
				b += colors[i].b * colors[i].b;
				a += colors[i].a * colors[i].a;
			}
			r = (float)Math.Sqrt(r / count);
			g = (float)Math.Sqrt(g / count);
			b = (float)Math.Sqrt(b / count);
			a = (float)Math.Sqrt(a / count);
			return new ColorRGBA((byte)r.Clamp0(255), (byte)g.Clamp0(255), (byte)b.Clamp0(255), (byte)a.Clamp0(255));
		}
	}
}