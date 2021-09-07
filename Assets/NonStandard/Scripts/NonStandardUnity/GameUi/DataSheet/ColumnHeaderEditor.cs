using UnityEngine;
using TMPro;
using NonStandard.Ui;
using NonStandard.Data.Parse;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Reflection;
using NonStandard.Data;

namespace NonStandard.GameUi.DataSheet {

	// TODO an optional extra field for icon, to replace the button/cell image
	public class ColumnHeaderEditor : MonoBehaviour {
		public ModalConfirmation confirmRemoveUi;
		public GameObject columnHeaderObject;
		private ColumnHeader cHeader;
		public UnityDataSheet uds;
		public int column;
		public Type expectedValueType;
		[System.Serializable] public struct ValidColumnEntry {
			public string name;
			public GameObject uiField;
		}
		public List<ValidColumnEntry> columnTypes = new List<ValidColumnEntry>();

		public Color errorColor = new Color(1,.75f,.75f);
		// compiles Token. errors make the field pink, and display the error popup. if valid, refresh valueType dropdown
		public TMP_InputField scriptValue;
		public UiHoverPopup popup;
		public TMP_InputField columnLabel;
		// TODO pick option from validColumnTypes
		public TMP_Dropdown fieldType;
		// TODO another scripted value. should also use error popup
		public TMP_InputField defaultValue;
		// TODO generate based on scriptValue. if type is ambiguous, offer [string, number, integer, Token]
		public TMP_Dropdown valueType;
		// change cHeader.columnSetting.data.width, refresh rows
		public TMP_InputField columnWidth;
		// ignore erroneous values. move column and refresh on change.
		public TMP_InputField columnIndex;
		// TODO confirm dialog. if confirmed, remove from UnityDataSheet and update everything
		public Button trashColumn;

		public void Start() {
			popup.defaultColor = scriptValue.GetComponent<Image>().color;
		}

		public void SetColumnHeader(ColumnHeader columnHeader, UnityDataSheet uds, int column) {
			this.uds = uds;
			this.column = column;
			cHeader = columnHeader;
			// setup script value
			scriptValue.onValueChanged.RemoveAllListeners();
			string text = cHeader.columnSetting.fieldToken.Stringify();
			scriptValue.text = text;
			scriptValue.onValueChanged.AddListener(OnScriptValueEdit);
			// implicitly setup value types dropdown
			OnScriptValueEdit(text);
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
			//List<ModalConfirmation.Entry> entries = new List<ModalConfirmation.Entry>();
			//int currentIndex = -1;
			//for (int i = 0; i < columnTypes.Count; ++i) {
			//	entries.Add(new ModalConfirmation.Entry(columnTypes[i].name, null));
			//	//Show.Log(columnTypes[i].name);
			//	if (cHeader.columnSetting.data.uiBase.name.StartsWith(columnTypes[i].uiField.name)) {
			//		currentIndex = i;
			//	}
			//}
			List<ModalConfirmation.Entry> entries = columnTypes.ConvertAll(c => new ModalConfirmation.Entry(c.name, null));
			int currentIndex = columnTypes.FindIndex(c=> cHeader.columnSetting.data.uiBase.name.StartsWith(c.uiField.name));
			//Show.Log(currentIndex+" "+ cHeader.columnSetting.data.uiBase.name);
			DropDownEvent.PopulateDropdown(fieldType, entries, this, SetFieldType, currentIndex);
		}

