using NonStandard;
using NonStandard.Data;
using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.DataSheet {
	public class ColumnHeader : MonoBehaviour {
		public void OnSortDropdownValueChanged(int index) {
			UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
			int col = transform.GetSiblingIndex();
			//Show.Log("value: "+index+" for "+col);
			uds.SetSortState(col, (SortState)index);
		}
	}
}