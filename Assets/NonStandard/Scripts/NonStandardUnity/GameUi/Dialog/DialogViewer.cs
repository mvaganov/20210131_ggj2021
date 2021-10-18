using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NonStandard.Data.Parse;
using NonStandard.Ui;
using UnityEngine.Events;
using NonStandard.Process;
using System.Text;
using static NonStandard.Commands.Commander;
using NonStandard.Cli;
using Commander = NonStandard.Commands.Commander;
using NonStandard.Data;

namespace NonStandard.GameUi.Dialog {
	public class DialogViewer : MonoBehaviour {
		public Text title;
		public ScrollRect scrollRect;
		public Button closeButton;
		public UnityEvent onDialog;

		ListUi listUi;
		ListItemUi prefab_buttonUi, prefab_textUi;
		List<ListItemUi> currentChoices = new List<ListItemUi>();
		ListItemUi closeDialogButton;
		bool initialized = false;
		bool goingToScrollAllTheWayDown;
		public void InitializeListUi() {
			listUi = GetComponentInChildren<ListUi>();
			if (scrollRect == null) { scrollRect = GetComponentInChildren<ScrollRect>(); }
			prefab_buttonUi = listUi.prefab_item;
			prefab_textUi = Instantiate(prefab_buttonUi.gameObject).GetComponent<ListItemUi>();
			Destroy(prefab_textUi.GetComponent<Button>());
			Destroy(prefab_textUi.GetComponent<Image>());
		}
		public void Print(string text) { Print_commandOutput.Append(text); }
		private void PossiblyAddParseCommandOutputToDialog(Dialog.DialogOption option) {
			if (Print_commandOutput.Length == 0) return;
			string str = Print_commandOutput.ToString();
			ListItemUi li = listUi.AddItem(option, str, null, prefab_textUi);
			Print_commandOutput.Clear();
		}
		private StringBuilder Print_commandOutput = new StringBuilder();
		public ListItemUi AddDialogOption(Dialog.DialogOption option, bool scrollAllTheWayDown) {
			if (!initialized) { Init(); }
			ListItemUi li = null;
			switch (option) {
			case Dialog.Choice c: AddDialogOption_Choice(c, out li, option); break;
			case Dialog.Text t: AddDialogOption_Text(t, out li, option); break;
			case Dialog.Command cmd: AddDialogOption_Command(cmd, out li, option); break;
			}
			if (li != null) {
				Dialog.Text txt = option as Dialog.Text;
				if (txt != null) { li.TextAlignment = txt.anchorText; }
			}
			if (scrollAllTheWayDown && !goingToScrollAllTheWayDown) {
				goingToScrollAllTheWayDown = true;
				// we want scroll all the way down, and can't control when the UI updates enough to realize it can scroll
				Proc.Delay(100, () => {
					goingToScrollAllTheWayDown = false; scrollRect.verticalNormalizedPosition = 0;
				});
				// 100ms (1/10th of a second) is not bad for UI lag, and should be enough time for the UI to update itself
			}
			return li;
		}

		private void AddDialogOption_Choice(Dialog.Choice c, out ListItemUi listItem, Dialog.DialogOption option) {
			ListItemUi li = null;
			li = listItem = listUi.AddItem(option, DialogManager.Instance.GetScriptScope().Format(c.text), () => {
				//NonStandard.Show.Log("------- "+c.command);
				Tokenizer tok = new Tokenizer();
				Commander.Instance.ParseCommand(new Instruction(c.command, li), Print, out tok);
				//NonStandard.Show.Log("/////// " + c.command);
				if (tok?.HasError() ?? false) { Print(tok.GetErrorString()); }
				PossiblyAddParseCommandOutputToDialog(option);
			}, prefab_buttonUi);
			currentChoices.Add(li);
		}
		private void AddDialogOption_Text(Dialog.Text t, out ListItemUi listItem, Dialog.DialogOption option) {
			listItem = listUi.AddItem(option, DialogManager.Instance.GetScriptScope().Format(t.text), null, prefab_textUi);
		}
		private void AddDialogOption_Command(Dialog.Command cmd, out ListItemUi listItem, Dialog.DialogOption option) {
			//NonStandard.Show.Log("executing command "+cmd.command);
			Tokenizer tok = new Tokenizer();
			Commander.Instance.ParseCommand(new Instruction(cmd.command, option), Print, out tok);
			if (tok?.HasError() ?? false) { Print(Col.r() + tok.GetErrorString()); }
			PossiblyAddParseCommandOutputToDialog(option);
			listItem = null;
		}
		public void ShowErrors(List<ParseError> errors) {
			if (errors.Count > 0) {
				for (int i = 0; i < errors.Count; ++i) { ShowError(errors[i].ToString()); }
			}
		}
		public void ShowError(string errorMessage) {
			ListItemUi li = AddDialogOption(new Dialog.Text { text = errorMessage }, true);
			li.TextColor = Color.red;
		}

