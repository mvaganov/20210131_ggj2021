using NonStandard.Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NonStandard.Extension;

namespace NonStandard.Cli {
	public class UnityConsole : MonoBehaviour {
		public TMP_InputField inputField;
		TMP_Text text;
		TMP_Text charBack;
		[System.Serializable] public class CursorSettings {
			public bool cursorVisible = true;
			public GameObject cursor;
			public Coord position;
			internal int index;
			Vector3[] cursorMeshPosition = new Vector3[4];
			public Vector3 CalculateCursorPosition() {
				return (cursorMeshPosition[0] + cursorMeshPosition[1] + cursorMeshPosition[2] + cursorMeshPosition[3]) / 4;
			}
			public void RefreshCursorPosition(UnityConsole console) {
				if (cursor == null) return;
				if (cursorVisible && index >= 0) {
					Transform t = cursor.transform;
					Vector3 p = CalculateCursorPosition();
					t.localPosition = p;
					t.rotation = console.transform.rotation;
					cursor.SetActive(true);
				} else {
					cursor.SetActive(false);
				}
			}
			internal void SetCursorPositionPoints(Vector3[] verts, int vertexIndex) {
				cursorMeshPosition[0] = verts[vertexIndex + 0];
				cursorMeshPosition[1] = verts[vertexIndex + 1];
				cursorMeshPosition[2] = verts[vertexIndex + 2];
				cursorMeshPosition[3] = verts[vertexIndex + 3];
			}
		}
		public CursorSettings cursor = new CursorSettings();
		[System.Serializable] public class DisplayWindowSettings {
			public Coord bufferSize;
			public static readonly CoordRect Maximum = new CoordRect(Coord.Zero, Coord.Max);
			[Tooltip("only render characters contained in the render window")]
			public bool UseWindow = true;
			public CoordRect rect = new CoordRect(Coord.Zero, new Coord(20, 5));
			public enum FollowBehavior { None, Yes }
			public FollowBehavior followCursor = FollowBehavior.Yes;
			public CoordRect Limit => UseWindow ? rect : Maximum;
			public Coord WindowSize {
				get => UseWindow ? rect.Size : Maximum.Size;
				set { rect.Size = value; }
			}
			public int WindowHeight { get => UseWindow ? rect.Height : -1; set => rect.Height = value; }
			public int WindowWidth { get => UseWindow ? rect.Width : -1; set => rect.Width = value; }
			internal void UpdateRenderWindow(ConsoleBody body) {
				if (rect.PositionX < 0) { rect.PositionX -= rect.PositionX; }
				else if (rect.Right > body.Size.col) {
					if (rect.Width >= body.Size.col) { rect.PositionX = 0; }
					else { rect.PositionX -= (short)(rect.Right - body.Size.col); }
				}
				if (rect.PositionY < 0) { rect.PositionY -= rect.PositionY; }
				else if (rect.Bottom > body.Size.row) {
					if (rect.Height >= body.Size.row) { rect.PositionY = 0; }
					else { rect.PositionY -= (short)(rect.Bottom - body.Size.row); }
				}
			}
			public void ScrollRenderWindow(Coord direction, ConsoleBody body) {
				rect.Position += direction;
				UpdateRenderWindow(body);
			}
		}
		public DisplayWindowSettings window = new DisplayWindowSettings();
		public void ScrollRenderWindow(Coord direction) {
			window.ScrollRenderWindow(direction, body);
			textNeedsRefresh = true;
		}
		internal ConsoleBody body = new ConsoleBody();
		[System.Serializable] public class ColorSettings {
			[Range(0, 1)] public float foregroundAlpha = 1f;
			[Range(0, 1)] public float backgroundAlpha = 0.5f;
			public List<Color> ConsoleColorPalette = new List<Color>(Array.ConvertAll(ColorRGBA.defaultColors, c => (Color)c));
			public void FillInDefaultPalette() {
				while (ConsoleColorPalette.Count < 16) {
					ConsoleColorPalette.Add((Color)(ColorRGBA)(ConsoleColor)ConsoleColorPalette.Count);
				}
			}
			public int AddConsoleColor(ColorRGBA colorRgba) {
				if (ConsoleColorPalette.Count >= 0xff) {
					Show.Error("too many colors");
					return -1;
				}
				ConsoleColorPalette.Add(colorRgba);
				return ConsoleColorPalette.Count - 1;
			}
		}
		public ColorSettings colorSettings = new ColorSettings();

