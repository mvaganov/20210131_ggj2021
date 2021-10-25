using NonStandard.Commands;
using NonStandard.Data;
using NonStandard.Utility;
using System;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryItem : MonoBehaviour {
		public string itemName;
		public Collider _pickupCollider;
		public Inventory inventory;
		public UnityEvent_GameObject addToInventoryEvent;
		public UnityEvent_GameObject removeFromInventoryEvent;
		public UnityEngine.Object data;

		public void RemoveFromCurrentInventory() {
			if (inventory == null) { return; }
			removeFromInventoryEvent?.Invoke(inventory.gameObject);
			inventory = null;
		}
		public void AddToInventory(Inventory inventory) {
			if (this.inventory == inventory) {
				Show.Warning(itemName+" being added to "+inventory.name+" again");
				return; // prevent double-add
			}
			RemoveFromCurrentInventory();
			this.inventory = inventory;
			addToInventoryEvent?.Invoke(inventory.gameObject);
		}

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
		public ScriptedDictionary GetDictionaryOfInventory() {
			GameObject go = inventory.gameObject;
			ScriptedDictionaryProxy sp;
			int sentinel = 0;
			do {
				sp = go.GetComponent<ScriptedDictionaryProxy>();
				if (sp == null || sp.dictionary == null) { break; }
				go = sp.dictionary;
			} while (++sentinel < 1000);
			return go.GetComponent<ScriptedDictionary>();
		}
		public void ExecuteScriptOnInventoryVariables(string script) {
			ScriptedDictionary variables = GetDictionaryOfInventory();
			Commander.Instance.EnqueueRun(new Commander.Instruction(script, variables));
		}
		public void ShootParticleFromHereToInventory(string expectedParticleName) {
		}
	}
}