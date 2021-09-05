using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using NonStandard.Utility.UnityEditor;
using NonStandard.Extension;
using System;

namespace NonStandard.Ui {
	public class DropDownEvent : MonoBehaviour {
		[ContextMenuItem("PopulateDropdown", "PopulateDropdown")]
		public List<ModalConfirmation.Entry> options;
		int lastValue = -1;
		public void PopulateDropdown() {
			TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
			PopulateDropdown(dd, options, this, HandleDropdown);
		}
		public static void PopulateDropdown(TMP_Dropdown dd, IList<string> options, object ownerOfDropdownHandler, Action<int> action) {
			dd.ClearOptions();
			List<TMP_Dropdown.OptionData> opts = dd.options;
			for (int i = 0; i < options.Count; ++i) {
				if (i >= opts.Count) { opts.Add(new TMP_Dropdown.OptionData(options[i], null)); }
				if (opts[i].text != options[i]) { opts[i].text = options[i]; }
				if (opts[i].image != null) { opts[i].image = null; }
			}
			BindDropdownAction(dd, ownerOfDropdownHandler, action);
		}
		public static void PopulateDropdown(TMP_Dropdown dd, IList<ModalConfirmation.Entry> options, object ownerOfDropdownHandler, Action<int> action) {
			dd.ClearOptions();
			List<TMP_Dropdown.OptionData> opts = dd.options;
			for (int i = 0; i < options.Count; ++i) {
				if (i >= opts.Count) { opts.Add(new TMP_Dropdown.OptionData(options[i].text, options[i].image)); }
				if (opts[i].text != options[i].text) { opts[i].text = options[i].text; }
				if (opts[i].image != options[i].image) { opts[i].image = options[i].image; }
			}
			BindDropdownAction(dd, ownerOfDropdownHandler, action);
		}
		public static void BindDropdownAction(TMP_Dropdown dd, object ownerOfDropdownHandler, Action<int> action) {
			dd.ClearOptions();
			//Show.Log("set " + options.Count + " opt : " + dd + "(" + dd.options.Count + ")\n" + options.Stringify(pretty: true));
#if UNITY_EDITOR
			UnityEngine.Object uObj = ownerOfDropdownHandler as UnityEngine.Object;
			if (uObj != null) {
				EventBind.IfNotAlready(dd.onValueChanged, uObj, action.Method.Name);
				return;
			}
#endif
			dd.onValueChanged.AddListener(action.Invoke);
		}
		public void HandleDropdown(int index) {
			HandleDropdown(index, options, GetComponent<TMP_Dropdown>(), ref lastValue);
		}
		public static void HandleDropdown(int index, IList<ModalConfirmation.Entry> options, TMP_Dropdown dd, ref int lastDropdownValue) {
			if (index < 0 || index >= options.Count) {
				Show.Error("index " + index + " out of range [0, " + options.Count + ")");
			}
			ModalConfirmation.Entry e = options[index];
			UnityEvent ue = e.selectionAction;
			if (ue != null) { ue.Invoke(); }
			if (e.eventOnly) {
				dd.SetValueWithoutNotify(lastDropdownValue);
			} else {
				lastDropdownValue = index;
			}
		}
	}
}