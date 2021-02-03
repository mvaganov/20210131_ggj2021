using NonStandard;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Inventory : MonoBehaviour {

	public List<GameObject> items;
	public ListUi inventoryUi;
	private static ParticleSystem pickupParticle;

	public ListItemUi AddItem(GameObject itemObject) {
		if (pickupParticle == null) { pickupParticle = Global.Get<ParticleSystems>().Get("circdir"); }
		if (pickupParticle != null) {
			pickupParticle.transform.position = itemObject.transform.position;
			pickupParticle.transform.LookAt(transform);
			EmitParams ep = new EmitParams() { startColor = itemObject.GetComponent<Renderer>().material.color };
			pickupParticle.Emit(ep, 10);
		}
		items.Add(itemObject);
		itemObject.SetActive(false);
		itemObject.transform.SetParent(transform);
		if (inventoryUi == null) { return null; }
		InventoryItem item = itemObject.GetComponent<InventoryItem>();
		string name = item != null ? item.itemName : null;
		if(string.IsNullOrEmpty(name)) { name = itemObject.name; }
		item.onAddToInventory?.Invoke(this);
		return inventoryUi.AddItem(itemObject, name, () => {
			RemoveItem(itemObject);
			inventoryUi.RemoveItem(itemObject);
			item.onRemoveFromInventory?.Invoke(this);
		});
	}

	public void RemoveItem(GameObject item) {
		items.Remove(item);
		item.SetActive(true);
		item.transform.SetParent(null);
		Rigidbody rb = item.GetComponent<Rigidbody>();
		if(rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
	}
}
