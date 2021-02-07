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
		Tokenizer cmdTok = new Tokenizer(); // don't use the global tokenizer, who knows where it's going
		cmdTok.Tokenize(command);
		bool moreCommands;
		do {
			moreCommands = false;
			string cmd = cmdTok.GetResolvedToken(0, GetScope()).ToString();
			ExecuteCommand(cmd, cmdTok);
			for(int i = 0; i < cmdTok.TokenCount; ++i) {
				Token t = cmdTok.GetToken(i);
				Delim d = t.GetAsDelimiter();
				if(d != null && d.text == ";") {
					moreCommands = true;
					cmdTok.PopTokens(i + 1);
					break;
				}
			}
		} while (moreCommands);
	}
	public void ExecuteCommand(string command, Tokenizer tok) {
		Show.Log(command+": "+tok.DebugPrint());
		Action<Tokenizer> commandToExecute;
		if (commandListing.TryGetValue(command, out commandToExecute)) {
			commandToExecute.Invoke(tok);
		} else {
			tok.AddError("unknown command \'" + command + "\'");
		}
		ActiveDialog.PopErrors(tok.errors);
	}
	private void InitializeCommands() {
		commandListing = new Dictionary<string, Action<Tokenizer>>() {
			["dialog"] = SetDialog,
			["start"] = StartDialog,
			["continue"] = ContinueDialog,
			["done"] = Done,
			["hide"] = Hide,
			["show"] = _Show,
			["++"] = Increment,
			["--"] = Decrement,
			["set"] = SetVariable,
			["give"] = GiveInventory,
			["claimplayer"] = ClaimPlayer,
			["exit"] = s => PlatformAdjust.Exit(),
		};
	}
	public void SetDialog(Tokenizer tok) { ActiveDialog.SetDialog(tok.GetStr(1)); }
	public void StartDialog(Tokenizer tok) { ActiveDialog.StartDialog(tok.GetStr(1)); }
	public void ContinueDialog(Tokenizer tok) { ActiveDialog.ContinueDialog(tok.GetStr(1)); }
	public void Done(Tokenizer tok) { ActiveDialog.Done(); }
	public void Hide(Tokenizer tok) { ActiveDialog.Hide(); }
	public void _Show(Tokenizer tok) { ActiveDialog.Show(); }
	public void Increment(string name) {
		if (ScopeDictionaryKeeper == null) { tokenizer.AddError("can't add 1 to \"" + name + "\", missing variable scope"); return; }
		ScopeDictionaryKeeper.AddTo(name, 1);
	}
	public void Decrement(string name) {
		if (ScopeDictionaryKeeper == null) { tokenizer.AddError("can't add -1 to \"" + name + "\", missing variable scope"); return; }
		ScopeDictionaryKeeper.AddTo(name, -1);
	}
	
	public void Increment(Tokenizer tok) { Increment(tok.GetStr(1)); }
	public void Decrement(Tokenizer tok) { Decrement(tok.GetStr(1)); }
	public void SetVariable(Tokenizer tok) {
		string key = tok.GetStr(1, Scope);
		object value = tok.GetResolvedToken(2, Scope);
		string vStr = value as string;
		float f;
		if(vStr != null && float.TryParse(vStr, out f)) { value = f; }
		ScopeDictionaryKeeper.Dictionary.Set(key, value);
	}
	public void GiveInventory(Tokenizer tok) {
		string itemName = tok.GetStr(1, Scope);
		Inventory inv = Global.Get<Inventory>();
		GameObject itemObj = inv.RemoveItem(itemName);
		if(tok.TokenCount == 2) { UnityEngine.Object.Destroy(itemObj); }
	}

	public void ClaimPlayer(Tokenizer tok) {
		Global.Get<Team>().AddMember(DialogManager.Instance.dialogSource);
		Discovery d = DialogManager.Instance.dialogSource.GetComponentInChildren<Discovery>(true);
		if(d != null) { d.gameObject.SetActive(true); }
	}
}
