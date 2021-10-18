using NonStandard.Commands;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Dialog {
	public class DialogManager : MonoBehaviour {
		public TextAsset root;
		public TextAsset[] knownAssets;
		public ScriptedDictionary dict;
		[Tooltip("what game object should be considered as initiating the dialog")]
		public GameObject dialogWithWho;

		public List<Dialog> dialogs = new List<Dialog>();
		public DialogViewer dialogView;

		public ScriptedDictionary GetScriptScope() { return dict; }
		public static ScriptedDictionary ScopeDictionaryKeeper { get { return DialogManager.Instance.GetScriptScope(); } }
		public static object Scope { get { return ScopeDictionaryKeeper.Dictionary; } }

		public static DialogViewer ActiveDialog { get { return Instance.dialogView; } set { Instance.dialogView = value; } }
		private static DialogManager _instance;
		public static DialogManager Instance { get { return (_instance) ? _instance : _instance = FindObjectOfType<DialogManager>(); } }

		public string GetAsset(string name) {
			TextAsset asset = knownAssets.Find(t => t.name == name);
			return asset != null ? asset.text : null;
		}
		private void Awake() {
			Commander.Instance.AddCommands( new Command[] {
				new Command("dialog", SetDialog, new Argument[] {
					new Argument("-n","nameOfDialog","the name of the dialog to set",type:typeof(string),order:1,required:true)
				}, help:"sets a dialog, disabling any previous dialog options"),
				new Command("start", StartDialog, new Argument[] {
					new Argument("-n","nameOfDialog","the name of the dialog to start",type:typeof(string),order:1,required:true)
				}, help:"sets and starts a dialog, clearing previous dialog"),
				new Command("continue", ContinueDialog, new Argument[] {
					new Argument("-n","nameOfDialog","the name of the dialog to continue",type:typeof(string),order:1,required:true)
				}, help:"continues a dialog without clearing or disabling previous dialogs"),
				new Command("done", Done, help:"brings up button to deactivate dialog"),
				new Command("hide", Hide, help:"hides current dialog"),
				new Command("show", Show, help:"shows dialog")
			});
			Commander.Instance.errorListeners = OnCommanderError;
		}

		void OnCommanderError(List<ParseError> errors) { ActiveDialog.ShowErrors(errors); }

		void Start() {
			if (dialogView == null) { dialogView = FindObjectOfType<DialogViewer>(); }
			if (dict == null) { dict = GetComponentInChildren<ScriptedDictionary>(); }
			Dialog[] dialogs;
			//NonStandard.Show.Log(knownAssets.JoinToString(", ", ta => ta.name));
			//NonStandard.Show.Log(root.name+":" + root.text.Length);
			Tokenizer tokenizer = new Tokenizer();
			if (dict != null) {
				Global.GetComponent<ScriptedDictionaryManager>().SetMainDicionary(dict);
			}
			try {
				CodeConvert.TryParse(root.text, out dialogs, dict.Dictionary, tokenizer);
				tokenizer.ShowErrorTo(NonStandard.Show.Error);
				if (dialogs == null) return;
				//NonStandard.Show.Log("dialogs: [" + d.Stringify(pretty:true)+"]");
				this.dialogs.AddRange(dialogs);
				ResolveTemplatedDialogs(this.dialogs);
			} catch (System.Exception e) {
				NonStandard.Show.Log("~~~#@Start " + e);
			}
			//NonStandard.Show.Log("finished initializing " + this);
			//NonStandard.Show.Log(this.dialogs.JoinToString(", ", dialog => dialog.name));
			// execute all "__init__" dialogs
			ScriptedDictionaryManager m = Global.Get<ScriptedDictionaryManager>();
			object standardScope = m.Main;//Commander.Instance.GetScope();
			for (int i = 0; i < this.dialogs.Count; ++i) {
				Dialog dialog = this.dialogs[i];
				if (!dialog.name.StartsWith("__init__")) { continue; }

				//NonStandard.Show.Log("initializing "+dialog.name);
				Tokenizer tok = new Tokenizer();
				dialog.ExecuteCommands(tok, standardScope);
				if (tok.HasError()) {
					tok.ShowErrorTo(NonStandard.Show.Warning);
				}
			}
			//NonStandard.Show.Log(standardScope.Stringify(pretty: true));
		}
		public static void ResolveTemplatedDialogs(List<Dialog> dialogs) {
			int counter = 0;
			for (int i = 0; i < dialogs.Count; ++i) {
				//NonStandard.Show.Log("checking " + i + " " + dialogs.Count);
				if (++counter > 100000) { throw new System.Exception("too many dialogs..."); }
				TemplatedDialog td = dialogs[i] as TemplatedDialog;
				if (td != null) {
					//NonStandard.Show.Log("resolving " + i + " " + dialogs.Count);
					Dialog[] d = td.Generate();
					//NonStandard.Show.Log("resolved " + i + " " + dialogs.Count);
					dialogs.RemoveAt(i);
					//NonStandard.Show.Log("removed " + i + " " + dialogs.Count);
					if (d != null) {
						dialogs.AddRange(d);
					} else {
						NonStandard.Show.Error("could not generate from " + td.Stringify(false));
					}
					//NonStandard.Show.Log("replaced " + i + " "+dialogs.Count);
					--i;
				}
			}
		}
		public void SetDialog(object src, Tokenizer tok, string name) { ActiveDialog.SetDialog(src, tok, name); }
		public void StartDialog(object src, Tokenizer tok, string name) { ActiveDialog.StartDialog(src, tok, name); }
		public void ContinueDialog(object src, Tokenizer tok, string name) { ActiveDialog.ContinueDialog(src, tok, name); }
		public void Done() { ActiveDialog.Done(); }
		public void Hide() { ActiveDialog.Hide(); }
		public void Show() { ActiveDialog.Show(); }

		public void SetDialog(Command.Exec e) { ActiveDialog.SetDialog(e.src, e.tok, e.tok.GetStr(1)); }
		public void StartDialog(Command.Exec e) { ActiveDialog.StartDialog(e.src, e.tok, e.tok.GetStr(1)); }
		public void ContinueDialog(Command.Exec e) { ActiveDialog.ContinueDialog(e.src, e.tok, e.tok.GetStr(1)); }
		public void Done(Command.Exec e) { ActiveDialog.Done(); }
		public void Hide(Command.Exec e) { ActiveDialog.Hide(); }
		public void Show(Command.Exec e) { ActiveDialog.Show(); }
	}
}