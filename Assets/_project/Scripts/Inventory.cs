using NonStandard;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Inventory : MonoBehaviour {
	List<GameObject> items;
	public ListUi inventoryUi;
	public Inventory proxyFor;
	private static ParticleSystem pickupParticle;

	public ListItemUi AddItem(GameObject itemObject) {
		if (proxyFor != null && proxyFor != this) { return proxyFor.AddItem(itemObject); }
		if (pickupParticle == null) { pickupParticle = Global.Get<ParticleSystems>().Get("circdir"); }
		if (pickupParticle != null) {
			pickupParticle.transform.position = itemObject.transform.position;
			pickupParticle.transform.LookAt(transform);
			EmitParams ep = new EmitParams() { startColor = itemObject.GetComponent<Renderer>().material.color };
			pickupParticle.Emit(ep, 10);
		}
		if(items == null) { items = new List<GameObject>(); }
		if (inventoryUi != null) { ListItemUi result = inventoryUi.GetListItemUi(itemObject); if (result != null) return result; }
		items.Add(itemObject);
		InventoryItem item = itemObject.GetComponent<InventoryItem>();
		itemObject.SetActive(false);
		item.onAddToInventory?.Invoke(this);
		Vector3 playerLoc = Global.Get<CharacterControlManager>().localPlayerInterfaceObject.transform.position;
		Vector3 localPosition = itemObject.transform.position - playerLoc;
		itemObject.transform.SetParent(transform);
		itemObject.transform.localPosition = localPosition;
		//Show.Log("POS IN" + localPosition);
		string name = item != null ? item.itemName : null;
		if (string.IsNullOrEmpty(name)) { name = itemObject.name; }
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
		if(go != null) {
			RemoveItem(go);
		}
		return go;
	}
	public void RemoveItem(GameObject itemObject) {
		if (proxyFor != null && proxyFor != this) { proxyFor.RemoveItem(itemObject); return; }
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
}
