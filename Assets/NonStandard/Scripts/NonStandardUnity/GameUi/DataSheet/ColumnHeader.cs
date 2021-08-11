using NonStandard;
using NonStandard.Data;
using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.DataSheet {
	public class ColumnHeader : MonoBehaviour {
		UnityDataSheet uds;
		// TODO remove column, add column, resize column, rearrange column
		private void Start() {
			uds = GetComponentInParent<UnityDataSheet>();
			//Show.Log("UDS: "+uds);
		}

		public void OnSortDropdownValueChanged(int index) {
			int col = transform.GetSiblingIndex();
			//Show.Log("value: "+index+" for "+col);
			uds.SetSortState(col, (SortState)index);
		}
	}
}