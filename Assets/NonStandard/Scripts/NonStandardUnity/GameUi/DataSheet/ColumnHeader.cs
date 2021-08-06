using NonStandard;
using NonStandard.Data;
using NonStandard.Ui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColumnHeader : MonoBehaviour {
	UnityDataSheet uds;
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
