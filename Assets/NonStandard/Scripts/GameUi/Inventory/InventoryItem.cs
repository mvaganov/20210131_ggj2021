using NonStandard.Utility;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItem : MonoBehaviour {
		public string itemName;
		public Collider _pickupCollider;
		public Action<Inventory> onAddToInventory;
		public Action<Inventory> onRemoveFromInventory;
		public UnityEvent addToInventoryEvent;
		public void Start() {
			if (_pickupCollider == null) { _pickupCollider = GetComponent<Collider>(); }
			CollisionTrigger trigger = _pickupCollider.gameObject.AddComponent<CollisionTrigger>();
			trigger.onTrigger.AddListener(_OnTrigger);
		}

		void _OnTrigger(GameObject other) {
			if (other == Global.Instance().gameObject) return;
			InventoryCollector inv = other.GetComponent<InventoryCollector>();
			if (inv != null && inv.autoPickup && inv.inventory) {
				inv.AddItem(gameObject);
			}
		}

		public void OnEnable() {
			if (_pickupCollider == null) return;
			CollisionTrigger trigger = _pickupCollider.GetComponent<CollisionTrigger>();
			trigger.enabled = false;
			GameClock.Delay(500, () => { if (trigger != null) trigger.enabled = true; });
			//NonStandard.Clock.setTimeout(() => { if (trigger != null) trigger.enabled = true; }, 500);
		}
	}
}