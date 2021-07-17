using NonStandard.Data;
using System.Collections.Generic;
using NonStandard.Extension;
using System;

public class ConsoleBody {
	public int spacesPerTab = 4;
	public ConsoleTile startingPalette = new ConsoleTile(' ', System.ConsoleColor.Gray, System.ConsoleColor.Black);
	public ConsoleTile currentPalette = new ConsoleTile(' ', System.ConsoleColor.Gray, System.ConsoleColor.Black);
	public System.ConsoleColor[] unprintableColors = new System.ConsoleColor[] {
		System.ConsoleColor.DarkGray, System.ConsoleColor.DarkCyan, System.ConsoleColor.DarkMagenta, System.ConsoleColor.DarkYellow,
		System.ConsoleColor.DarkRed, System.ConsoleColor.DarkGreen, System.ConsoleColor.DarkBlue,
	};
	/// <summary>
	/// allows exceptions to be made for specific otherwise unprintable characters to be printed
	/// </summary>
	public Dictionary<char,char> printableCharacters = new Dictionary<char, char>();
	public List<List<ConsoleTile>> lines = new List<List<ConsoleTile>>();
	protected Coord writeCursor;
	protected Coord size;
	public bool CursorAllowedInEmptyAreaInRow = false;
	public Coord Cursor {
		get => writeCursor;
		set {
			writeCursor = value;
			Coord limit = size;
			writeCursor.row = writeCursor.row.Clamp((short)0, (short)(limit.row-1));
			if (!CursorAllowedInEmptyAreaInRow) { limit.col = (short)(writeCursor.row < lines.Count ? lines[writeCursor.row].Count : 0); }
			writeCursor.col = writeCursor.col.Clamp((short)0, limit.col);
		}
	}
	public int CursorLeft { get => writeCursor.X; set => writeCursor.X = value; }
	public int CursorTop { get => writeCursor.Y; set => writeCursor.Y = value; }
	public Coord Size {
		get => size;
	}
	public void Clear() {
		Cursor = Coord.Zero;
		size = Coord.Zero;
		lines.Clear();
	}
	public void Write(string text) {
		List<ConsoleTile> line;
		for (int i = 0; i < text.Length; ++i) {
			char c = text[i];
			bool printC = true;
			switch (c) {
			case '\b':
				line = lines[writeCursor.row];
				bool needToRediscover = false;
				//Show.Log(writeCursor.col+" "+ line.Count + "         max:"+size.col);
				if (writeCursor.col == line.Count) {
					needToRediscover = writeCursor.col+1 >= size.col;
					printC = false; // don't print, that will add a character, we're removing a character
					if (line.Count == 0) {
						if (writeCursor.row == lines.Count - 1) {
							lines.RemoveAt(lines.Count - 1);
							size.row = (short)lines.Count;
						}
					} else {
						line.RemoveAt(line.Count - 1);
					}
				}
				--writeCursor.col;
				if (needToRediscover) {
					size.x = CalculateWidth();
				}
				while (writeCursor.col < 0) {
					if (writeCursor.row <= 0) { writeCursor.col = writeCursor.row = 0; break; }
					--writeCursor.row;
					line = lines[writeCursor.row];
					writeCursor.col += (short)(line.Count+1);
					printC = false; // don't print, that will add a character to the end of the previous line
				} break;
			case '\n':
				++writeCursor.row;
				writeCursor.col = 0;
				printC = false; // don't print, that will add a character to the end of the line
				break;
			}
			while (writeCursor.row >= lines.Count) { lines.Add(new List<ConsoleTile>()); }
			if (printC) {
				ConsoleTile thisLetter = GetPrintable(c, out short letterWidth);
				line = lines[writeCursor.row];
				while (writeCursor.col + letterWidth > line.Count) { line.Add(currentPalette); }
				line[writeCursor.col] = thisLetter;
				writeCursor.col += letterWidth;
			}
			if (writeCursor.col >= size.col) { size.col = (short)(writeCursor.col+1); }
		}
		size.row = (short)Math.Max(lines.Count, Cursor.row+1);
	}
	public int CalculateWidth() {
		int w = 0;
		for (int r = 0; r < lines.Count; ++r) {
			if (lines[r].Count >= w) {
				w = lines[r].Count;
			}
		}
		return w;
	}
	public bool MoveCursor(Coord direction) {
		Coord next = Cursor + direction;
		if (next.x < 0 || next.y < 0 || next.y >= size.y || next.x >= size.x) { return false; }
		Cursor = next;
		return true;
	}
	public ConsoleTile GetPrintable(char c, out short letterWidth) {
		ConsoleTile thisLetter = currentPalette;
		switch (c) {
		case '\b': thisLetter.Set(' ', currentPalette.Fore, currentPalette.Back);
			letterWidth = 0;
			return thisLetter;
		case '\t':
			thisLetter.Set('t', currentPalette.Back, currentPalette.Fore);
			letterWidth = (short)(spacesPerTab - (writeCursor.col % spacesPerTab));
			return thisLetter;
		default:
			letterWidth = 1;
			if (c >= 32 && c < 128) {
				thisLetter.Letter = c;
			} else {
				if (printableCharacters.TryGetValue(c, out char printableAs)) {
					thisLetter.Letter = printableAs;
				} else {
					int weirdColor = (c / 32) % unprintableColors.Length;
					thisLetter.Letter = CharExtension.ConvertToHexadecimalPattern(c);
					thisLetter.Fore = unprintableColors[weirdColor];
				}
			}
			return thisLetter;
		}
	}
	public ConsoleTile GetAt(Coord cursor) {
		if (cursor.row < 0 || cursor.row >= lines.Count || cursor.col < 0) {
			return currentPalette.CloneWithLetter('\0');
		}
		List<ConsoleTile> line = lines[cursor.row];
		if (cursor.col >= line.Count) {
			return currentPalette.CloneWithLetter('\0');
		}
		return line[cursor.col];
	}
	public void Draw(CoordRect location, Coord offset) {
		location.ForEach(c => {
			ConsoleTile tile = GetAt(c - offset);
			c.SetConsoleCursorPosition();
			tile.Write();
		});
	}
}
