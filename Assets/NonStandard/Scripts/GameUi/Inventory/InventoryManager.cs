using NonStandard.Data.Parse;
using UnityEngine;

namespace NonStandard.GameUi.Inventory {
	public class InventoryManager : MonoBehaviour {
		public void Awake() {
			Commander.Instance.AddCommand("give", GiveInventory);
		}
		public void GiveInventory(object src, Tokenizer tok) {
			string itemName = tok.GetStr(1, Commander.Instance.GetScope());
			Inventory inv = Global.Get<Inventory>();
			GameObject itemObj = inv.RemoveItem(itemName);
			if (itemObj != null) {
				if (tok.TokenCount == 2 || tok.GetToken(2).ToString() == ";") {
					UnityEngine.Object.Destroy(itemObj);
					//Show.Log("giving to nobody... destroying");
				} else {
					Token recieptiant = tok.GetToken(2);
					Show.Log("TODO give " + itemObj + " to " + recieptiant);
				}
			}
		}
	}
}