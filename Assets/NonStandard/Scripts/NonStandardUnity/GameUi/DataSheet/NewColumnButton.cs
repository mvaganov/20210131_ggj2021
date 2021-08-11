using NonStandard.Ui;
using System;
using UnityEngine;


namespace NonStandard.GameUi.DataSheet {
	public class NewColumnButton : MonoBehaviour {
		UnityDataSheet uds;
		private void Start() {
			uds = GetComponentInParent<UnityDataSheet>();
			//Show.Log("UDS: "+uds);
		}
		public void AddColumn() {
			//Show.Log(uds);
			int index = uds.list.FindIndex(o => o != null);
			if (index >= 0) {
				object exampleEntity = uds.list[index];
				Type t = exampleEntity.GetType();
				Show.Log("should make column options for "+t);
				Show.Log("TODO list fields and properties");
			}
		}
	}
}