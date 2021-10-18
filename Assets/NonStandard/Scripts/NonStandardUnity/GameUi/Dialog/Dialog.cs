using NonStandard.Commands;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using System;
using System.Collections.Generic;

namespace NonStandard.GameUi.Dialog {
	[Serializable] public class Dialog {
		public string name;
		public DialogOption[] options;
		public abstract class DialogOption {
			public Expression required; // conditional requirement for this option
			public bool Available(Tokenizer tok, object scope) {
				if (required == null) return true;
				//Show.Log("resolving required condition: "+required.ToString());
				if (!required.TryResolve(out bool available, tok, scope)) { return false; }
				return available;
			}
		}
		[Serializable] public class Text : DialogOption {
			public string text;
			public UnityEngine.TextAnchor anchorText = UnityEngine.TextAnchor.UpperLeft;
		}
		[Serializable] public class Choice : Text { public string command; }
		[Serializable] public class Command : DialogOption { public string command; }

		/// <param name="tok"></param>
		/// <param name="scope">Commander.Instance.GetScope()</param>
		public void ExecuteCommands(Tokenizer tok, object scope) {
			if (options == null) { return; }
			for (int i = 0; i < options.Length; ++i) {
				Command cmd = options[i] as Command;
				if (cmd == null) { continue; }
				Commander.Instance.ParseCommand(new Commander.Instruction(cmd.command, scope), null, out tok);
			}
		}
	}
	public class TemplatedDialog : Dialog {
		public string template, parameters;
		public Dialog[] Generate() {
			Dictionary<string, object> parametersForTemplate;
			Tokenizer tokenizer = new Tokenizer();
			string dialogData = DialogManager.Instance.GetAsset(parameters),
				dialogTemplate = DialogManager.Instance.GetAsset(template);
			if (dialogData == null || dialogTemplate == null) {
				Show.Error("failed to find components of templated script " + template + "<" + parameters + ">");
				return null;
			}
			CodeConvert.TryParse(dialogData, out parametersForTemplate, null, tokenizer);
			if (tokenizer.ShowErrorTo(Show.Error)) { return null; }
			//Show.Log(parametersForTemplate.Stringify(pretty: true));
			Dialog[] templatedDialogs;
			CodeConvert.TryParse(dialogTemplate, out templatedDialogs, parametersForTemplate, tokenizer);
			if (tokenizer.ShowErrorTo(Show.Error)) { return null; }
			//Show.Log(templatedDialogs.Stringify(pretty: true));
			return templatedDialogs;
		}
	}
}