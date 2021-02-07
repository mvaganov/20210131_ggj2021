using NonStandard;
using NonStandard.Data;
using NonStandard.Data.Parse;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialog {
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
	[Serializable]
	public class Text : DialogOption {
		public string text;
		public TextAnchor anchorText = TextAnchor.UpperLeft;
	}
	[Serializable] public class Choice : Text { public string command; }
	[Serializable] public class Command : DialogOption { public string command; }
}
public class TemplatedDialog : Dialog {
	public string template, parameters;
	public Dialog[] Generate() {
		Dictionary<string, object> data;
		Tokenizer tokenizer = Commander.Instance.GetTokenizer();
		TextAsset dialogData = DialogManager.Instance.GetAsset(parameters),
			dialogTemplate = DialogManager.Instance.GetAsset(template);
		CodeConvert.TryParse(dialogData.text, out data, null, tokenizer);
		if (tokenizer.errors.Count > 0) { Debug.LogError(tokenizer.errors.JoinToString("\n")); }
		//Debug.Log(tok.DebugPrint());
		//Debug.Log(Show.Stringify(data));
		Dialog[] dialogs;
		CodeConvert.TryParse(dialogTemplate.text, out dialogs, data, tokenizer);
		if (tokenizer.errors.Count > 0) { Debug.LogError(tokenizer.errors.JoinToString("\n")); }
		//Debug.Log(tok.DebugPrint());
		//Debug.Log(Show.Stringify(dialogs));
		return dialogs;
	}
}
