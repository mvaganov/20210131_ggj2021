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
			Commander.Instance.AddCommand(new Command("give", GiveInventory, new Argument[] {
				new Argument("-i","itemName","unique item name",type:typeof(string),order:1,required:true),
				new Argument("-t","target","inventory to give the item to",type:typeof(string),order:2),
			}, help:"removes the given item from the main inventory, possibly adding it to the target inventory"));
			Commander.Instance.AddCommand(new Command("useinv", SetMainInventory, new Argument[] {
				new Argument("-i","inventoryName","which inventory to mark as the main one",type:typeof(string),order:1,required:true),
			}, help:"identifies which inventory should be considered the main inventory for following inventory operations"));
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
			//Show.Log("Give " + e.tok);
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