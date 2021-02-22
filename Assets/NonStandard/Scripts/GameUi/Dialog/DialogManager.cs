using NonStandard.Data;
using NonStandard.Data.Parse;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Dialog {
    public class DialogManager : MonoBehaviour {
        public TextAsset root;
        public TextAsset[] knownAssets;
        public DictionaryKeeper dict;
        [Tooltip("what game object should be considered as initiating the dialog")]
        public GameObject dialogWithWho;

        public List<Dialog> dialogs = new List<Dialog>();
        public DialogViewer dialogView;

        public DictionaryKeeper GetScriptScope() { return dict; }
        public static DictionaryKeeper ScopeDictionaryKeeper { get { return DialogManager.Instance.GetScriptScope(); } }
        public static object Scope { get { return ScopeDictionaryKeeper.Dictionary; } }

        public static DialogViewer ActiveDialog { get { return Instance.dialogView; } set { Instance.dialogView = value; } }
        private static DialogManager _instance;
        public static DialogManager Instance { get { return (_instance) ? _instance : _instance = FindObjectOfType<DialogManager>(); } }

        public TextAsset GetAsset(string name) {
            return knownAssets.Find(t => t.name == name);
        }
        private void Awake() {
            Commander.Instance.AddCommands(
                new Dictionary<string, System.Action<object, Tokenizer>>() {
                    ["dialog"] = SetDialog,
                    ["start"] = StartDialog,
                    ["continue"] = ContinueDialog,
                    ["done"] = Done,
                    ["hide"] = Hide,
                    ["show"] = _Show,
                });
            Commander.Instance.onErrors = OnCommanderError;
        }

        void OnCommanderError(List<ParseError> errors) { ActiveDialog.ShowErrors(errors); }

        void Start() {
            if (dialogView == null) { dialogView = FindObjectOfType<DialogViewer>(); }
            if (dict == null) { dict = GetComponentInChildren<DictionaryKeeper>(); }
            Dialog[] d;
            //NonStandard.Show.Log(knownAssets.JoinToString(", ", ta => ta.name));
            //NonStandard.Show.Log(root.name+":" + root.text.Length);
            Tokenizer tokenizer = new Tokenizer();
            try {
                CodeConvert.TryParse(root.text, out d, dict.Dictionary, tokenizer);
                tokenizer.ShowErrorTo(NonStandard.Show.Error);
                if (d == null) return;
                //NonStandard.Show.Log("dialogs: [" + NonStandard.Show.Stringify(d, false)+"]");
                dialogs.AddRange(d);
                ResolveTemplatedDialogs(dialogs);
            } catch (System.Exception e) {
                NonStandard.Show.Log("~~~#@Start " + e);
            }
            //NonStandard.Show.Log("finished initializing " + this);
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
                        NonStandard.Show.Error("could not generate from " + NonStandard.Show.Stringify(td, false));
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

        public void SetDialog(object src, Tokenizer tok) { ActiveDialog.SetDialog(src, tok, tok.GetStr(1)); }
        public void StartDialog(object src, Tokenizer tok) { ActiveDialog.StartDialog(src, tok, tok.GetStr(1)); }
        public void ContinueDialog(object src, Tokenizer tok) { ActiveDialog.ContinueDialog(src, tok, tok.GetStr(1)); }
        public void Done(object src, Tokenizer tok) { ActiveDialog.Done(); }
        public void Hide(object src, Tokenizer tok) { ActiveDialog.Hide(); }
        public void _Show(object src, Tokenizer tok) { ActiveDialog.Show(); }
    }
}