﻿using System;
using UnityEngine;
using UnityEngine.Events;

public class InventoryItem : MonoBehaviour {
	public string itemName;
	public Collider _pickupCollider;
	public Action<Inventory> onAddToInventory;
	public Action<Inventory> onRemoveFromInventory;
	public void Start() {
		if(_pickupCollider == null) { _pickupCollider = GetComponent<Collider>(); }
		CollisionTrigger trigger = _pickupCollider.gameObject.AddComponent<CollisionTrigger>();
		trigger.onTrigger.AddListener(_OnTrigger);
	}

	void _OnTrigger(GameObject other) {
		Inventory inv = other.GetComponent<Inventory>();
		if(inv != null) {
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
