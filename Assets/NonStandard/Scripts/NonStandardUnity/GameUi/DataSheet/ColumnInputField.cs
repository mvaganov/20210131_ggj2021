using NonStandard;
using NonStandard.Data.Parse;
using NonStandard.Ui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnInputField : MonoBehaviour
{
	public object target;
	public string path;

	public void Init() {
		path = path.Trim();
		while (path.StartsWith("(") && path.EndsWith(")")) { path = path.Substring(1, path.Length - 2).Trim(); }
	}

	public void AssignFromText(string text) {
		UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
		int col = transform.GetSiblingIndex();
		int row = uds.GetRow(transform.parent.gameObject);
		Udash.ColumnData column = uds.GetColumn(col);
		Token t = column.fieldToken;
		string shown = t.GetAsSmallText();
		Show.Log("need to change "+uds.list[row]+" element "+shown);
		//uds.SetSortState(col, (SortState)index);
	}
}
