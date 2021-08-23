using NonStandard;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using NonStandard.Utility.UnityEditor;
using NonStandard.Extension;

public class DropDownEvent : MonoBehaviour
{
	[ContextMenuItem("PopulateDropdown", "PopulateDropdown")]
	public List<ModalConfirmation.Entry> options;
	int lastValue = -1;
	public void PopulateDropdown() {
		TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
		dd.ClearOptions();
		List<TMP_Dropdown.OptionData> opts = dd.options;
		for (int i = 0; i < options.Count; ++i) {
			if (i >= opts.Count) { opts.Add(new TMP_Dropdown.OptionData(options[i].text, options[i].image)); }
			if (opts[i].text != options[i].text) { opts[i].text = options[i].text; }
			if (opts[i].image != options[i].image) { opts[i].image = options[i].image; }
		}
		Show.Log("set "+options.Count+" opt : "+dd+"("+dd.options.Count+")\n"+options.Stringify(pretty:true));
		EventBind.IfNotAlready(dd.onValueChanged, this, nameof(HandleDropdown));
	}

	public void HandleDropdown(int index) {
		if (index < 0 || index >= options.Count) {
			Show.Error("index "+index+" out of range [0, "+options.Count+")");
		}
		ModalConfirmation.Entry e = options[index];
		UnityEvent ue = e.selectionAction;
		if(ue != null) { ue.Invoke(); }
		if (e.eventOnly) {
			TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
			dd.SetValueWithoutNotify(lastValue);
		} else {
			lastValue = index;
		}
	}
}