		public void ShowCloseDialogButton() {
			if (!initialized) { Init(); }
			if (closeDialogButton != null) { Destroy(closeDialogButton.gameObject); }
			closeDialogButton = AddDialogOption(new Dialog.Choice {
				text = "\n<close dialog>\n", command = "hide", anchorText = TextAnchor.MiddleCenter
			}, true);
			currentChoices.Remove(closeDialogButton);
		}
		void Init() {
			if (initialized) { return; } else { initialized = true; }
			InitializeListUi();
			//tokenizer = new Tokenizer();
			//CodeConvert.TryParse(dialogAsset.text, out dialogs, GetScriptScope(), tokenizer);
			//if (tokenizer.errors.Count > 0) {
			//	Debug.LogError(tokenizer.errors.JoinToString("\n"));
			//}
			////Debug.Log(tokenizer.DebugPrint());
			////Debug.Log(NonStandard.Show.Stringify(dialogs, true));
			//if (dialogs == null) { dialogs = new List<Dialog>(); }
			//if (dialogs.Count > 0) { SetDialog(dialogs[0], UiPolicy.StartOver); }
		}
		void Start() { Init(); }
		public void DeactivateDialogChoices() {
			for (int i = 0; i < currentChoices.Count; ++i) {
				currentChoices[i].button.interactable = false;
			}
			currentChoices.Clear();
		}
		public void RemoveDialogElements() {
			DeactivateDialogChoices();
			Transform pt = listUi.transform;
			for (int i = 0; i < pt.childCount; ++i) {
				Transform t = pt.GetChild(i);
				if (t != prefab_buttonUi.transform && t != prefab_textUi.transform) {
					Destroy(t.gameObject);
				}
			}
		}
		public enum UiPolicy { StartOver, DisablePrev, Continue }
		public void SetDialog(object src, Tokenizer tok, string name, UiPolicy uiPolicy) {
			if (!initialized) { Init(); }
			Dialog dialog = DialogManager.Instance.dialogs.Find(d => d.name == name);
			if (dialog == null) { tok.AddError("missing dialog \"" + name + "\""); }
			SetDialog(src, tok, dialog, uiPolicy);
		}
		public void SetDialog(object src, Tokenizer tok, Dialog dialog, UiPolicy uiPolicy) {
			if (!initialized) { Init(); }
			bool isScrolledAllTheWayDown = !scrollRect.verticalScrollbar.gameObject.activeInHierarchy ||
				scrollRect.verticalNormalizedPosition < 1f / 1024; // keep scrolling down if really close to bottom
			switch (uiPolicy) {
			case UiPolicy.StartOver: RemoveDialogElements(); break;
			case UiPolicy.DisablePrev: DeactivateDialogChoices(); break;
			case UiPolicy.Continue: break;
			}
			if (dialog == null) { tok.AddError("missing dialog"); return; }
			if (dialog.options != null) {
				for (int i = 0; i < dialog.options.Length; ++i) {
					Dialog.DialogOption opt = dialog.options[i];
					//NonStandard.Show.Log("checking opt " + NonStandard.Show.Stringify(opt, false));
					ScriptedDictionaryManager m = Global.Get<ScriptedDictionaryManager>();
					if (opt.Available(tok, m.Main)) {
						AddDialogOption(opt, isScrolledAllTheWayDown);
						//NonStandard.Show.Log("added" + NonStandard.Show.Stringify(opt, false));
					} else {
						//NonStandard.Show.Log("ignored" + NonStandard.Show.Stringify(opt, false));
					}
				}
			}
			ShowErrors(tok.errors);
			onDialog?.Invoke();
		}
		public void SetDialog(object src, Tokenizer tok, string name) {
			DialogManager.ActiveDialog = this; SetDialog(src, tok, name, UiPolicy.DisablePrev);
		}
		public void StartDialog(object src, Tokenizer tok, string name) {
			DialogManager.ActiveDialog = this; SetDialog(src, tok, name, UiPolicy.StartOver);
		}
		public void ContinueDialog(object src, Tokenizer tok, string name) {
			DialogManager.ActiveDialog = this; SetDialog(src, tok, name, UiPolicy.Continue);
		}
		public void Done() { DeactivateDialogChoices(); ShowCloseDialogButton(); }
		public void Hide() {
			UiClick.Click(closeButton);
			//gameObject.SetActive(false);
		}
		public void Show() { DialogManager.ActiveDialog = this; gameObject.SetActive(true); }

	}
}