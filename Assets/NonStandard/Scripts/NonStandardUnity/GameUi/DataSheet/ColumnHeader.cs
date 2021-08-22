using NonStandard.Data;
using NonStandard.Ui;
using UnityEngine;
using TMPro;

namespace NonStandard.GameUi.DataSheet {
	public class ColumnHeader : MonoBehaviour {
		int previousColumn = -1;
		public void OnSortDropdownValueChanged(int index) {
			UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
			int col = transform.GetSiblingIndex();
			//Show.Log("value: "+index+" for "+col);
			if (index < (int)SortState.Count) {
				uds.SetSortState(col, (SortState)index);
				previousColumn = index;
			} else {
				TMP_Dropdown dd = GetComponent<TMP_Dropdown>();
				switch (index) {
				case 3: uds.MoveColumn(col); break;
				case 4: uds.ResizeColumn(col); break;
				case 5: uds.EditColumn(col); break;
				case 6: uds.RemoveColumn(col); break;
				}
				dd.value = previousColumn;
			}
		}
	}
}