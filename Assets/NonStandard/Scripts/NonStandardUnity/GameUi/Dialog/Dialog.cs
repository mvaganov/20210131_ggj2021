using NonStandard.Data;
using NonStandard.Data.Parse;
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
				bool available;
				if (!required.TryResolve(out available, tok, scope)) { return false; }
				return available;
			}
		}
		[Serializable] public class Text : DialogOption {
			public string text;
			public UnityEngine.TextAnchor anchorText = UnityEngine.TextAnchor.UpperLeft;
		}
		[Serializable] public class Choice : Text { public string command; }
		[Serializable] public class Command : DialogOption { public string command; }
	}
	public class TemplatedDialog : Dialog {
		public string template, parameters;
		public Dialog[] Generate() {
			Dictionary<string, object> data;
			Tokenizer tokenizer = new Tokenizer();
			string dialogData = DialogManager.Instance.GetAsset(parameters),
				dialogTemplate = DialogManager.Instance.GetAsset(template);
			if (dialogData == null || dialogTemplate == null) {
				Show.Error("failed to find components of templated script " + template + "<" + parameters + ">");
				return null;
			}
			CodeConvert.TryParse(dialogData, out data, null, tokenizer);
			if (tokenizer.ShowErrorTo(Show.Error)) { return null; }
			Dialog[] dialogs;
			CodeConvert.TryParse(dialogTemplate, out dialogs, data, tokenizer);
			if (tokenizer.ShowErrorTo(Show.Error)) { return null; }
			return dialogs;
		}
	}
}