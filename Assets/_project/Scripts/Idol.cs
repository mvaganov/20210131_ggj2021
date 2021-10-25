using NonStandard;
using NonStandard.Character;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using UnityEngine;

public class Idol : MonoBehaviour {
	/// <summary>
	/// what color something is. phylum -> class -> order -> family -> [genus] -> species
	/// </summary>
	public string color;
	/// <summary>
	/// what kind it is. phylum -> class -> order -> family -> genus -> [species]
	/// </summary>
	public string kind;
	public int[] intMetaData;
	public void PickUp(GameObject finder) {
		//Show.Log(finder+" picked up "+specificName+"("+typeName+")");
		Game game = Global.Get<Game>();
		Vector3 p = transform.position;
		Material mat = GetComponent<Renderer>().material;
		GameClock.Delay(0, () => { // make sure object creation happens on the main thread
			FloatyText ft = FloatyTextManager.Create(p + (Vector3.up * game.maze.tileSize.y * game.maze.wallHeight), kind);
			ft.TmpText.faceColor = mat.color;
		});
		// find which NPC wants this, and make them light up
		ParticleSystem ps = null;
		//Show.Log(specificName+" turns on particle for "+typeName);
		CharacterRoot npc = game.npcCreator.npcs.Find(n => {
			ps = n.GetComponentInChildren<ParticleSystem>();
			//Show.Log(ps.name + " v " + typeName);
			if (ps != null && ps.name == color) return true;
			ps = null;
			return false;
		});
		if (npc != null && ps != null) { ps.Play(); }
	}
	/// <summary>
	/// adds to a variable with the same name as this item's material color. the variable is in the same dictionary as the inventory this item is in
	/// </summary>
	/// <param name="count"></param>
	public void AddColorToVariables(float count) {
		string n = color;
		if (string.IsNullOrEmpty(color)) {
			Renderer r = GetComponent<Renderer>();
			if (r != null) {
				Material mat = r.material;
				color = mat.name;
			}
		}
		InventoryItem ii = GetComponent<InventoryItem>();
		ii.GetDictionaryOfInventory().AddTo(n, count);
	}
}
