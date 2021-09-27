using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using NonStandard.Utility.UnityEditor;
using System;
using NonStandard.Data;

namespace NonStandard.Ui {
	public class DropDownEvent : MonoBehaviour {
		public List<ModalConfirmation.Entry> options;
		[ContextMenuItem("PopulateDropdown", "PopulateDropdown")]
		public bool initOptionsOnStart;
		public bool connectUiText;
		int lastValue = -1;

		void Start() {
			if (initOptionsOnStart) { PopulateDropdown(); }
			if (connectUiText) {
				UiText onSelection = GetComponent<UiText>();
				if (onSelection != null && onSelection.OnSetText.GetPersistentEventCount() > 0) {
					TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
					BindDropdownAction(dd, this, HandleDropdown_OnSelection);
					HandleDropdown_OnSelection(dd.value);
				}
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
			PopulateDropdown(dd, () => options.Count, i => options[i], ownerOfDropdownHandler, action, currentIndex, null);
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
			PopulateDropdown(dd, () => options.Count, i => options[i].text, ownerOfDropdownHandler, action, currentIndex, null);
		}
		public static void PopulateDropdown(TMP_Dropdown dd, Func<int> getCount, Func<int,string> getText, object ownerOfDropdownHandler, Action<int> dropdownHandler, int currentIndex = -1, Func<int, Sprite> getImage = null) {
			dd.ClearOptions();
			List<TMP_Dropdown.OptionData> opts = dd.options;
			for (int i = 0; i < getCount(); ++i) {
				string text = getText(i);
				Sprite image = getImage != null ? getImage(i) : null;
				if (i >= opts.Count) { opts.Add(new TMP_Dropdown.OptionData(text, image)); }
				if (opts[i].text != text) { opts[i].text = text; }
				if (opts[i].image != image) { opts[i].image = image; }
			}
			BindDropdownAction(dd, ownerOfDropdownHandler, dropdownHandler);
			if (currentIndex >= 0) {
				dd.captionText.text = opts[currentIndex].text;
				dd.SetValueWithoutNotify(currentIndex);
				UiTextUpdate(dd);
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
			UiText onSelection = GetComponent<UiText>();
			if (onSelection != null) {
				HandleDropdown(index, dd, onSelection.OnSetText);
			}
			lastValue = index;
		}
		public static void UiTextUpdate(TMP_Dropdown dd) {
			UiText onSelection = dd.GetComponent<UiText>();
			if (onSelection == null) return;
			UnityEvent_string setText = onSelection.setText;
			HandleDropdown(dd.value, dd, setText);
		}
		public static void HandleDropdown(int index, TMP_Dropdown dd, UnityEvent_string stringNotify) {
			if (stringNotify.GetPersistentEventCount() == 0) { return; }
			if (index >= 0 && index < dd.options.Count) {
				TMP_Dropdown.OptionData o = dd.options[index];
				stringNotify.Invoke(o.text);
			}
		}
		public static void HandleDropdown(int index, IList<ModalConfirmation.Entry> options, TMP_Dropdown dd, ref int lastDropdownValue) {
			ModalConfirmation.Entry e = (options != null && index < options.Count) ? options[index] : null;
			UnityEvent ue = e != null ? e.selectionAction : null;
			if (ue != null) { ue.Invoke(); }
			if (e != null && e.eventOnly) {
				dd.SetValueWithoutNotify(lastDropdownValue);
			} else {
				lastDropdownValue = index;
				UiTextUpdate(dd);
			}
		}
	}
}