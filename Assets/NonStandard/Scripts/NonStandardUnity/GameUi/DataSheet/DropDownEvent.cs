using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using NonStandard.Utility.UnityEditor;
using NonStandard.Extension;
using System;
using NonStandard.Data;

namespace NonStandard.Ui {
	public class DropDownEvent : MonoBehaviour {
		public List<ModalConfirmation.Entry> options;
		[ContextMenuItem("PopulateDropdown", "PopulateDropdown")]
		public bool initOptionsOnStart;
		int lastValue = -1;

		public UnityEvent_string onSelection;

		void Start() {
			if (initOptionsOnStart) { PopulateDropdown(); }
			if (onSelection.GetPersistentEventCount() > 0) {
				TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
				BindDropdownAction(dd, this, HandleDropdown_OnSelection);
				HandleDropdown_OnSelection(dd.value);
			}
		}
		public void Refresh_OnSelection() {
			TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
			HandleDropdown_OnSelection(dd.value);
		}
		public void PopulateDropdown() {
			TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
			PopulateDropdown(dd, options, this, HandleDropdown_Options);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dd"></param>
		/// <param name="options"></param>
		/// <param name="ownerOfDropdownHandler"></param>
		/// <param name="action"></param>
		/// <param name="currentIndex">sets the dropdown to this value. if negative value, this is ignored</param>
		public static void PopulateDropdown(TMP_Dropdown dd, IList<string> options, object ownerOfDropdownHandler, Action<int> action, int currentIndex = -1) {
			dd.ClearOptions();
			List<TMP_Dropdown.OptionData> opts = dd.options;
			for (int i = 0; i < options.Count; ++i) {
				if (i >= opts.Count) { opts.Add(new TMP_Dropdown.OptionData(options[i], null)); }
				if (opts[i].text != options[i]) { opts[i].text = options[i]; }
				if (opts[i].image != null) { opts[i].image = null; }
			}
			BindDropdownAction(dd, ownerOfDropdownHandler, action);
			if (currentIndex >= 0) {
				dd.captionText.text = opts[currentIndex].text;
				dd.SetValueWithoutNotify(currentIndex);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dd"></param>
		/// <param name="options"></param>
		/// <param name="ownerOfDropdownHandler"></param>
		/// <param name="action"></param>
		/// <param name="currentIndex">sets the dropdown to this value. if negative value, this is ignored</param>
		public static void PopulateDropdown(TMP_Dropdown dd, IList<ModalConfirmation.Entry> options, object ownerOfDropdownHandler, Action<int> action, int currentIndex = -1) {
			dd.ClearOptions();
			List<TMP_Dropdown.OptionData> opts = dd.options;
			for (int i = 0; i < options.Count; ++i) {
				if (i >= opts.Count) { opts.Add(new TMP_Dropdown.OptionData(options[i].text, options[i].image)); }
				if (opts[i].text != options[i].text) { opts[i].text = options[i].text; }
				if (opts[i].image != options[i].image) { opts[i].image = options[i].image; }
			}
			BindDropdownAction(dd, ownerOfDropdownHandler, action);
			if (currentIndex >= 0) {
				dd.captionText.text = opts[currentIndex].text;
				dd.SetValueWithoutNotify(currentIndex);
			}
		}
		public static void BindDropdownAction(TMP_Dropdown dd, object ownerOfDropdownHandler, Action<int> action) {
			if (ownerOfDropdownHandler == null || action == null) return;
			//Show.Log("set " + options.Count + " opt : " + dd + "(" + dd.options.Count + ")\n" + options.Stringify(pretty: true));
#if UNITY_EDITOR
			UnityEngine.Object uObj = ownerOfDropdownHandler as UnityEngine.Object;
			if (uObj != null) {
				EventBind.IfNotAlready(dd.onValueChanged, uObj, action.Method.Name);
				return;
			}
#endif
			dd.onValueChanged.RemoveAllListeners();
			dd.onValueChanged.AddListener(action.Invoke);
		}
		public void HandleDropdown_Options(int index) {
			TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
			HandleDropdown(index, options, dd, ref lastValue);
		}
		public void HandleDropdown_OnSelection(int index) {
			TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
			HandleDropdown(index, dd, onSelection);
			lastValue = index;
		}
		public static void HandleDropdown(int index, TMP_Dropdown dd, UnityEvent_string stringNotify) {
			if (stringNotify.GetPersistentEventCount() == 0) { return; }
			TMP_Dropdown.OptionData o = dd.options[index];
			stringNotify.Invoke(o.text);
		}
		public static void HandleDropdown(int index, IList<ModalConfirmation.Entry> options, TMP_Dropdown dd, ref int lastDropdownValue) {
			int optCount = options != null ? options.Count : dd.options.Count;
			if (index < 0 || index >= options.Count) {
				Show.Error("index " + index + " out of range [0, " + options.Count + ")");
			}

			ModalConfirmation.Entry e = (options != null && index < options.Count) ? options[index] : null;
			//if (e != null) return;
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