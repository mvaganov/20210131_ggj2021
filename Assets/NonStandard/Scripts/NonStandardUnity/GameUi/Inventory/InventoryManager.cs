using NonStandard.Data.Parse;
using NonStandard.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryManager : MonoBehaviour {
		public List<Inventory> inventories = new List<Inventory>();
		public static Inventory main;
		public void Register(Inventory inv) { inventories.Add(inv); if (main == null) { main = inv; } }
		public void Awake() {
			Commander.Instance.AddCommand("give", GiveInventory);
			Commander.Instance.AddCommand("useinv", SetMainInventory);
		}
		public void SetMainInventory(string inventoryName) {
			main = inventories.Find(i => i.name == inventoryName);
		}
		public void SetMainInventory(Command.Exec e) {
			string n = e.tok.GetStr(1, Commander.Instance.GetScope());
			SetMainInventory(n);
		}
		public void GiveInventory(Command.Exec e) {
			string itemName = e.tok.GetStr(1, Commander.Instance.GetScope());
			Inventory inv = main;
			GameObject itemObj = inv.RemoveItem(itemName);
			if (itemObj != null) {
				if (e.tok.TokenCount == 2 || e.tok.GetToken(2).ToString() == ";") {
					UnityEngine.Object.Destroy(itemObj);
					//Show.Log("giving to nobody... destroying");
				} else {
					Token recieptiant = e.tok.GetToken(2);
					Show.Log("TODO give " + itemObj + " to " + recieptiant);
				}
			}
		}
	}
}