		public float FontSize {
			get => inputField.pointSize;
			set { inputField.pointSize = charBack.fontSize = value; }
		}
		public void AddToFontSize(float value) {
			FontSize += value;
			if (FontSize < 1) { FontSize = 1; }
		}

		[System.Serializable] public class CharSettings {
			public char EmptyChar = ' ';
			public char BackgroundChar = '\u2588'; // █
		}
		public CharSettings charSettings = new CharSettings();

		public int AddConsoleColor(ColorRGBA colorRgba) { return colorSettings.AddConsoleColor(colorRgba); }
		public int GetConsoleColorCount() { return colorSettings.ConsoleColorPalette.Count; }
		public ColorRGBA GetConsoleColor(int code, bool foreground) {
			if(code == Col.DefaultColorIndex) { code = foreground ? body.defaultColors.fore : body.defaultColors.back; }
			//if(code < 0 || code >= colorSettings.ConsoleColorPalette.Count) {
			//	Show.Log("wtf is " + code + "? max is "+ colorSettings.ConsoleColorPalette.Count);
			//}
			return colorSettings.ConsoleColorPalette[code];
		}
		public Coord Cursor {
			get => body.Cursor;
			set {
				//bool cursorInWindow = window.rect.Contains(body.Cursor);
				body.Cursor = value;
				cursor.position = body.Cursor;
				//if (cursorInWindow && !window.rect.Contains(body.Cursor)) {
				if (window.followCursor == DisplayWindowSettings.FollowBehavior.Yes) {
					window.rect.MoveToContain(body.Cursor);
				}
				cursor.RefreshCursorPosition(this);
				textNeedsRefresh = true;
			}
		}

		/// <summary>
		/// -1 means dynamic
		/// </summary>
		public Coord WindowSize { get => window.WindowSize; set => window.WindowSize = value; }
		public int WindowHeight { get => window.WindowHeight; set => window.WindowHeight = value; }
		public int WindowWidth { get => window.WindowWidth; set => window.WindowWidth= value; }
		public ConsoleColor ForegoundColor { get => body.currentColors.Fore; set => body.currentColors.Fore = value; }
		public ConsoleColor BackgroundColor { get => body.currentColors.Back; set => body.currentColors.Back = value; }
		public byte ForeColor { get => body.currentColors.fore; set => body.currentColors.fore = value; }
		public byte BackColor { get => body.currentColors.back; set => body.currentColors.back = value; }
		public int BufferHeight => window.bufferSize.Y;
		public int BufferWidth => window.bufferSize.X;
		public int CursorLeft { get => body.CursorLeft; set => body.CursorLeft = value; }
		public int CursorTop { get => body.CursorTop; set => body.CursorTop = value; }
		public int CursorSize {
			get { return (int)(cursor.cursor.transform.localScale.MagnitudeManhattan() / 3); }
			set { cursor.cursor.transform.localScale = Vector3.one * (value / 100f); }
		}
		public bool CursorVisible { get => cursor.cursorVisible; set => cursor.cursorVisible = value; }

		public void PushForeColor(ConsoleColor c) { _colorStack.Add(ForeColor); ForegoundColor = c; }
		public void PushForeColor(byte c) { _colorStack.Add(ForeColor); ForeColor = c; }
		public void PopForeColor() {
			if (_colorStack.Count > 0) {
				ForeColor = _colorStack[_colorStack.Count-1];
				_colorStack.RemoveAt(_colorStack.Count - 1);
			}
		}
		private List<byte> _colorStack = new List<byte>();

		public struct Tile {
			public Color32 color;
			public float height;
			public char letter;
			public Tile(char letter, Color32 color, float height) { this.letter = letter;this.height = height;this.color = color; }
		}

		public void ResetColor() { body.currentColors = body.defaultColors; }
		private void Awake() {
			colorSettings.FillInDefaultPalette();
		}

