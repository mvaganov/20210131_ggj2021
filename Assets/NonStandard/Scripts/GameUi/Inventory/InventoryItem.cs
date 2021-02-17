using System;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItem : MonoBehaviour {
		public string itemName;
		public Collider _pickupCollider;
		public Action<Inventory> onAddToInventory;
		public Action<Inventory> onRemoveFromInventory;
		public void Start() {
			if (_pickupCollider == null) { _pickupCollider = GetComponent<Collider>(); }
			CollisionTrigger trigger = _pickupCollider.gameObject.AddComponent<CollisionTrigger>();
			trigger.onTrigger.AddListener(_OnTrigger);
		}

		void _OnTrigger(GameObject other) {
			if (other == Global.Instance().gameObject) return;
			Inventory inv = other.GetComponent<Inventory>();
			if (inv != null && inv.autoPickup) {
				inv.AddItem(gameObject);
			}
		}

		public void OnEnable() {
			if (_pickupCollider == null) return;
			CollisionTrigger trigger = _pickupCollider.GetComponent<CollisionTrigger>();
			trigger.enabled = false;
			NonStandard.Clock.setTimeout(() => { if (trigger != null) trigger.enabled = true; }, 500);
		}
	}
}