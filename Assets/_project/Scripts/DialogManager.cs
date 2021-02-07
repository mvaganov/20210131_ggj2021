using NonStandard;
using NonStandard.Data;
using NonStandard.Data.Parse;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public TextAsset root;
    public TextAsset[] knownAssets;
    public DictionaryKeeper dict;
    public GameObject dialogSource;

    public List<Dialog> dialogs = new List<Dialog>();
    public DialogViewer dialogView;
    public DictionaryKeeper GetScriptScope() { return dict; }
	public static DictionaryKeeper ScopeDictionaryKeeper { get { return DialogManager.Instance.GetScriptScope(); } }
	public static object Scope { get { return ScopeDictionaryKeeper.Dictionary; } }

    public static DialogViewer ActiveDialog { get { return Instance.dialogView; } set { Instance.dialogView = value; } }
    private static DialogManager _instance;
    public static DialogManager Instance {  get { return (_instance) ? _instance : _instance = FindObjectOfType<DialogManager>(); } }

    public TextAsset GetAsset(string name) {
        return knownAssets.Find(t => t.name == name);
	}

    void Start()
    {
        if (dialogView == null) { dialogView = FindObjectOfType<DialogViewer>(); }
        if(dict == null) { dict = GetComponentInChildren<DictionaryKeeper>(); }
        Dialog[] d;
        CodeConvert.TryParse(root.text, out d, dict.Dictionary, Commander.Tokenizer);
        dialogs.AddRange(d);
        ResolveTemplatedDialogs(dialogs);
    }
    public static void ResolveTemplatedDialogs(List<Dialog> dialogs) {
        int counter = 0;
        for (int i = 0; i < dialogs.Count; ++i) {
            if(++counter > 100000) { throw new System.Exception("too many dialogs..."); }
            TemplatedDialog td = dialogs[i] as TemplatedDialog;
			if (td != null) {
                Dialog[] d = td.Generate();
                dialogs.RemoveAt(i);
                --i;
                dialogs.AddRange(d);
			}
        }
    }
    public void SetDialog(string name) { ActiveDialog.SetDialog(name); }
    public void StartDialog(string name) { ActiveDialog.StartDialog(name); }
    public void ContinueDialog(string name) { ActiveDialog.ContinueDialog(name); }
    public void Done() { ActiveDialog.Done(); }
    public void Hide() { ActiveDialog.Hide(); }
    public void Show() { ActiveDialog.Show(); }

}
