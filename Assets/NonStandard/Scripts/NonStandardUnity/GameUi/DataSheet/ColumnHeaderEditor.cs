using UnityEngine;
using TMPro;
namespace NonStandard.GameUi.DataSheet {

	public class ColumnHeaderEditor : MonoBehaviour {
		public GameObject columnHeaderObject;
		private ColumnHeader cHeader;

		[System.Serializable] public struct ValidColumnEntry {
			public string name;
			public GameObject uiField;
		}
		public ValidColumnEntry[] columnTypes;

		// TODO when this is changed, it should compile a Token. errors should make the field pink, and display the error popup. if valid, refresh valueType dropdown
		public TMP_InputField scriptValue;
		// TODO should only appear if scriptValue is focused
		public GameObject errorPopup;
		// TODO editing this changes the label in cHeader.columnSetting.data.label, and UiText.SetText(cHeader.gameObject, label)
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

		public void ActivateOn(GameObject columnHeaderObject) {
			cHeader = columnHeaderObject.GetComponent<ColumnHeader>();
			//cHeader.columnSetting.data.label
		}
	}
}
