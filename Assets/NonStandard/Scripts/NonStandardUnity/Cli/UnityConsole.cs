using NonStandard.Data;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UnityConsole : MonoBehaviour
{
    [Tooltip("only render characters contained in the render window")]
    public bool limitToRenderWindow = true;
    public bool cursorVisible = true;
    protected int cursorSize = 1;
    public CoordRect renderWindow = new CoordRect(Coord.Zero, new Coord(20, 5));
    internal ConsoleBody body = new ConsoleBody();
    [Range(0, 1)] public float foregroundAlpha = 1f;
    [Range(0, 1)] public float backgroundAlpha = 0.5f;
    TMP_InputField inputField;
    TMP_Text text;
    TMP_Text charBack;

    public TMP_Text Text => inputField?.textComponent ?? text;
    public char EmptyChar = ' ';
    public char BackgroundChar = '\u2588'; // █
    public char CursorChar = '_';

    List<ColorRGBA> ConsoleColorPalette = new List<ColorRGBA>();

    public int AddConsoleColor(ColorRGBA colorRgba) {
        if (ConsoleColorPalette.Count >= 0xff) return -1;
        ConsoleColorPalette.Add(colorRgba);
        return ConsoleColorPalette.Count - 1;
	}
    public int GetConsoleColorCount() { return ConsoleColorPalette.Count; }
    public ColorRGBA GetConsoleColor(int code) { return ConsoleColorPalette[code]; }

    public void GenerateDefaultPalette() {
        for (int i = 0; i < ConsoleColorPalette.Count; ++i) {
            ConsoleColorPalette[i] = (ConsoleColor)ConsoleColorPalette.Count;
        }
        while (ConsoleColorPalette.Count < 16) { ConsoleColorPalette.Add((ConsoleColor)ConsoleColorPalette.Count); }
	}

    public Coord Cursor => body.Cursor;
    public Coord consoleBodySize;
    /// <summary>
    /// -1 means dynamic
    /// </summary>
    public Coord WindowSize {
        get => limitToRenderWindow ? renderWindow.Size : Coord.NegativeOne;
        set { if (limitToRenderWindow) { renderWindow.Size = value; } }
    }
    public int WindowHeight { get => limitToRenderWindow ? renderWindow.Height : -1; set => renderWindow.Height = value; }
    public int WindowWidth { get => limitToRenderWindow ? renderWindow.Width : -1; set => renderWindow.Width = value; }
    public byte ForegoundColor => body.currentPalette.fore;
    public byte BackgroundColor => body.currentPalette.fore;
    public int BufferHeight => body.Size.Y;
    public int BufferWidth => body.Size.Y;
    public int CursorLeft { get => body.CursorLeft; set => body.CursorLeft = value; }
    public int CursorTop { get => body.CursorTop; set => body.CursorTop = value; }
    public int CursorSize { get => cursorSize; set => cursorSize = value; } // value ignored.
    public bool CursorVisible { get => cursorVisible; set => cursorVisible = value; }
    public void ResetColor() { body.currentPalette = body.startingPalette; }
    void Start() {
        GenerateDefaultPalette();

        inputField = GetComponentInChildren<TMP_InputField>();
		if (!inputField) {
			text = GetComponentInChildren<TMP_Text>();
		} else {
			text = inputField.textComponent;
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
		body.currentPalette.Fore = ConsoleColor.Red;
		Write("World");
		body.currentPalette.Fore = ConsoleColor.Blue;
		Write("!\n");
		body.currentPalette.Fore = ConsoleColor.Gray;
		Write("and now I test the very long\n"+// strings in the command line console. We shall see how the wrapping\n"+
			"and multiple\n"+
			"lines are handled\n"+
			"by this new,\n"+
			"fledgling command-\n"+
			"line implementation\n"+
			".");
		body.Cursor = new Coord(4, 2);
	}
    public void ScrollRenderWindow(Coord direction) {
        renderWindow.Position += direction;
        UpdateRenderWindow();
    }
    public void MoveCursor(Coord direction) {
        bool cursorInWindow = renderWindow.Contains(body.Cursor);
        body.Cursor += direction;
        if (cursorInWindow && !renderWindow.Contains(body.Cursor)) {
            renderWindow.MoveToContain(body.Cursor);
        }
    }
    void UpdateRenderWindow() {
        if(renderWindow.PositionX < 0) {
            renderWindow.PositionX -= renderWindow.PositionX;
        } else if (renderWindow.Right > body.Size.col) {
            if(renderWindow.Width >= body.Size.col) {
                renderWindow.PositionX = 0;
			} else {
                renderWindow.PositionX -= (short)(renderWindow.Right - body.Size.col);
            }
		}
        if (renderWindow.PositionY < 0) {
            renderWindow.PositionY -= renderWindow.PositionY;
        } else if (renderWindow.Bottom > body.Size.row) {
            if (renderWindow.Height >= body.Size.row) {
                renderWindow.PositionY = 0;
            } else {
                renderWindow.PositionY -= (short)(renderWindow.Bottom - body.Size.row);
            }
        }
    }

    public static readonly CoordRect Maximum = new CoordRect(Coord.Zero, Coord.Max);
    public void RefreshText() {
        CoordRect limit = Maximum;
        if(limitToRenderWindow) { limit = renderWindow; }
        CalculateText(body, limit, out string text, out Color32[] colors, true, (byte)(255 * foregroundAlpha));
        Assign(text, colors, true);
        if (charBack) {
            CalculateText(body, limit, out text, out colors, false, (byte)(255* backgroundAlpha));
            Assign(text, colors, false);
        }
        consoleBodySize = body.Size;
    }

    public void Write(string text) {
        Coord oldSize = body.Size;
        body.Write(text);
        renderWindow.MoveToContain(body.Cursor);
        if (body.Size != oldSize) {
            //Show.Log("window update");
            UpdateRenderWindow();
        }
        RefreshText();
    }
    public void WriteLine(string text) { Write(text + "\n"); }

    void CalculateText(ConsoleBody body, CoordRect window, out string text, out Color32[] color, bool foregound, byte alpha) {
        ConsoleTile current = body.startingPalette;
        StringBuilder sb = new StringBuilder();
        Coord limit = new Coord(window.Max.col, Math.Min(window.Max.row, Math.Max(body.lines.Count, body.Cursor.row+1)));
        int rowsPrinted = 0;
        Coord c = body.Cursor;
        List<Color32> colors = new List<Color32>();
        for (int row = window.Min.row; row < limit.row; ++row, ++rowsPrinted) {
            if (rowsPrinted > 0) {
                sb.Append("\n");
                ColorRGBA colorRgba = GetConsoleColor(foregound ? current.fore : current.back);
                colorRgba.a = alpha;
                colors.Add(colorRgba);
            }
            if (row < 0) { continue; }
            if (row < body.lines.Count) {
                List<ConsoleTile> line = body.lines[row];
                limit.col = Math.Min(window.Max.col, (short)line.Count);
                for (int col = window.Min.col; col < limit.col; ++col) {
                    if (col >= 0) {
                        ConsoleTile tile = line[col];
                        current = tile;
                        if (!foregound) { current.Letter = BackgroundChar; }
                    } else if (line.Count > 0) {
                        current.Letter = foregound ? EmptyChar : BackgroundChar;
                    }
                    if (!foregound && cursorVisible && c.col == col && c.row == row) { current.Letter = CursorChar; }
                    sb.Append(current.Letter);
                    ColorRGBA colorRgba = GetConsoleColor(foregound ? current.fore : current.back);
                    colorRgba.a = alpha;
                    colors.Add(colorRgba);
                }
            }
            if (c.row == row && c.col >= limit.col && window.Contains(c)) {
                int col = limit.col;
                ColorRGBA colorRgba = GetConsoleColor(foregound ? current.fore : current.back);
                colorRgba.a = alpha;
                while (col <= c.col) {
                    current.Letter = foregound ? EmptyChar : BackgroundChar;
                    if (!foregound && cursorVisible && c.col == col && c.row == row) { current.Letter = CursorChar; }
                    sb.Append(current.Letter);
                    colors.Add(colorRgba);
                    ++col;
                }
            }
        }
        text = sb.ToString();
        color = colors.ToArray();
    }
    
    public void Assign(string text, Color32[] colors, bool foreground) {
        TMP_Text label;
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
        for (int m = 0; m < label.textInfo.meshInfo.Length; ++m) {
            Color32[] vertColors = label.textInfo.meshInfo[m].colors32;
            for (int i = 0; i < chars.Length; ++i) {
                TMP_CharacterInfo cinfo = chars[i];
                cinfo.xAdvance = 1;
                if (!cinfo.isVisible) continue;
                int vertexIndex = cinfo.vertexIndex;
                if (vertexIndex < vertColors.Length && i < colors.Length) {
                    vertColors[vertexIndex + 0] = colors[i];
                    vertColors[vertexIndex + 1] = colors[i];
                    vertColors[vertexIndex + 2] = colors[i];
                    vertColors[vertexIndex + 3] = colors[i];
                }
            }
        }
        label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
