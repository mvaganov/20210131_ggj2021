using NonStandard.Data;
using NonStandard.Ui;
using UnityEngine;
using TMPro;

namespace NonStandard.GameUi.DataSheet {
	public class ColumnHeader : MonoBehaviour {
		[ContextMenuItem("PopulateDropdown", "PopulateDropdown")] public GameObject editUi;
		public ModalConfirmation confirmRemoveUi;
		public Udash.ColumnSetting columnSetting;
		int Col() { return transform.GetSiblingIndex(); }
		TMP_Dropdown DD() { return GetComponent<TMP_Dropdown>(); }
		UnityDataSheet UDS() { return GetComponentInParent<UnityDataSheet>(); }
		public void ColumnNoSort() { SetSortMode((int)SortState.None); }
		public void ColumnSortAscend() { SetSortMode((int)SortState.Ascending); }
		public void ColumnSortDescend() { SetSortMode((int)SortState.Descening); }
		public void ColumnEdit() {
			editUi.SetActive(true);
			editUi.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			UDS().EditColumn(Col());
		}
		public void ColumnRemove() {
			ModalConfirmation ui = confirmRemoveUi;
			if (ui == null) { ui = Global.GetComponent<ModalConfirmation>(); }
			Udash.ColumnSetting cS = UDS().GetColumn(Col());
			ui.OkCancel("Are you sure you want to delete column \"" + cS.data.label+"\"?", ()=> { UDS().RemoveColumn(Col()); });
		}
		public void SetSortMode(int sortModeIndex) {
			if (sortModeIndex < 0 || sortModeIndex >= (int)SortState.Count) { return; }
			UnityDataSheet uds = UDS();
			int col = transform.GetSiblingIndex();
			uds.SetSortState(col, (SortState)sortModeIndex);
		}
		public void PopulateDropdown() {
			DropDownEvent dde = GetComponent<DropDownEvent>();
			if (dde == null) { dde = gameObject.AddComponent<DropDownEvent>(); }
			dde.options = new System.Collections.Generic.List<ModalConfirmation.Entry> {
				new ModalConfirmation.Entry("No Sort", this, nameof(ColumnNoSort)),
				new ModalConfirmation.Entry("Sort Ascending", this, nameof(ColumnSortAscend)),
				new ModalConfirmation.Entry("Sort Descending", this, nameof(ColumnSortDescend)),
				new ModalConfirmation.Entry("Edit Column", this, nameof(ColumnEdit), false),
				new ModalConfirmation.Entry("Remove Column", this, nameof(ColumnRemove), false),
			};
			dde.PopulateDropdown();
		}
	}
}