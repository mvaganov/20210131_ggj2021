using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Cli {
	public class TTYData {
		public string cachedString;
		[HideInInspector]
		/// optimization for GetTMProString. Keeps track of changes to text, not changes to cursor movement
		public bool cacheValid = false;
		/// keeps track of whether or not there is text to display. If clean, this can be assumed to be an empty output.
		public bool isClean = true;
		public bool newLineBehavior_columnSameAsStart = false;
		public char defaultChar = '\0';
		public char emptySpace = ' ';
		public Color defaultForeground = Color.white;
		public Color defaultBackground = Color.clear;
		public ManageUI.InitialColorSettings colorSettings;
		struct ColorSpot { public Color f, b; public ColorSpot(Color f, Color b) { this.f = f; this.b = b; } }
		private int lastWrittenLine;
		/// real time of last change, including changes to the string, and/or the cursor location. color changes have no effect.
		public long timestamp;

		public TTYInput input = new TTYInput();

		// using array of arrays instead of 2D array because the RollConsole method is efficient this way. otherwise, adding a new line can become very, very expensive
		public char[][] buffer;
		ColorSpot[][] colors;
		bool[] lineOverflow;

		public Vector2Int cursorIndex;
		/// when the output is a single string, this is where the i/o cursor is.
		public int cursorIndexInString;
		List<Color> currentF = new List<Color>(), currentB = new List<Color>();

		public TTYData() {
			input.data = this;
		}

		public bool IsClean() { return isClean; }
		public void SetForeground(Color color) {
			currentF.Add(color);
		}
		public void UnsetForeground() {
			if (currentF.Count == 0) return;
			currentF.RemoveAt(currentF.Count-1);
		}
		public void SetBackground(Color color) {
			currentB.Add(color);
		}
		public void UnsetBackground() {
			if (currentB.Count == 0) return;
			currentB.RemoveAt(currentB.Count - 1);
		}
		public Color CurrentForeground() {
			if (currentF.Count == 0) return defaultForeground;
			return currentF[currentF.Count - 1];
		}
		public Color CurrentBackground() {
			if (currentB.Count == 0) return defaultBackground;
			return currentB[currentB.Count - 1];
		}

		public int WriteOutput(string s) {
			int cursorMove = 0;
			for(int i = 0; i < s.Length; ++i) {
				cursorMove += WriteCharOutput(s[i]);
			}
			return cursorMove;
		}
		public int WriteCharOutput(char c) {
			return WriteCharOutput(c, CurrentForeground());
		}
		public int WriteCharOutput(char c, Color f) {
			return WriteCharOutput(c, f, CurrentBackground());
		}
		private void BackspaceAtCursor() {
			BackspaceAtCursor(ref this.cursorIndex);
		}
		private void BackspaceAtCursor(ref Vector2Int cursorIndex) {
			cursorIndex.x--;
			if (cursorIndex.x < 0) {
				if (lineOverflow[cursorIndex.y]) {
					lineOverflow[cursorIndex.y] = false;
				}
				if (cursorIndex.y <= 0) {
					cursorIndex.x = 0;
					cursorIndex.y = 0;
					return;
				}
				cursorIndex.y--;
				bool foundCorrectSpotInPreviousLine = false;
				for (int col = buffer[cursorIndex.y].Length - 1; col >= 0; col--) {
					if (buffer[cursorIndex.y][col] == '\n') {
						foundCorrectSpotInPreviousLine = true;
						cursorIndex.x = col;
					}
				}
				if (!foundCorrectSpotInPreviousLine) {
					cursorIndex.x = buffer[cursorIndex.y].Length - 1;
				}
			}
			Set(cursorIndex.y, cursorIndex.x, defaultChar);
		}
		private void NewlineAtCursor() {
			Set(cursorIndex.y, cursorIndex.x, '\n');
			ClearLine(cursorIndex.y, cursorIndex.x + 1);
			lastWrittenLine++;
			AdvanceLine();
		}
		private void AdvanceLine() {
			AdvanceLine(ref cursorIndex);
		}
		public void AdvanceLine(ref Vector2Int cursorIndex) {
			cursorIndex.y++;
			cursorIndex.x = 0;
			if (cursorIndex.y >= buffer.Length) {
				RollConsole(1);
				cursorIndex.y--;
			}
			InitTimestamp();
		}
		public void RollConsole(int count) {
			RollConsole(count, ref buffer, ref colors, ref lineOverflow);
		}
		private void InitTimestamp() {
			timestamp = System.Environment.TickCount;
		}
		private void RollConsole(int count, ref char[][] a_buffer, ref ColorSpot[][] a_colors, ref bool[] a_lineOverflow) {
			if (count == 0) return;
			if (count < 0) throw new System.Exception("can't roll down "+count);
			char[][] rolled = new char[count][];
			ColorSpot[][] rolledC = new ColorSpot[count][];
			bool[] rolledOverflow = new bool[count];
			for(int i = 0; i < count; ++i) {
				rolled[i] = a_buffer[i];
				rolledC[i] = a_colors[i];
				rolledOverflow[i] = a_lineOverflow[i];
			}
			for(int i = 0; i < a_buffer.Length-count; ++i) {
				a_buffer[i] = a_buffer[i + count];
				a_colors[i] = a_colors[i + count];
				a_lineOverflow[i] = a_lineOverflow[i + count];
			}
			for(int i = 0; i < count; ++i) {
				int row = a_buffer.Length - count + i;
				a_buffer[row] = rolled[i];
				a_colors[row] = rolledC[i];
				a_lineOverflow[row] = false;
				for (int col = 0; col < a_buffer[row].Length; ++col)
				{
					Set(row, col, defaultChar, defaultForeground, defaultBackground, a_buffer, a_colors);
				}
			}
			cacheValid = false;
			InitTimestamp();
		}
		/// <summary>write a character into the commandline console</summary>
		/// <param name="c">character</param>
		/// <param name="f">foreground color</param>
		/// <param name="b">background color</param>
		/// <returns>how much the cursor should advance in the TMPro string</returns>
		public int WriteCharOutput(char c, Color f, Color b) {
			int changed = 0;
			switch (c) {
				case '\b':
					int backspace = 1;
					BackspaceAtCursor();
					changed = - backspace;
					break;
				case '\n':
					int extraSpace = 1;
					NewlineAtCursor();
					changed = extraSpace;
					break;
				case '\r':
					int horizontalPosition = cursorIndex.x;
					cursorIndex.x = 0;
					changed = - horizontalPosition;
					break;
				default:
					Set(cursorIndex.y, cursorIndex.x, c, f, b);
					int lineBeingAdvancedOn = cursorIndex.y;
					if (AdvanceCursor(ref cursorIndex))
					{
						lineOverflow[lineBeingAdvancedOn] = true;
					}
					changed = 1;
					break;
			}
			cacheValid = false;
			InitTimestamp();
			return changed;
		}

		/// <param name="cursorIndex"></param>
		/// <returns>true if the line was advanced vertically (row++)</returns>
		public bool AdvanceCursor(ref Vector2Int cursorIndex) {
			cursorIndex.x++;
			if (cursorIndex.x >= buffer[cursorIndex.y].Length) {
				AdvanceLine(ref cursorIndex);
				return true;
			} else { InitTimestamp(); }
			return false;
		}
		public void PadEmptyWithSpace(int row, int col) {
			for(int i = 0; i < col; ++i) {
				if(buffer[row][i] == defaultChar) {
					buffer[row][i] = emptySpace;
				}
			}
		}
		public void SetCursorIndex(int row, int col) {
			PadEmptyWithSpace(row, col);
			cursorIndex = new Vector2Int(col, row);
			InitTimestamp();
		}
		public bool Set(int row, int col, char c, Color f, Color b) {
			bool changeHappened = Set(row, col, c, f, b, buffer, colors);
			if (changeHappened) {
				cacheValid = false;
				isClean = false;
			}
			if (row > lastWrittenLine) {
				lastWrittenLine = row;
			}
			timestamp = System.Environment.TickCount;
			return changeHappened;
		}
		private static bool Set(int row, int col, char c, Color f, Color b, char[][] buffer, ColorSpot[][] colors)
		{
			bool changeHappened = buffer[row][col] != c || colors[row][col].f != f || colors[row][col].b != b;
			buffer[row][col] = c;
			colors[row][col] = new ColorSpot(f, b);
			//Debug.Log("[" + row + "][" + col + "]:" + buffer[row][col]);
			return changeHappened;
		}

		public void Set(int row, int col, char c, Color f)
		{
			Set(row, col, c, f, CurrentBackground());
		}

		public void Set(int row, int col, char c)
		{
			Set(row, col, c, CurrentForeground());
		}

		public void SetClean(int row, int col)
		{
			Set(row, col, defaultChar, defaultForeground, defaultBackground);
		}

		public void SetSize(int height, int width)
		{
			char[][] buffer = new char[height][];
			ColorSpot[][] colors = new ColorSpot[height][];
			bool[] lineOverflow = new bool[height];
			for (int row = 0; row < height; ++row)
			{
				buffer[row] = new char[width];
				colors[row] = new ColorSpot[width];
				lineOverflow[row] = false;
				for (int col = 0; col < width; ++col)
				{
					if (this.buffer != null && row < this.buffer.Length && col < this.buffer[row].Length)
					{
						Set(row, col, this.buffer[row][col], this.colors[row][col].f, this.colors[row][col].b, buffer, colors);
					} else
					{
						Set(row, col, defaultChar, defaultForeground, defaultBackground, buffer, colors);
					}
				}
				// if we're resizing an existing terminal, and the rest of this row has actual data, it needs to be wordwrapped
				if (this.buffer != null && this.buffer[row].Length > buffer[row].Length)
				{
					System.Text.StringBuilder longString;
					List<ColorSpot> colorSequence;
					// get everything that needs to be wordwrapped
					GetWrappedStringStartingAt(ref row, width, out longString, out colorSequence);
					// TODO
					throw new System.Exception("write wordwrapped string into the commandline");
				}
			}
			// delete this.buffer, this.colors
			this.buffer = buffer;
			this.colors = colors;
			this.lineOverflow = lineOverflow;
			if (cursorIndex.x <= width) { cursorIndex.x = width - 1; }
			if (cursorIndex.y <= height) { cursorIndex.y = height - 1; }
			cacheValid = false;
			InitTimestamp();
		}
		public void ClearLine(int line, int startingWithCharacter = 0) {
			for (int col = startingWithCharacter; col < buffer[line].Length; ++col) {
				SetClean(line, col);
			}
			lineOverflow[cursorIndex.y] = false;
		}
		public void Clear() {
			for(int row =0; row < buffer.Length; ++row) {
				ClearLine(row);
			}
			cursorIndex = new Vector2Int(0, 0);
			cacheValid = true;
			isClean = true;
			InitTimestamp();
		}

		//public void SetText(string text)
		//{
		//	Clear();
		//	// write the 
		//	throw new System.Exception("implement SetText");
		//}
		public void SetColors(ManageUI.InitialColorSettings colors)
		{
			colorSettings = colors;
		}

		private void GetWrappedStringStartingAt(ref int row, int col,
			out System.Text.StringBuilder longString, out List<ColorSpot> colorSequence)
		{
			Vector2Int cursor = new Vector2Int(col, row);
			longString = new System.Text.StringBuilder();
			colorSequence = new List<ColorSpot>();
			int nonEmpty = 0;
			do
			{
				char ch = buffer[cursor.y][cursor.x];
				longString.Append(ch);
				colorSequence.Add(colors[cursor.y][cursor.x]);
				if (ch != defaultChar) { nonEmpty = longString.Length; }
				cursor.x++;
				if (cursor.x >= buffer[cursor.y].Length)
				{
					cursor.x = 0;
					if (!lineOverflow[cursor.y])
					{
						break;
					}
					cursor.y++;
				}
			} while (cursor.y < buffer.Length);
			int howManyToDrop = longString.Length - nonEmpty;
			longString.Remove(nonEmpty, howManyToDrop);
			colorSequence.RemoveRange(nonEmpty, howManyToDrop);
			row = cursor.y;
		}

		public string GetTMProString(string promptArtifact)
		{
			if (cacheValid)
			{
				return cachedString;
			}
			int index;
			cachedString = GetTMProString(out index, promptArtifact);
			cacheValid = true;
			return cachedString;
		}

		public bool GetAt(Vector2Int cursor, out char letter, out Color fore, out Color back, bool includingInput = true, string inputBarrier = "")
		{
			back = colors[cursor.y][cursor.x].b;
			fore = colors[cursor.y][cursor.x].f;
			letter = buffer[cursor.y][cursor.x];
			if(includingInput && input.GetAt(cursor, ref letter, ref fore, ref back, inputBarrier))
			{
				return true;
			}
			return false;
		}

		public string GetTMProString(out int out_cursorStringIndex, string inputBarrier = "", Vector2Int start = default(Vector2Int), int visibleLetterCount = -1)
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			Color currentForegroundColor = defaultForeground, currentBackgroundColor = defaultBackground;
			char letterHere;
			Color foregroundColorHere, backgroundColorHere;
			Vector2Int cursor = start;
			int visibleLetters = 0;
			// figure out which row is the last one that needs to be printed, to probably dramatically reduce the size of the final string
			int lastRowToWorryAbout = lastWrittenLine;
			if(cursorIndex.y+ input.userInputLineExtraLineLocations.Count > lastWrittenLine)
			{
				lastRowToWorryAbout = cursorIndex.y + input.userInputLineExtraLineLocations.Count;
			}
			bool noParseEscaped = false;
			while (cursor.y < buffer.Length && (visibleLetterCount < 0 || visibleLetters < visibleLetterCount))
			{
				bool isUserText = GetAt(cursor, out letterHere, out foregroundColorHere, out backgroundColorHere, true, inputBarrier);
				if(noParseEscaped && !isUserText)
				{
					result.Append("</noparse>");
					noParseEscaped = false;
				}
				if (backgroundColorHere != currentBackgroundColor)
				{
					if (currentBackgroundColor != defaultBackground) { result.Append(BGColorEnd()); }
					currentBackgroundColor = backgroundColorHere;
					if (currentBackgroundColor != defaultBackground) { result.Append(BGColorStart(currentBackgroundColor)); }
				}
				if ((isUserText || letterHere != defaultChar) && foregroundColorHere != currentForegroundColor)
				{
					if (currentForegroundColor != defaultForeground) { result.Append(ColorEnd()); }
					currentForegroundColor = foregroundColorHere;
					if (currentForegroundColor != defaultForeground) { result.Append(ColorStart(currentForegroundColor)); }
				}
				if(isUserText && !noParseEscaped)
				{
					result.Append("<noparse>");
					noParseEscaped = true;
				}
				// get correct cursor index right after invisible color tags
				if (cursor.x == cursorIndex.x && cursor.y == cursorIndex.y) { cursorIndexInString = result.Length; }
				switch (letterHere)
				{
					case '\n': break;
					case '<':
						if (!noParseEscaped)
						{
							result.Append("<noparse><</noparse>");
						} else
						{
							result.Append(letterHere);
						}
						break;
					case '\0': visibleLetterCount--; break;
					default:
						result.Append(letterHere);
						break;
				}
				visibleLetters++;
				cursor.x++;
				// if the cursor is at the end of a string/line, this next line will catch the position correctly before moving the cursor to the next line
				if (cursor.x >= buffer[cursor.y].Length || letterHere == '\n')
				{
					cursor.y++;
					cursor.x = 0;
					// if the cursor is at the end of a string/line, this next line will catch the position correctly before moving the cursor to the next line
					if (cursor.y > lastRowToWorryAbout)
						break;
					result.Append("\n");
				}
				// get correct cursor index if at the end of the console
				if (cursor.x == cursorIndex.x && cursor.y == cursorIndex.y) { cursorIndexInString = result.Length; }
			}
			if (currentForegroundColor != defaultForeground) { result.Append(ColorEnd()); }
			if (currentBackgroundColor != defaultBackground) { result.Append(BGColorEnd()); }
			out_cursorStringIndex = cursorIndexInString + input.userInputCursor + inputBarrier.Length;
			return result.ToString();
		}
		public static string ColorStart(Color c)
		{
			return "<#" + Util.ColorToHexCode(c) + ">";
		}
		public static string ColorEnd()
		{
			return "</color>";
		}
		public static string BGColorStart(Color c)
		{
			return "<mark=#" + Util.ColorToHexCode(c) + ">";
		}
		public static string BGColorEnd()
		{
			return "</mark>";
		}

		public static Vector2Int CalculateCoordinateOf(string text, int cursorIndex)
		{
			int lineBegin = 0, lineEnd = -1;
			Vector2Int coord = new Vector2Int();
			do
			{
				lineEnd = text.IndexOf('\n', lineBegin);
				if(lineEnd == -1) { lineEnd = text.Length; }
				if(lineBegin <= cursorIndex && lineEnd >= cursorIndex)
				{
					//Debug.Log("found " + cursorIndex + " at line " + coord.y);
					coord.x = VisibleCharactersInLine(text, lineBegin, lineEnd);
					return coord;
				}
				lineBegin = lineEnd + 1;
				coord.y++;
			} while (lineBegin < text.Length);
			//Debug.Log(cursorIndex+"was not in string length "+text.Length);
			return coord;
		}

		/// <summary>Count how many visibles characters are in the given line (with the given start & end points).</summary>
		/// <returns>The characters in line if maxCount is less than 0. 
		/// Otherwise, the index where the maxCount visible character was printed, or -1 if there arent enough characters.</returns>
		/// <param name="text">Text.</param>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="maxCount">Max count.</param>
		public static int VisibleCharactersInLine(string text, int start, int end, int maxCount = -1)
		{
			int count = 0;
			for (int i = start; i < end; ++i)
			{
				char c = text[i];
				switch (c)
				{
					case '\0': case '\a': case '\b': case '\t': case '\n': case '\r': // @debug
						throw new System.Exception("VisibleCharactersInLine should not have to deal with char " + (int)c + ".");
					case '<':
						int tend = text.IndexOf('>', i + 1);
						int tendalt = text.IndexOf(' ', i + 1);
						if (tendalt < 0) tendalt = text.Length;
						int tokenEnd = System.Math.Min(tend, tendalt);
						string tagname = text.Substring(i + 1, tokenEnd-i-1);
						// move cursor past color tags, any ending tags (possible bug?), or any known tag. assume others will be shown.
						if (tagname.StartsWith("#") || tagname.StartsWith("/") || System.Array.IndexOf(Util.tagsTMP, tagname) >= 0)
						{
							i = tend;
						} else { count++; }
						break;
					default:
						count++;
						break;
				}
				if (maxCount >= 0 && count > maxCount)
				{
					return i;
				}
			}
			if (maxCount >= 0)
			{
				return -1;
			}
			return count;
		}
	}
}