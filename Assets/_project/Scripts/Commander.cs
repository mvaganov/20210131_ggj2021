using NonStandard;
using NonStandard.Data;
using NonStandard.Data.Parse;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Commander {
	Dictionary<string, Action<Tokenizer>> commandListing;
	Tokenizer tokenizer;

	private static Commander _instance;
	public static Commander Instance { get { return (_instance != null) ? _instance : _instance = new Commander(); } }
	public Tokenizer GetTokenizer() { return tokenizer; }
	public static Tokenizer Tokenizer { get { return Instance.GetTokenizer(); } }
	public static DictionaryKeeper ScopeDictionaryKeeper { get { return DialogManager.Instance.GetScriptScope(); } }
	public static object Scope { get { return ScopeDictionaryKeeper.Dictionary; } }
	public object GetScope() { return Scope; }
	DialogViewer ActiveDialog { get { return DialogManager.ActiveDialog; } }
	private Commander() {
		InitializeCommands();
		tokenizer = new Tokenizer();
	}
	public void ParseCommand(string command) {
		tokenizer.Tokenize(command);
		string cmd = tokenizer.GetResolvedToken(0, GetScope()).ToString();
		Action<Tokenizer> commandToExecute;
		if (commandListing.TryGetValue(cmd, out commandToExecute)) {
			commandToExecute.Invoke(tokenizer);
		} else {
			tokenizer.AddError("unknown command \'" + cmd + "\'");
		}
		if (tokenizer.errors.Count > 0) {
			for (int i = 0; i < tokenizer.errors.Count; ++i) {
				ListItemUi li = ActiveDialog.AddDialogOption(new Dialog.Text { text = tokenizer.errors[i].ToString() }, true);
				li.text.color = Color.red;
			}
			tokenizer.errors.Clear();
		}
	}
	private void InitializeCommands() {
		commandListing = new Dictionary<string, Action<Tokenizer>>() {
			["dialog"] = SetDialog,
			["start"] = StartDialog,
			["continue"] = ContinueDialog,
			["done"] = Done,
			["hide"] = Hide,
			["show"] = Show,
			["++"] = Increment,
			["set"] = SetVariable,
			["exit"] = s => PlatformAdjust.Exit(),
		};
	}
	public void SetDialog(Tokenizer tok) { ActiveDialog.SetDialog(tok.GetStr(1)); }
	public void StartDialog(Tokenizer tok) { ActiveDialog.StartDialog(tok.GetStr(1)); }
	public void ContinueDialog(Tokenizer tok) { ActiveDialog.ContinueDialog(tok.GetStr(1)); }
	public void Done(Tokenizer tok) { ActiveDialog.Done(); }
	public void Hide(Tokenizer tok) { ActiveDialog.Hide(); }
	public void Show(Tokenizer tok) { ActiveDialog.Show(); }
	public void Increment(string name) {
		if (ScopeDictionaryKeeper == null) {
			tokenizer.AddError("can't add 1 to \"" + name + "\", missing variable scope");
			return;
		}
		ScopeDictionaryKeeper.AddTo(name, 1);
	}
	public void Increment(Tokenizer tok) { Increment(tok.GetStr(1)); }
	public void SetVariable(Tokenizer tok) {
		string key = tok.GetStr(1, Scope);
		object value = tok.GetResolvedToken(2, Scope);
		string vStr = value as string;
		float f;
		if(vStr != null && float.TryParse(vStr, out f)) { value = f; }
		ScopeDictionaryKeeper.Dictionary.Set(key, value);
	}

}
