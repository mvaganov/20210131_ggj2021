using NonStandard.GameUi.Particles;
using NonStandard.Ui;
using NonStandard.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class Inventory : MonoBehaviour {
		List<GameObject> items;
		public ListUi inventoryUi;
		public UnityEvent_GameObject onAddItem;
		public UnityEvent_GameObject onRemoveItem;
		public List<GameObject> GetItems() { return items; }
		private void Awake() {
			Global.Get<InventoryManager>().Register(this);
		}
		public ListItemUi AddItem(GameObject itemObject) {
			if (items == null) { items = new List<GameObject>(); }
			if (inventoryUi != null) { ListItemUi result = inventoryUi.GetListItemUi(itemObject); if (result != null) return result; }
			items.Add(itemObject);
			InventoryItem item = itemObject.GetComponent<InventoryItem>();
			itemObject.SetActive(false);
			item.onAddToInventory?.Invoke(this);
			item.addToInventoryEvent?.Invoke();
			Vector3 playerLoc = Global.Get<CharacterControlManager>().localPlayerInterfaceObject.transform.position;
			Vector3 localPosition = itemObject.transform.position - playerLoc;
			itemObject.transform.SetParent(transform);
			itemObject.transform.localPosition = localPosition;
			//Show.Log("POS IN" + localPosition);
			string name = item != null ? item.itemName : null;
			if (string.IsNullOrEmpty(name)) { name = itemObject.name; }
			onAddItem.Invoke(itemObject);
			if (inventoryUi == null) { return null; }
			return inventoryUi.AddItem(itemObject, name, () => {
				RemoveItem(itemObject);
			});
		}
		public GameObject FindItem(string name) {
			if (items == null) return null;
			// TODO wildcard search
			GameObject found = items.Find(i => i.name == name);
			return found;
		}
		public GameObject RemoveItem(string name) {
			GameObject go = FindItem(name);
			if (go != null) {
				RemoveItem(go);
				onRemoveItem.Invoke(go);
			}
			return go;
		}
		public void RemoveItem(GameObject itemObject) {
			if (items != null) { items.Remove(itemObject); }
			itemObject.SetActive(true);
			Vector3 localPos = itemObject.transform.localPosition;
			//Show.Log("POS out " + localPos);
			Vector3 playerLoc = Global.Get<CharacterControlManager>().localPlayerInterfaceObject.transform.position;
			itemObject.transform.SetParent(null);
			itemObject.transform.position = playerLoc + localPos;
			Rigidbody rb = itemObject.GetComponent<Rigidbody>();
			if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
			inventoryUi.RemoveItem(itemObject);
			InventoryItem item = itemObject.GetComponent<InventoryItem>();
			item.onRemoveFromInventory?.Invoke(this);
		}

		public void RemoveAllItems() {
			if (items == null) return;
			for(int i = items.Count-1; i >= 0; --i) {
				GameObject go = items[i];
				RemoveItem(go);
				Destroy(go);
			}
			items.Clear();
		}
	}
}