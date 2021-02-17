using NonStandard;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using NonStandard.Ui;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class InventoryCollector : MonoBehaviour {
	public Inventory inventory;
	public bool autoPickup = true;
	private static ParticleSystem pickupParticle;

	public static void EmitPickupParticle(GameObject itemObject, Vector3 lookAtPosition) {
		if (pickupParticle == null) { pickupParticle = Global.Get<ParticleSystems>().Get("circdir"); }
		if (pickupParticle != null) {
			pickupParticle.transform.position = itemObject.transform.position;
			pickupParticle.transform.LookAt(lookAtPosition);
			Renderer r = itemObject.GetComponent<Renderer>();
			EmitParams ep = new EmitParams();
			if (r != null) { ep.startColor = r.material.color; }
			pickupParticle.Emit(ep, 10);
		}
	}
	public ListItemUi AddItem(GameObject itemObject) {
		EmitPickupParticle(itemObject, inventory.transform.position);
		return inventory.AddItem(itemObject);
	}
	public void RemoveItem(GameObject itemObject) {
		inventory.RemoveItem(itemObject);
	}
}