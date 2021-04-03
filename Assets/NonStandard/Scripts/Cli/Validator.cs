using UnityEngine;
using TMPro;

namespace NonStandard.Cli
{
	// TODO search for "data." and evaluate refactor
	/// <summary>the class that tries to keep the user from wrecking the command line terminal</summary>
	public class Validator : TMP_InputValidator
	{
		public CmdLine_base cmd;
		private TMP_InputField inputField;
		public const char ADDED_FOR_NULL_SPACE = '\b';
		public const char FILLED_IN_NULL_SPACE = (char)(0xA0);
		/// <summary>what replaces an attempt to un-escape the TextMeshPro noparse boundary in the command line</summary>
		public const string NOPARSE_REPLACEMENT = ">NOPARSE<";

		public void Init(CmdLine_base cmd)
		{
			this.cmd = cmd;
			this.inputField = cmd._tmpInputField;
		}
		public void AddUserInput(string userInput)
		{
			string s = inputField.text;
			int cursor = inputField.caretPosition;
			for (int i = 0; i < userInput.Length; ++i)
			{
				char c = userInput[i];
				Validate(ref s, ref cursor, c);
			}
			inputField.text = s;
			inputField.caretPosition = cursor;
		}

		public int CalculateCursorIndexInUserInput()
		{
			int indexInUserInput = cmd._tmpInputField.stringPosition - cmd.data.cursorIndexInString;
			if (indexInUserInput < 0) indexInUserInput = 0;
			cmd.data.input.userInputCursor = indexInUserInput;
			return indexInUserInput;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text">text being written into</param>
		/// <param name="pos">index in the text where pressedLetter was pressed (not inserted into text yet)</param>
		/// <param name="pressedLetter">what letter was just written</param>
		/// <returns>null terminator.</returns>
		public override char Validate(ref string text, ref int pos, char pressedLetter)
		{
			int posAtStart = pos;
			if (!ManageUI.IsInteractive(cmd)) return '\0';
			if (pressedLetter == '\0')
			{
				//Debug.Log("null teminator pressed?\n@"+pos+"\n"+text);
				return '\0';
			}
			if (cmd.AcceptingCommands && pressedLetter == '\n')
			{
				ExecuteUserInput();
			}
			// insert letters into the text (unless the enter key was pressed and the CmdLine Terminal is accepting commands) 
			else //if (pressedLetter != '\n' || !cmd.AcceptingCommands)
			{
				text = cmd.data.GetTMProString(cmd.commander.CommandPromptArtifact());
				int indexInUserInput = CalculateCursorIndexInUserInput();
				//Debug.Log("index " + indexInUserInput);
				int cursorChange = cmd.data.input.WriteInputChar(pressedLetter, indexInUserInput);
				cmd.data.input.userInputCursor += cursorChange; // TODO does this line do anything?
				//cmd._tmpInputField.stringPosition+= cursorChange;
				pos += cursorChange;
			}
			
			text = cmd.data.GetTMProString(out pos, cmd.commander.CommandPromptArtifact());

			return '\0';// pressedLetter;
		}

		void ExecuteUserInput()
		{
			string inpt = cmd.data.input.ToString();
			// paste the command onto the TTYData
			cmd.data.input.FlushInputIntoTTY();
			object whoExecutes = cmd.UserRawInput; // the user-controlled input field

			int start = 0, end = -1;
			do
			{
				// TODO test this code with multiple lines...
				// if there is more than one line, that means multiple commands need to get executed...
				end = inpt.IndexOf("\n", start);
				if (end >= start && start < inpt.Length)
				{
					int len = end - start;
					if (len > 0) {
						Debug.Log("enqueue! " + inpt.Substring(start, len));
						cmd.commander.EnqueueRun(new Commander.Instruction() { text = inpt.Substring(start, len), user = whoExecutes });
					}
					start = end + 1; // start again after the newline character
				}
			} while (end > 0);
			if (start < inpt.Length) {
				cmd.commander.EnqueueRun(new Commander.Instruction() { text = inpt.Substring(start), user = whoExecutes });
			}
		}

		public void listener_OnValueChanged(string str)
		{
			if (cmd.addingOnChanged) return; // prevent listener_OnValueChanged from being called recursively by setText
			cmd.addingOnChanged = true;
			string expectedText = cmd.data.GetTMProString(cmd.commander.CommandPromptArtifact());// cmd.data.GetTMProString(out stringIndex);
			int deletedSomething = expectedText.Length - str.Length;
			if (deletedSomething > 0)
			{
				//Debug.Log("deleted something?"); // TODO what about when the text rolls from filling the vertical buffer?
			} else
			{
				//Debug.Log("nothing deleted?\n---\n" + expectedText + "\n---\n" + str + "\n---");
			}

			if (deletedSomething > 0)
			{
				// if deleting the first letter of a tagged word would bring the cursor out infront of the tag. this loop might cause the stringPosition to skip too far if the generated TMPro-ML creates empty tags.
				if (cmd._tmpInputField.stringPosition < cmd.data.cursorIndexInString
				&& cmd._tmpInputField.stringPosition < cmd._tmpInputField.text.Length
				&& cmd._tmpInputField.text[cmd._tmpInputField.stringPosition] == '<')
				{
					// get the cursor back into the correct tag area
					int endOfTag = cmd._tmpInputField.text.IndexOf('>', cmd._tmpInputField.stringPosition);
					cmd._tmpInputField.stringPosition = endOfTag + 1; // shouldn't be possible for it to move stringPosition OOB, since tags are automatically added, and there should always be a closing tag at least.
				}
				bool nothingIsAllowedTobeDeleted = cmd.data.input.userInput.Length == 0;
				bool editingOOB_2soon = cmd._tmpInputField.stringPosition < cmd.data.cursorIndexInString &&
					!(deletedSomething == cmd.data.input.userInput.Length); // deleting the entire string is ok.
				bool editingOOB_2far = !editingOOB_2soon && cmd._tmpInputField.stringPosition > cmd.data.cursorIndexInString + cmd.data.input.userInput.Length;
				if (nothingIsAllowedTobeDeleted || editingOOB_2soon || editingOOB_2far)
				{
					cmd._tmpInputField.text = expectedText;
					if (nothingIsAllowedTobeDeleted)
					{
						//Debug.Log("y4uDELETE?");
					}
					if (editingOOB_2soon)
					{
						//Debug.Log("2soon "+ cmd._tmpInputField.stringPosition+" < "+ cmd.data.cursorIndexInString+" : "+cmd._tmpInputField.text.Substring(cmd._tmpInputField.stringPosition));
						cmd._tmpInputField.stringPosition = cmd.data.cursorIndexInString;
					}
					if (editingOOB_2far)
					{
						cmd._tmpInputField.stringPosition = cmd.data.cursorIndexInString + cmd.data.input.userInput.Length;
						//Debug.Log("2far");
					}
				} else
				{
					string originalStr = expectedText;
					string deletedStr = originalStr.Substring(cmd._tmpInputField.stringPosition, deletedSomething);
					Vector2Int coord = TTYData.CalculateCoordinateOf(originalStr, cmd._tmpInputField.stringPosition);
					//Debug.Log("Deleted >" + cmd._tmpInputField.stringPosition + "< "+ coord+" : \'"+ deletedStr+"\'\n----\n"+cmd._tmpInputField.text+"\n-- vs --\n"+ originalStr);
					int indexInUserInput = CalculateCursorIndexInUserInput();
					//Debug.Log("deleting "+ deletedSomething + " letters @ index " + indexInUserInput);
					cmd.data.input.WriteInputDelete(indexInUserInput, deletedSomething);
					// force valid state after any text is removed
					cmd._tmpInputField.text = cmd.data.GetTMProString(cmd.commander.CommandPromptArtifact());
					if(cmd.data.input.userInput.Length == 0)
					{
						cmd._tmpInputField.stringPosition--; // for some reason, deleting the last letter moves the cursor off by one
					}
				}
			}
			cmd.addingOnChanged = false;
		}

		public string BEGIN_USER_INPUT()
		{
			return "<#" + cmd.ColorSet.UserInputHex + "><noparse>";
		}
		public string END_USER_INPUT() { return "</noparse></color>"; }
	}
}