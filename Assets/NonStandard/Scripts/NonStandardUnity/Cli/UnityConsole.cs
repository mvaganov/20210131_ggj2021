using NonStandard;
using NonStandard.Data;
using NonStandard.Procedure;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UnityConsole : MonoBehaviour
{
    public CoordRect bodyWindow = new CoordRect(Coord.Zero, new Coord(20, 5));
    ConsoleBody body = new ConsoleBody();
    public TMPro.TMP_InputField inputField;
    public Coord Cursor;
    void Start()
    {
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
    }

	private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            body.Write("TEXT!");
            Cursor = body.Cursor;
            RefreshText();
        }
        else if (UpdateKey() || body.Cursor != Cursor) {
            body.Cursor = Cursor;
            RefreshText();
        }
        a = inputField.caretPosition;
        b = inputField.characterLimit;
        c = inputField.lineLimit;
        d = inputField.selectionAnchorPosition;
        e = inputField.selectionFocusPosition;
        f = inputField.selectionStringAnchorPosition;
        g = inputField.selectionStringFocusPosition;
    }

    public bool UpdateKey() {
        Coord move = Coord.Zero;
        if (Input.GetKey(KeyCode.LeftArrow)) { move = Coord.Left; }
        if (Input.GetKey(KeyCode.RightArrow)) { move = Coord.Right; }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { move = Coord.Up; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { move = Coord.Down; }
        if (move != Coord.Zero) {
            Scroll(move);
            return true;
		}
        return false;
    }

    public void Scroll(Coord direction) {
        bodyWindow.Position += direction;
        if(bodyWindow.PositionX < 0) {
            bodyWindow.PositionX -= bodyWindow.PositionX;
        } else if (bodyWindow.Right > body.Size.col) {
            if(bodyWindow.Width >= body.Size.col) {
                bodyWindow.PositionX = 0;
			} else {
                bodyWindow.PositionX -= (short)(bodyWindow.Right - body.Size.col);
            }
		}
        if (bodyWindow.PositionY < 0) {
            bodyWindow.PositionY -= bodyWindow.PositionY;
        } else if (bodyWindow.Bottom > body.Size.row) {
            if (bodyWindow.Height >= body.Size.row) {
                bodyWindow.PositionY = 0;
            } else {
                bodyWindow.PositionY -= (short)(bodyWindow.Bottom - body.Size.row);
            }
        }
    }
    public int a, b, c, d, e, f, g;
    public void RefreshText() {
        string text = ConvertToTMPro(body, out int cursorIndex, bodyWindow, false);
        inputField.text = text;
        //Show.Log("~~~" + cursorIndex+ " "+body.Cursor);
        //Proc.Enqueue(() => {
        inputField.ActivateInputField();
        if (cursorIndex >= 0) {
            inputField.selectionStringAnchorPosition = cursorIndex;
            inputField.selectionStringFocusPosition = cursorIndex + 1;
        } else {
            inputField.selectionStringAnchorPosition = text.Length;
            inputField.selectionStringFocusPosition = text.Length;
        }
        //inputField.Select();
        //});
    }

    public void Write(string text) {
        body.Write(text);
        RefreshText();
    }

    public static string ConvertToTMPro(ConsoleBody body, out int cursorIndex) {
        return ConvertToTMPro(body, out cursorIndex, new CoordRect(Coord.Zero, Coord.Max));
    }
    public static string ConvertToTMPro(ConsoleBody body, out int cursorIndex, CoordRect window, bool correctlyCloseColorTags = true) {
        bool colorSet = false;
        ConsoleTile current = body.startingPalette;
        StringBuilder sb = new StringBuilder();
        Coord limit = new Coord(window.Max.col, Math.Min(window.Max.row, body.lines.Count));
        int rowsPrinted = 0;
        cursorIndex = -1;
        for (int row = window.Min.row; row < limit.row; ++row, ++rowsPrinted) {
            if (rowsPrinted > 0) {
                short lastRow = (short)(row - 1);
                short lastCol = (short)body.lines[lastRow].Count;
                Coord lastCoord = new Coord(lastCol, lastRow);
                if (body.Cursor == lastCoord && window.Contains(lastCoord)) {
                    sb.Append(" ");
                    cursorIndex = sb.Length - 1;
                }
                sb.Append("\n");
            }
            if (row < 0) { continue; }
            List<ConsoleTile> line = body.lines[row];
            limit.col = Math.Min(window.Max.col, (short)line.Count);
            for (int col = window.Min.col; col < limit.col; ++col) {
                if (col >= 0) {
                    ConsoleTile tile = line[col];
                    if (current.fore != tile.fore || !colorSet) {
                        if (correctlyCloseColorTags && colorSet) { sb.Append("</color>"); }
                        sb.Append("<#" + ((ColorRGBA)tile.Fore).ToHexStringMaybeShort() + ">");
                        colorSet = true;
                    }
                    current = tile;
                } else if (line.Count > 0) {
                    current.Letter = ' ';
                }
                sb.Append(current.Letter);
                if (body.Cursor == new Coord(col, row)) {
                    cursorIndex = sb.Length - 1;
                }
            }
        }
        if (correctlyCloseColorTags) { sb.Append("</color>"); }
        return sb.ToString();
	}
}