		public TMP_Text Text => inputField?.textComponent ?? text;

		void Start() {
			if (inputField == null) { inputField = GetComponentInChildren<TMP_InputField>(); }
			if (!inputField) {
				text = GetComponentInChildren<TMP_Text>();
			} else {
				text = inputField.textComponent;
				inputField.readOnly = true;
				inputField.richText = false;
			}
			TMP_Text pTmp = Text;
			GameObject backgroundObject = Instantiate(Text.gameObject);
			UnityConsole extra = backgroundObject.GetComponent<UnityConsole>();
			if (extra != null) { DestroyImmediate(extra); }
			RectTransform brt = backgroundObject.GetComponent<RectTransform>() ?? backgroundObject.AddComponent<RectTransform>();
			backgroundObject.transform.SetParent(pTmp.transform);
			backgroundObject.transform.localPosition = Vector3.zero;
			backgroundObject.transform.localScale = Vector3.one;
			charBack = backgroundObject.GetComponent<TMP_Text>();
			//charBack.geometrySortingOrder = (VertexSortingOrder)(-1);
			charBack.fontMaterial.renderQueue -= 1;
			if (inputField) {
				inputField.targetGraphic.material.renderQueue -= 2;
			}
			RectTransform rt = pTmp.GetComponent<RectTransform>();
			brt.anchorMin = rt.anchorMin;
			brt.anchorMax = rt.anchorMax;
			brt.offsetMin = rt.offsetMin;
			brt.offsetMax = rt.offsetMax;

			Write("Hello ");
			body.currentColors.Fore = ConsoleColor.Red;
			Write("World");
			body.currentColors.Fore = ConsoleColor.Blue;
			Write("!\n");
			body.currentColors.Fore = ConsoleColor.Gray;
			Write("and now I test the very long\n" +// strings in the command line console. We shall see how the wrapping\n"+
				"and multiple\n" +
				"lines are handled\n" +
				"by this new,\n" +
				"fledgling command-\n" +
				"line implementation\n" +
				".");
		}
		public void Update() {
			if(cursor.position != body.Cursor) {
				Cursor = cursor.position;
			}
			if (textNeedsRefresh) {
				RefreshText();
			}
		}

		List<Tile> foreTile = new List<Tile>(), backTile = new List<Tile>();
		private bool textNeedsRefresh = false;
		public void RefreshText() {
			CoordRect limit = window.Limit;
			CalculateText(body, limit, foreTile, true, colorSettings.foregroundAlpha);
			TransferToTMP(true, foreTile);
			if (charBack) {
				CalculateText(body, limit, backTile, false, colorSettings.backgroundAlpha);
				TransferToTMP(false, backTile);
			}
			cursor.RefreshCursorPosition(this);
			textNeedsRefresh = false;
			//Show.Log(body.Cursor);
		}

