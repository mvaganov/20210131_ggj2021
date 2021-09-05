using NonStandard.Utility.UnityEditor;
using UnityEngine;
using NonStandard.Data;

namespace NonStandard.GameUi.DataSheet {
	public class ColumnInputField : MonoBehaviour {
		public void Reset() {
			TMPro.TMP_InputField input = GetComponent<TMPro.TMP_InputField>();
			EventBind.IfNotAlready(input.onValueChanged, this, nameof(AssignFromText));
			//EventBind.IfNotAlready(input.onEndEdit, this, nameof(Sort));
		}
		public void Sort(string text) {
			UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
			uds?.Sort();
		}
		public void AssignFromText(string text) {
			//TMPro.TMP_InputField input = GetComponent<TMPro.TMP_InputField>();
			//Show.Log("assign "+text+" instead of "+input.text);
			UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
			if (uds == null) {
				// this happens the first instant that the input field is created, before it is connected to the UI properly
				//Show.Log("missing "+nameof(UnityDataSheet)+" for "+transform.HierarchyPath());
				return;
			}
			int col = transform.GetSiblingIndex();
			int row = uds.GetRowIndex(transform.parent.gameObject);
			Udash.ColumnSetting column = uds.GetColumn(col);
			if (column.canEdit) {
				object value = text;
				if (column.type != null) {
					CodeConvert.Convert(ref value, column.type);
				}
				column.SetValue(uds.GetItem(row), value);
				uds.data[row, col] = value;
			}
		}
	}
}
