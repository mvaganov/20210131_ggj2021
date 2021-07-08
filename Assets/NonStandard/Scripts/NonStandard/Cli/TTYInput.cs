using MazeGeneration;
using NonStandard.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Cli {
	public class TTYInput {
		/// raw text input that the user has entered
		public System.Text.StringBuilder userInput = new System.Text.StringBuilder();
		/// where in the output area the different lines of user input are located
		public List<Coord> userInputLineExtraLineLocations = new List<Coord>();
		/// where in the userInput the user is currently writing to (because the user can insert text into the input stream)
		public int userInputCursor;
		/// object that manages TTY output
		public TTYData data;

		/// <param name="index"></param>
		/// <returns>which index if the userInput string a given line starts at (which 
		/// is seperate from where each line starts *in 2D space in the display*, which 
		/// can be found in the lookup table userInputLineExtraLineLocations</returns>
		private int GetIndexOfUserInputLine(int lineIndex)
		{
			if (lineIndex == 0) return 0;
			int newlines = 0;
			for (int i = 0; i < userInput.Length; ++i)
			{
				if (newlines == lineIndex)
				{
					return i;
				}
				if (userInput[i] == '\n')
				{
					newlines++;
				}
			}
			if (newlines == lineIndex)
			{
				return userInput.Length;
			}
			return -1;
		}

		public override string ToString()
		{
			return userInput.ToString();
		}

		public void Clear()
		{
			userInput.Clear();
			data.cacheValid = false;
		}

		public int WriteInputChar(char c, int indexOfLetterToAdd = -1)
		{
			if (c == '\b')
			{
				throw new System.Exception("can't delete here. use WriteInputDelete");
			}
			int cursorPush = 1;
			// if the cursor is way past the end of the string, push the cursor back with a negative cursorPush
			if (indexOfLetterToAdd > userInput.Length)
			{
				cursorPush += userInput.Length - indexOfLetterToAdd;
			}
			// for invalid indexes, append to the end
			if (indexOfLetterToAdd == -1 || indexOfLetterToAdd > userInput.Length) {
				indexOfLetterToAdd = userInput.Length;
			}
			userInput.Insert(indexOfLetterToAdd, c);
			// calculate where to continue printing next line if user input is multi-line
			if (c == '\n')
			{
				//numUserInputLines++;
				int lastLineRow = (userInputLineExtraLineLocations.Count > 0)
					? userInputLineExtraLineLocations[userInputLineExtraLineLocations.Count - 1].y
					: data.cursorIndex.y;
				Coord nextLineStart = new Coord(0, lastLineRow);
				if (data.newLineBehavior_columnSameAsStart)
				{
					nextLineStart.x = data.cursorIndex.x;
				}
				userInputLineExtraLineLocations.Add(nextLineStart);
			}
			data.cacheValid = false;
			return cursorPush;
		}
		public int WriteInputDelete(int index = -1, int count = 1)
		{
			int cursorPush = -count;
			// if the cursor is way past the end of the string, push the cursor back with a negative cursorPush
			if (index > userInput.Length)
			{
				cursorPush += userInput.Length - index;
			}
			if (index < 0 || index > userInput.Length)
			{
				index = userInput.Length;
			}
			// if the user deleted a newline character, update the data about drawing newlines
			int limit = Mathf.Min(index + count, userInput.Length);
			int newlinesDeleted = 0;
			for (int i = index; i < limit; ++i)
			{
				if (userInput[i] == '\n')
				{
					newlinesDeleted++;
				}
			}
			if (newlinesDeleted > 0)
			{
				// find which newlines need to be deleted
				int newlinesBeforeIndex = 0;
				for (int i = 0; i < index; ++i)
				{
					if (userInput[i] == '\n') newlinesBeforeIndex++;
				}
				// if there is a crash here, check to see if userInputLineExtraLineLocations has all newlines added to it correctly
				userInputLineExtraLineLocations.RemoveRange(newlinesBeforeIndex, newlinesDeleted);
			}
			// remove the text
			if (index >= 0 && index < userInput.Length)
			{
				userInput.Remove(index, count);
			}
			data.cacheValid = false;
			return cursorPush;
		}

		public int WriteInput(string s)
		{
			for (int i = 0; i < s.Length; ++i)
			{
				WriteInputChar(s[i]);
			}
			return s.Length; // always just .Length; don't care about whitespace.
		}

		public void FlushInputIntoTTY()
		{
			Coord cursor = data.cursorIndex;
			char letter = data.defaultChar;
			Color fore = data.defaultForeground, back = data.defaultBackground;
			for(int i = 0; i < userInput.Length; ++i)
			{
				char c = userInput[i];
				data.WriteCharOutput(c, data.colorSettings.UserInput);
			}
			userInput.Clear();
			data.WriteCharOutput('\n');
		}

		public bool GetAt(Coord cursor, ref char letter, ref ColorRGBA fore, ref ColorRGBA back, string inputBarrier = "")
		{
			if (cursor.y >= data.cursorIndex.y)
			{
				if (userInput.Length != 0)
				{
					// basic calculation for determining which userInput character to pull out here
					int userInputIndex = (cursor.y - data.cursorIndex.y) * data.buffer[cursor.x].Length +
						(cursor.x - data.cursorIndex.x);
					// if the basic calculation isn't good enough, use the line metadata
					if (userInputLineExtraLineLocations.Count > 0)
					{
						int userInputLine = (cursor.y - data.cursorIndex.y);
						if (userInputLine > 0)
						{
							Coord lineLoc = userInputLineExtraLineLocations[userInputLine - 1]; // -1 because line 0 is always cursorIndex, userInputLineExtraLineLocations stores *additional* line locations
							int lineStartIndex = GetIndexOfUserInputLine(userInputLine);
							int lineEndIndex = GetIndexOfUserInputLine(userInputLine + 1);
							if (lineEndIndex < 0) lineEndIndex = userInput.Length;
							int lengthOfLine = lineEndIndex - lineStartIndex;
							//Debug.Log("seeking letter at " + cursor + ", user input line " + userInputLine + " (of " +
							//	userInputLineExtraLineLocations.Count + ") "+lineStartIndex+"->"+lineEndIndex);
							if (cursor.x < lineLoc.x || cursor.x > lineLoc.x + lengthOfLine) // OOB horizontally
							{
								userInputIndex = -1;
							} else {
								userInputIndex = lineStartIndex + (cursor.x - lineLoc.x);
							}
						}
					}
					if (userInputIndex >= 0 && userInputIndex < userInput.Length)
					{
						letter = userInput[userInputIndex];
						fore = data.colorSettings.UserInput;
						return true;
					}
				}
			}
			return false;
		}
	}
}