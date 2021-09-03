using UnityEngine;
using TMPro;
using NonStandard.Ui;
using NonStandard.Data.Parse;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NonStandard.GameUi.DataSheet {

	public class ColumnHeaderEditor : MonoBehaviour {
		public ModalConfirmation confirmRemoveUi;
		public GameObject columnHeaderObject;
		private ColumnHeader cHeader;
		public UnityDataSheet uds;
		public int column;
		[System.Serializable] public struct ValidColumnEntry {
			public string name;
			public GameObject uiField;
		}
		public ValidColumnEntry[] columnTypes;

		public Color errorColor = new Color(1,.75f,.75f);
		protected Color defaultColor;
		// TODO when this is changed, it should compile a Token. errors should make the field pink, and display the error popup. if valid, refresh valueType dropdown
		public TMP_InputField scriptValue;
		// TODO should only appear if scriptValue is focused
		public GameObject errorPopup;
		public TMP_InputField columnLabel;
		// TODO pick option from validColumnTypes
		public TMP_Dropdown fieldType;
		// TODO another scripted value. should also use error popup
		public TMP_InputField defaultValue;
		// TODO generate based on scriptValue. if type is ambiguous, offer [string, number, integer, Token]
		public TMP_Dropdown valueType;
		// TODO change cHeader.columnSetting.data.width, refresh rows
		public TMP_InputField columnWidth;
		// TODO be smart. ignore erroneous values. move column and refresh on change.
		public TMP_InputField columnIndex;
		// TODO confirm dialog. if confirmed, remove from UnityDataSheet and update everything
		public Button trashColumn;

		public void Start() {
			defaultColor = scriptValue.GetComponent<Image>().color;
		}

		public void SetColumnHeader(ColumnHeader columnHeader, UnityDataSheet uds, int column) {
			this.uds = uds;
			this.column = column;
			cHeader = columnHeader;
			// setup script value
			scriptValue.onValueChanged.RemoveAllListeners();
			scriptValue.text = cHeader.columnSetting.fieldToken.Stringify();
			scriptValue.onValueChanged.AddListener(OnScriptValueEdit);
			// setup column label
			columnLabel.onValueChanged.RemoveAllListeners();
			columnLabel.text = cHeader.columnSetting.data.label;
			columnLabel.onValueChanged.AddListener(OnLabelEdit);
			// setup column width
			columnWidth.onValueChanged.RemoveAllListeners();
			columnWidth.text = cHeader.columnSetting.data.width.ToString();
			columnWidth.onValueChanged.AddListener(OnColumnWidthEdit);
			// setup column index
			columnIndex.onValueChanged.RemoveAllListeners();
			columnIndex.text = column.ToString();
			columnIndex.onValueChanged.AddListener(OnIndexEdit);
			// setup type options dropdown
			List<ModalConfirmation.Entry> entries = new List<ModalConfirmation.Entry>();
			int currentIndex = -1;
			for (int i = 0; i < columnTypes.Length; ++i) {
				entries.Add(new ModalConfirmation.Entry(columnTypes[i].name, null));
				if (cHeader.columnSetting.data.uiBase.name.StartsWith(columnTypes[i].uiField.name)) {
					currentIndex = i;
				}
			}
			Show.Log(currentIndex+" "+ cHeader.columnSetting.data.uiBase.name);
			DropDownEvent.PopulateDropdown(fieldType, entries, this, SetFieldType);
			if (currentIndex >= 0) {
				//fieldType.itemText.text = entries[currentIndex].text;
				fieldType.captionText.text = entries[currentIndex].text;
				fieldType.SetValueWithoutNotify(currentIndex);
			}
		}
		public void SetFieldType(int index) {
			Show.Log("SetFieldType "+index);
			cHeader.columnSetting.data.uiBase = columnTypes[index].uiField;
			uds.RefreshRowsAndColumns();
		}
		public void OnLabelEdit(string text) {
			cHeader.columnSetting.data.label = text;
			UiText.SetText(cHeader.gameObject, text);
		}
		public void OnColumnWidthEdit(string text) {
			float oldWidth = cHeader.columnSetting.data.width;
			if (float.TryParse(text, out float newWidth)) {
				if (newWidth > 0 && newWidth < 2048) {
					cHeader.columnSetting.data.width = newWidth;
					uds.ResizeColumnWidth(column, oldWidth, newWidth);
				} else {
					SetErrorPopup("invalid width: "+newWidth+". Requirement: 0 < value < 2048", true, columnWidth.gameObject);
					return;
				}
			}
			uds.RefreshRowsAndColumns();
			HidePopup();
		}
		public void OnIndexEdit(string text) {
			int oldIndex = cHeader.transform.GetSiblingIndex();
			if (oldIndex != column) {
				SetErrorPopup("WOAH PROBLEM! column " + column + " is not the same as childIndex " + oldIndex, true, columnIndex.gameObject);
				return;
			}
			if (int.TryParse(text, out int newIndex)) {
				if (newIndex > 0 && newIndex < cHeader.transform.parent.childCount) {
					uds.MoveColumn(oldIndex, newIndex);
					column = newIndex;
				} else {
					SetErrorPopup("invalid index: "+newIndex+". Requirement: 0 < index < "+ cHeader.transform.parent.childCount, true, columnIndex.gameObject);
					return;
				}
			}
			uds.RefreshRowsAndColumns();
			HidePopup();
		}
		private GameObject lastErrorInput = null;
		public void SetErrorPopup(string text, bool isError, GameObject errorInputObject) {
			Debug.Log(text);
			UiText.SetText(errorPopup, text);
			errorPopup.gameObject.SetActive(true);
			Image img;
			if (lastErrorInput != null) {
				img = lastErrorInput.GetComponent<Image>();
				img.color = defaultColor;
			}
			img = errorInputObject.GetComponent<Image>();
			if (img.color != errorColor) { defaultColor = img.color; }
			if (isError) {
				img.color = errorColor;
			} else {
				img.color = defaultColor;
			}
			lastErrorInput = errorInputObject;
		}
		public void HidePopup() {
			errorPopup.gameObject.SetActive(false);
			if (lastErrorInput != null) {
				Image img = lastErrorInput.GetComponent<Image>();
				img.color = defaultColor;
			}
			lastErrorInput = null;
		}
		public void OnScriptValueEdit(string text) {
			Tokenizer tokenizer = new Tokenizer();
			tokenizer.Tokenize(text);
			GameObject go = scriptValue.gameObject;
			// parse errors
			if (tokenizer.HasError()) { SetErrorPopup(tokenizer.GetErrorString(), true, go); return; }
			// just one token
			if (tokenizer.tokens.Count > 1) { SetErrorPopup("too many tokens: should only be one value", true, go); return; }
			// try to set the field
			cHeader.columnSetting.SetFieldToken(tokenizer.tokens[0], tokenizer);
			// valid variable path
			if (tokenizer.HasError()) { SetErrorPopup(tokenizer.GetErrorString(), true, go); return; }
			// refresh column values
			uds.RefreshColumnText(column, tokenizer);
			// failed to set values
			if (tokenizer.HasError()) { SetErrorPopup(tokenizer.GetErrorString(), true, go); return; }
			// success!
			HidePopup();
		}
		public void ColumnRemove() {
			ModalConfirmation ui = confirmRemoveUi;
			if (ui == null) { ui = Global.GetComponent<ModalConfirmation>(); }
			Udash.ColumnSetting cS = uds.GetColumn(column);
			ui.OkCancel("Are you sure you want to delete column \"" + cS.data.label + "\"?", () => { uds.RemoveColumn(column); });
			uds.Sort();
		}
	}
}
