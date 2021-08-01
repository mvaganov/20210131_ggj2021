using NonStandard.GameUi.Inventory;
using NonStandard.Ui;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryCollector : MonoBehaviour {
		public Inventory inventory;
		public bool autoPickup = true;
		public ListItemUi AddItem(GameObject itemObject) {
			InventoryItem item = itemObject.GetComponent<InventoryItem>();
			item.addToInventoryEvent?.Invoke(gameObject);
			return inventory.AddItem(itemObject);
		}
		public void RemoveItem(GameObject itemObject) {
			inventory.RemoveItem(itemObject);
		}
	}
}