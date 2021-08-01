using NonStandard.Process;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.Ui {
	public class ListUi : MonoBehaviour {

		public ListItemUi prefab_item;
		public string[] itemNames;
		bool refreshing = false;

		public void Refresh() {
			if (refreshing) return;
			refreshing = true;
			Proc.Delay(100, () => {
				refreshing = false;
				LayoutGroup lg = GetComponent<LayoutGroup>();
				lg.enabled = false;
				lg.enabled = true;
			});
		}

		public void SetItems(object[] items, string[] names, Action[] onButton) {
			ClearItems();
			this.itemNames = names;
			for (int i = 0; i < names.Length; ++i) {
				AddItem(items != null ? items[i] : null, names[i],
					onButton != null ? onButton[i] : null, prefab_item);
			}
		}

		public void ClearItems() { RemoveItem(null); }

		public ListItemUi AddItem(object item, string text, Action onButton, ListItemUi prefab = null) {
			if (prefab == null) { prefab = prefab_item; }
			GameObject newItem = Instantiate(prefab.gameObject);
			newItem.SetActive(true);
			ListItemUi li = newItem.GetComponent<ListItemUi>();
			li.name = text;
			li.Text = text;
			li.item = item;
			if (li.button != null && onButton != null) {
				li.button.onClick.AddListener(onButton.Invoke);
				//li.button.enabled = true;
				//Image img = li.GetComponentInChildren<Image>();
				//if (img != null) { img.enabled = true; }
			}
			newItem.transform.SetParent(transform, false);
			Refresh();
			return li;
		}
		public int IndexOf(object item) {
			Transform t = transform;
			for (int i = 0; i < t.childCount; ++i) {
				Transform child = t.GetChild(i);
				ListItemUi liui = child.GetComponent<ListItemUi>();
				if (child != prefab_item.transform && (item == null ||
					liui.item == item || (liui.item == null && child.name == item as string))) {
					return i;
				}
			}
			return -1;
		}
		public ListItemUi GetItemUi(int index) { return transform.GetChild(index).GetComponent<ListItemUi>(); }
		public ListItemUi GetListItemUi(object item) {
			int i = IndexOf(item);
			if (i < 0) { return null; }
			return transform.GetChild(i).GetComponent<ListItemUi>();
		}
		public bool RemoveItem(object item) {
			int i = IndexOf(item);
			if (i < 0) { return false; }
			Transform child = transform.GetChild(i);
			Destroy(child.gameObject);
			return true;
		}

		void Awake() {
			if (prefab_item == null) { prefab_item = GetComponentInChildren<ListItemUi>(); }
			if (prefab_item.transform.parent == transform) { prefab_item.gameObject.SetActive(false); }
			SetItems(null, itemNames, null);
		}
	}
}