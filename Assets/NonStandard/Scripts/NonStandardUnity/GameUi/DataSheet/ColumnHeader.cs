using NonStandard.Data;
using NonStandard.Ui;
using UnityEngine;
using TMPro;
using NonStandard.Extension;
using NonStandard.Process;

namespace NonStandard.GameUi.DataSheet {
	public class ColumnHeader : MonoBehaviour {
		Transform originalParent;
		private void Awake() {
			originalParent = editUi.transform.parent;
		}
		[ContextMenuItem("PopulateDropdown", "PopulateDropdown")] public GameObject editUi;
		public Udash.ColumnSetting columnSetting;
		int Col() { return transform.GetSiblingIndex(); }
		TMP_Dropdown DD() { return GetComponent<TMP_Dropdown>(); }
		UnityDataSheet UDS() { return GetComponentInParent<UnityDataSheet>(); }
		public void ColumnNoSort() { SetSortMode((int)SortState.None); }
		public void ColumnSortAscend() { SetSortMode((int)SortState.Ascending); }
		public void ColumnSortDescend() { SetSortMode((int)SortState.Descening); }
		public void Close() {
			gameObject.SetActive(false);
		}
		public void ColumnEdit() {
			editUi.SetActive(true);
			editUi.transform.SetParent(transform, false);
			Debug.Log("path is " + editUi.transform.HierarchyPath());
			editUi.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			ColumnHeaderEditor chEditor = editUi.GetComponent<ColumnHeaderEditor>();
			chEditor.SetColumnHeader(this, UDS(), Col());
			Proc.Enqueue(() => {
				editUi.transform.SetParent(originalParent, true);
			});
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
				//new ModalConfirmation.Entry("Remove Column", this, nameof(ColumnRemove), false),
			};
			dde.PopulateDropdown();
		}
	}
}