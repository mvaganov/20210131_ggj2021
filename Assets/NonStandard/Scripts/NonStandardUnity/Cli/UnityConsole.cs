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
    public CoordRect renderWindow = new CoordRect(Coord.Zero, new Coord(20, 5));
    ConsoleBody body = new ConsoleBody();
    [Range(0, 1)] public float foregroundAlpha = 1f;
    [Range(0, 1)] public float backgroundAlpha = 0.5f;
    TMP_InputField inputField;
    TMP_Text text;
    TMP_Text charBack;

    public TMP_Text Text => inputField?.textComponent ?? text;
    public char EmptyChar = ' ';
    public char BackgroundChar = '\u2588'; // █

    public Coord Cursor => body.Cursor;

    void Start()
	{
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
		Write("and now I test the very long strings in the command line console. We shall see how the wrapping\n"+
			"and multiple\n"+
			"lines are handled\n"+
			"by this new,\n"+
			"fledgling command-\n"+
			"line implementation\n"+
			".");
		body.Cursor = new Coord(4, 2);
	}

	private void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            body.Write("TEXT!");
            RefreshText();
        }
        else if (UpdateKey()) {
            RefreshText();
        }
    }

	public bool UpdateKey() {
        Coord move = Coord.Zero;
        if (Input.GetKey(KeyCode.LeftArrow)) { move = Coord.Left; }
        if (Input.GetKey(KeyCode.RightArrow)) { move = Coord.Right; }
        if (Input.GetKey(KeyCode.UpArrow)) { move = Coord.Up; }
        if (Input.GetKey(KeyCode.DownArrow)) { move = Coord.Down; }
        if (move != Coord.Zero) {
            ScrollRenderWindow(move);
            return true;
		}
        return false;
    }

    public void ScrollRenderWindow(Coord direction) {
        renderWindow.Position += direction;
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
        GetPrintText(body, limit, out string text, out Color32[] colors, true, EmptyChar, '\0', (byte)(255 * foregroundAlpha));
        Assign(text, colors, true);
        if (charBack) {
            GetPrintText(body, limit, out text, out colors, false, BackgroundChar, '_', (byte)(255* backgroundAlpha));
            Assign(text, colors, false);
        }
    }

    public void Write(string text) {
        body.Write(text);
        RefreshText();
    }

    public static void GetPrintText(ConsoleBody body, CoordRect window, out string text, out Color32[] color, bool useForeground, char emptyChar, char cursorChar, byte alpha) {
        ConsoleTile current = body.startingPalette;
        StringBuilder sb = new StringBuilder();
        Coord limit = new Coord(window.Max.col, Math.Min(window.Max.row, body.lines.Count));
        int rowsPrinted = 0;
        Coord c = body.Cursor;
        List<Color32> colors = new List<Color32>();
        for (int row = window.Min.row; row < limit.row; ++row, ++rowsPrinted) {
            if (rowsPrinted > 0) {
                sb.Append("\n");
                colors.Add((ColorRGBA)(useForeground?current.Fore:current.Back));
            }
            if (row < 0) { continue; }
            List<ConsoleTile> line = body.lines[row];
            limit.col = Math.Min(window.Max.col, (short)line.Count);
            for (int col = window.Min.col; col < limit.col; ++col) {
                if (col >= 0) {
                    ConsoleTile tile = line[col];
                    current = tile;
					if (!useForeground) { current.Letter = emptyChar; }
                } else if (line.Count > 0) {
                    current.Letter = emptyChar;
                }
                if(cursorChar != '\0' && c.col == col && c.row == row) { current.Letter = cursorChar; }
                sb.Append(current.Letter);
                ColorRGBA colorRgba = useForeground ? current.Fore : current.Back;
                colorRgba.a = alpha;
                colors.Add(colorRgba);
            }
            if (c.row == row && c.col > limit.col && window.Contains(c)) {
                int col = limit.col;
                while(col <= c.col) {
                    current.Letter = emptyChar;
                    if (cursorChar != '\0' && c.col == col && c.row == row) { current.Letter = cursorChar; }
                    sb.Append(current.Letter);
                    ColorRGBA colorRgba = useForeground ? current.Fore : current.Back;
                    colorRgba.a = alpha;
                    colors.Add(colorRgba);
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