		public void Write(char c) { Write(c.ToString()); }
		public void Write(object o) { Write(o.ToString()); }
		public void Write(string text) {
			Coord oldSize = body.Size;
			body.Write(text);
			Cursor = body.Cursor;
			//window.rect.MoveToContain(body.Cursor);
			if (body.Size != oldSize) {
				//Show.Log("window update");
				window.UpdateRenderWindow(body);
			}
			textNeedsRefresh = true;
		}
		public void WriteLine(string text) { Write(text + "\n"); }
		void CalculateText(ConsoleBody body, CoordRect window, List<Tile> out_tile, bool foreground, float alpha) {
			out_tile.Clear();
			ConsoleTile current = body.defaultColors;
			Coord limit = new Coord(window.Max.col, Math.Min(window.Max.row, Math.Max(body.lines.Count, body.Cursor.row + 1)));
			int rowsPrinted = 0;
			Coord cursor = body.Cursor;
			this.cursor.index = -1;
			for (int row = window.Min.row; row < limit.row; ++row, ++rowsPrinted) {
				if (rowsPrinted > 0) {
					ColorRGBA colorRgba = GetConsoleColor(foreground ? current.fore : current.back, foreground);
					colorRgba.a = (byte)(colorRgba.a * alpha);
					out_tile.Add(new Tile('\n', colorRgba, 0));
				}
				if (row < 0) { continue; }
				if (row < body.lines.Count) {
					List<ConsoleTile> line = body.lines[row];
					limit.col = Math.Min(window.Max.col, (short)line.Count);
					for (int col = window.Min.col; col < limit.col; ++col) {
						if (col >= 0) {
							ConsoleTile tile = line[col];
							current = tile;
							if (!foreground) { current.Letter = charSettings.BackgroundChar; }
						} else if (line.Count > 0) {
							current.Letter = foreground ? charSettings.EmptyChar : charSettings.BackgroundChar;
						}
						if (!foreground && this.cursor.cursorVisible && cursor.col == col && cursor.row == row) {
							this.cursor.index = out_tile.Count;
						}
						ColorRGBA colorRgba = GetConsoleColor(foreground ? current.fore : current.back, foreground);
						colorRgba.a = (byte)(colorRgba.a * alpha);
						out_tile.Add(new Tile(current.Letter, colorRgba, 0));
					}
				}
				if (cursor.row == row && cursor.col >= limit.col && window.Contains(cursor)) {
					int col = limit.col;
					ColorRGBA colorRgba = GetConsoleColor(foreground ? current.fore : current.back, foreground);
					colorRgba.a = (byte)(colorRgba.a * alpha);
					while (col <= cursor.col) {
						current.Letter = foreground ? charSettings.EmptyChar : charSettings.BackgroundChar;
						if (!foreground && this.cursor.cursorVisible && cursor.col == col && cursor.row == row) {
							this.cursor.index = out_tile.Count;
						}
						out_tile.Add(new Tile(current.Letter, colorRgba, 0));
						++col;
					}
				}
			}
		}

		public void TransferToTMP(bool foreground, List<Tile> tiles) {
			TMP_Text label;
			char[] letters = new char[tiles.Count];
			for(int i = 0; i < letters.Length; ++i) { letters[i] = tiles[i].letter; }
			string text = new string(letters);
			if (foreground) {
				if (inputField) {
					inputField.text = text;
					inputField.ForceLabelUpdate();
					label = inputField.textComponent;
				} else {
					label = this.text;
					label.text = text;
					label.ForceMeshUpdate();
				}
			} else {
				label = charBack;
				label.text = text;
				label.ForceMeshUpdate();
			}
			TMP_CharacterInfo[] chars = label.textInfo.characterInfo;
			Vector3 normal = -transform.forward;
			Color32 color;
			float height;
			bool vertChange = false, colorChange = false;
			for (int m = 0; m < label.textInfo.meshInfo.Length; ++m) {
				Color32[] vertColors = label.textInfo.meshInfo[m].colors32;
				Vector3[] verts = label.textInfo.meshInfo[m].vertices;
				for (int i = 0; i < chars.Length; ++i) {
					TMP_CharacterInfo cinfo = chars[i];
					cinfo.xAdvance = 1;
					if (!cinfo.isVisible) continue;
					int vertexIndex = cinfo.vertexIndex;
					if(i == cursor.index) {
						cursor.SetCursorPositionPoints(verts, vertexIndex);
					}
					if (vertexIndex < vertColors.Length && i < tiles.Count && !vertColors[vertexIndex].Eq(color = tiles[i].color)) {
						colorChange = true;
						vertColors[vertexIndex + 0] = color;
						vertColors[vertexIndex + 1] = color;
						vertColors[vertexIndex + 2] = color;
						vertColors[vertexIndex + 3] = color;
					}
					if(vertexIndex < vertColors.Length && i < tiles.Count && (height = tiles[i].height) != 0) {
						vertChange = true;
						Vector3 h = height * normal;
						verts[vertexIndex + 0] = verts[vertexIndex + 0] + h;
						verts[vertexIndex + 1] = verts[vertexIndex + 1] + h;
						verts[vertexIndex + 2] = verts[vertexIndex + 2] + h;
						verts[vertexIndex + 3] = verts[vertexIndex + 3] + h;
					}
				}
			}
			if (colorChange) { label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32); }
			if (vertChange) { label.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); }
		}
	}
}