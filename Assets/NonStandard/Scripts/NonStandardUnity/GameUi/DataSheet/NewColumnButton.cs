using NonStandard.Ui;
using System;
using UnityEngine;


namespace NonStandard.GameUi.DataSheet {
	public class NewColumnButton : MonoBehaviour {
		public void AddColumn() {
			UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
			uds.AddColumn();
			//Show.Log(uds);
			int index = uds.IndexOf(o => o != null);
			if (index >= 0) {
				object exampleEntity = uds.GetItem(index);
				Type t = exampleEntity.GetType();
				Show.Log("should make column options for "+t);
				Show.Log("TODO list fields and properties");
			}
		}
	}
}