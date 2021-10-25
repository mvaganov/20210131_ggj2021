using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using NonStandard.Utility.UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class IdolCreation : MonoBehaviour
{
	public Game game;
	public TextAsset resourceNamesText;
	private Dictionary<string, string[]> resourceNames;
	public List<GameObject> idolPrefabs = new List<GameObject>();
	public List<Material> idolMaterials = new List<Material>();
	public List<GameObject> idols;
	public int idolsDistributed = 0;
	int[] advancementColor, advancementShape;

	public void AdvanceIdols() { idolsDistributed += idols.Count; }

	public void Init() {
		Tokenizer tokenizer = new Tokenizer();
		CodeConvert.TryParse(resourceNamesText.text, out resourceNames, null, tokenizer);
		GenerateAdvancementSequence();
	}
	public void GenerateAdvancementSequence() {
		if (advancementColor == null) {
			int[] counts = new int[idolMaterials.Count];
			counts.SetEach(i => idolPrefabs.Count);
			NumberSequence.GenerateAdvancementOrder2(counts, out advancementColor, out advancementShape);
			//Show.Log(advancementColor.JoinToString(" ", i=>i.ToString("X")) + "\n" + advancementShape.JoinToString(" "));
		}
	}
	public void Generate(EventBind onPickupAction) {
		idols = CreateIdols(game.maze.floorTileNeighborHistogram[1], onPickupAction);
		for (int i = 0; i < game.maze.floorTileNeighborHistogram[1]; ++i) {
			game.maze.PlaceObjectOverTile(idols[i].transform, game.maze.floorTiles[i]);
		}
	}

	public void GenerateMoreIdols(EventBind onPickupAction) {
		// TODO maze should have a list of unfilled tiles sorted by weight
		int start = game.maze.floorTileNeighborHistogram[1] + game.npcCreator.npcs.Count;
		int limit = game.maze.floorTileNeighborHistogram[1] + game.maze.floorTileNeighborHistogram[2] + game.maze.floorTileNeighborHistogram[3];
		int ii = idols.Count;
		idols.AddRange(CreateIdols(limit - start, onPickupAction));
		for (int i = start; i < limit; ++i) {
			game.maze.PlaceObjectOverTile(idols[ii++].transform, game.maze.floorTiles[i]);
		}
	}

	public int CountUnclaimedIdols() {
		int count = 0;
		for (int i = idols.Count - 1; i >= 0; --i) {
			GameObject go = idols[i];
			//Show.Log("checking " + go);
			bool isProblem = false;
			InventoryItem ii = null;
			if (go == null) {
				isProblem = true;
			} else {
				ii = go.GetComponent<InventoryItem>();
				if (ii == null) {
					Show.Log("uhh... " + i + " doesn't is not an inventory item");
					isProblem = true;
				}
				if (go.GetComponent<Idol>() == null) {
					Show.Log("uhh... " + i + " is not an idol");
					isProblem = true;
				}
			}
			if (isProblem) { idols.RemoveAt(i); continue; }
			if (ii.inventory == null) { ++count; }
		}
		return count;
	}

	public GameObject CreateIdol(int color, int shape, EventBind onPickupAction) {
		Material mat = idolMaterials[color];
		GameObject originalItem = idolPrefabs[shape];
		GameObject go = Instantiate(originalItem);
		Idol idol = go.GetComponent<Idol>();
		idol.intMetaData = new int[] { color, shape };
		idol.color = mat.name;
		go.name = mat.name;
		Renderer r = go.GetComponent<Renderer>();
		r.material = mat;
		InventoryItem ii = go.GetComponent<InventoryItem>();
		string[] floatyTextOptions = null;
		if (resourceNames == null || (resourceNames.TryGetValue(mat.name, out floatyTextOptions) && floatyTextOptions != null)) {
			string floatyTextString = floatyTextOptions[shape];
			idol.kind = floatyTextString;
			ii.itemName = floatyTextString;
		}
		if (onPickupAction != null && !onPickupAction.IsAlreadyBound(ii.addToInventoryEvent)) {
			//Show.Log("bound "+onPickupAction.methodName);
			onPickupAction.Bind(ii.addToInventoryEvent);
		}
		return go;
	}
	public void AddToPlayerInventory(InventoryItem ii) {
		Vector3 p = ii.transform.position;
		Material mat = ii.GetComponent<Renderer>().material;
		GameClock.Delay(0, () => { // make sure object creation happens on the main thread
			FloatyText ft = FloatyTextManager.Create(p + (Vector3.up * game.maze.tileSize.y * game.maze.wallHeight), mat.name);
			ft.TmpText.faceColor = mat.color;
		});
		// find which NPC wants this, and make them light up
		ParticleSystem ps = null;
		CharacterRoot npc = game.npcCreator.npcs.Find(n => {
			ps = n.GetComponentInChildren<ParticleSystem>();
			if (ps.name == mat.name) return true;
			ps = null;
			return false;
		});
		if (npc != null) { ps.Play(); }
	}

	public void Clear() {
		if (idols == null) return;
		for(int i = idols.Count - 1; i >= 0; --i) {
			Destroy(idols[i]);
		}
		idols.Clear();
	}

	public List<GameObject> CreateIdols(int count, EventBind onPickupAction) {
		List<GameObject> idols = new List<GameObject>();
		int advancementindex = idolsDistributed;
		for (int i = 0; i < count; ++i) {
			idols.Add(CreateIdol(advancementColor[advancementindex], advancementShape[advancementindex], onPickupAction));
			++advancementindex;
			if (advancementindex >= advancementShape.Length) { advancementindex = 0; }
		}
		return idols;
	}
}