		public void SetFieldType(int index) {
			cHeader.columnSetting.data.uiBase = columnTypes[index].uiField;
			uds.RefreshRowAndColumnUi();
		}
		public void OnLabelEdit(string text) {
			cHeader.columnSetting.data.label = text;
			UiText.SetText(cHeader.gameObject, text);
		}
		public void OnColumnWidthEdit(string text) {
			float oldWidth = cHeader.columnSetting.data.width;
			if (float.TryParse(text, out float newWidth)) {
				if (newWidth > 0 && newWidth < 2048) {
					uds.ResizeColumnWidth(column, oldWidth, newWidth);
				} else {
					popup.Set("err", columnWidth.gameObject, "invalid width: " + newWidth + ". Requirement: 0 < value < 2048");
					return;
				}
			}
			popup.Hide();
		}
		public void OnIndexEdit(string text) {
			int oldIndex = cHeader.transform.GetSiblingIndex();
			if (oldIndex != column) {
				popup.Set("err", columnIndex.gameObject, "WOAH PROBLEM! column " + column + " is not the same as childIndex " + oldIndex);
				return;
			}
			int max = uds.GetMaximumUserColumn();
			if (int.TryParse(text, out int newIndex)) {
				if (newIndex > 0 && newIndex < max) {
					uds.MoveColumn(oldIndex, newIndex);
					column = newIndex;
				} else {
					popup.Set("err", columnIndex.gameObject,"invalid index: " + newIndex + ". Requirement: 0 < index < " + max);
					return;
				}
			}
			popup.Hide();
		}
		public void OnScriptValueEdit(string fieldScript) {
			Tokenizer tokenizer = new Tokenizer();
			tokenizer.Tokenize(fieldScript);
			GameObject go = scriptValue.gameObject;
			// parse errors
			if (tokenizer.HasError()) { popup.Set("err", go, tokenizer.GetErrorString()); return; }
			// just one token
			if (tokenizer.tokens.Count > 1) { popup.Set("err", go, "too many tokens: should only be one value"); return; }
			// try to set the field based on field script
			if(!ProcessFieldScript(tokenizer)) return;
			// refresh column values
			uds.RefreshColumnText(column, tokenizer);
			// failed to set values
			if (tokenizer.HasError()) { popup.Set("err", go, tokenizer.GetErrorString()); return; }
			// success!
			popup.Hide();
		}
		private bool ProcessFieldScript(Tokenizer tokenizer) {
			if (tokenizer.tokens.Count == 0) {
				expectedValueType = null;
				cHeader.columnSetting.editPath = null;
				return false;
			}
			object value = cHeader.columnSetting.SetFieldToken(tokenizer.tokens[0], tokenizer);
			// update the expected edit type
			SetExpectedEditType(value);
			// valid variable path
			if (tokenizer.HasError()) { popup.Set("err", scriptValue.gameObject, tokenizer.GetErrorString()); return false; }
			return true;
		}
		public void SetExpectedEditType(object sampleValue) {
			Type sampleValueType = GetEditType();
			if (sampleValueType == null) {
				// set to read only
				expectedValueType = null;
				DropDownEvent.PopulateDropdown(valueType, new string[] { "read only" }, this, null, 0);
			} else {
				if (sampleValueType != expectedValueType) {
					// set to specific type
					if (sampleValueType == typeof(object)) {
						sampleValueType = sampleValue.GetType();
						int defaultChoice = -1;
						if (defaultChoice < 0 && CodeConvert.IsIntegral(sampleValueType)) {
							defaultChoice = defaultValueTypes.FindIndex(kvp=>kvp.Key == typeof(long));
						}
						if (defaultChoice < 0 && CodeConvert.IsNumeric(sampleValueType)) {
							defaultChoice = defaultValueTypes.FindIndex(kvp => kvp.Key == typeof(double));
						}
						if (defaultChoice < 0) {// && sampleValueType == typeof(string)) {
							defaultChoice = defaultValueTypes.FindIndex(kvp => kvp.Key == typeof(string));
						}
						List<string> options = defaultValueTypes.ConvertAll(kvp => kvp.Value);
						DropDownEvent.PopulateDropdown(valueType, options, this, SetEditType, defaultChoice);
						cHeader.columnSetting.type = defaultValueTypes[defaultChoice].Key;
					} else {
						DropDownEvent.PopulateDropdown(valueType, new string[] { sampleValueType.ToString() }, this, null, 0);
						cHeader.columnSetting.type = sampleValueType;
					}
					expectedValueType = sampleValueType;
				}
			}
		}
		public Type GetEditType() {
			List<object> editPath = cHeader.columnSetting.editPath;
			if (editPath == null || editPath.Count == 0) {
				return null;
			} else {
				object lastPathComponent = editPath[editPath.Count - 1];
				switch (lastPathComponent) {
				case FieldInfo fi: return fi.FieldType;
				case PropertyInfo pi: return pi.PropertyType;
				case string s: return typeof(object);
				}
			}
			return null;
		}
		private void SetEditType(int index) { cHeader.columnSetting.type = defaultValueTypes[index].Key; }
		private static List<KeyValuePair<Type, string>> defaultValueTypes = new List<KeyValuePair<Type, string>> {
			new KeyValuePair<Type, string>(typeof(object), "unknown"),
			new KeyValuePair<Type, string>(typeof(string), "string"),
			new KeyValuePair<Type, string>(typeof(double),"number"),
			new KeyValuePair<Type, string>(typeof(long),"integer"),
			new KeyValuePair<Type, string>(typeof(Token), "script"),
			new KeyValuePair<Type, string>(null, "read only"),
		};
		public void ColumnRemove() {
			ModalConfirmation ui = confirmRemoveUi;
			if (ui == null) { ui = Global.GetComponent<ModalConfirmation>(); }
			Udash.ColumnSetting cS = uds.GetColumn(column);
			ui.OkCancel("Are you sure you want to delete column \"" + cS.data.label + "\"?", () => { uds.RemoveColumn(column); });
			uds.Sort();
		}
	}
}
