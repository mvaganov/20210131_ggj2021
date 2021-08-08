using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TokenCreation : MonoBehaviour
{
    public Game game;
    public TextAsset resourceNamesText;
    private Dictionary<string, string[]> resourceNames;
    public List<GameObject> tokenPrefabs = new List<GameObject>();
    public List<Material> tokenMaterials = new List<Material>();
    public List<GameObject> tokens;
    public int tokensDistributed = 0;
    int[] advancementColor, advancementShape;

    public void AdvanceTokens() { tokensDistributed += tokens.Count; }

    public void Init() {
        Tokenizer tokenizer = new Tokenizer();
        CodeConvert.TryParse(resourceNamesText.text, out resourceNames, null, tokenizer);
        GenerateAdvancementSequence();
    }
    public void GenerateAdvancementSequence() {
        if (advancementColor == null) {
            int[] counts = new int[tokenMaterials.Count];
            counts.SetEach(i => tokenPrefabs.Count);
            NumberSequence.GenerateAdvancementOrder2(counts, out advancementColor, out advancementShape);
            //Show.Log(advancementColor.JoinToString(" ", i=>i.ToString("X")) + "\n" + advancementShape.JoinToString(" "));
        }
    }
    // TODO move to Game?
    public void Generate(Action<Inventory> onAdd) {
        tokens = CreateTokens(game.maze.floorTileNeighborHistogram[1], onAdd);
        for (int i = 0; i < game.maze.floorTileNeighborHistogram[1]; ++i) {
            game.maze.PlaceObjectOverTile(tokens[i].transform, game.maze.floorTiles[i]);
        }
    }

    // TODO move to Game?
    public void GenerateMoreIdols(Action<Inventory> onAdd) {
        // TODO maze should have a list of unfilled tiles sorted by weight
        int start = game.maze.floorTileNeighborHistogram[1] + game.npcCreator.npcs.Count;
        int limit = game.maze.floorTileNeighborHistogram[1] + game.maze.floorTileNeighborHistogram[2] + game.maze.floorTileNeighborHistogram[3];
        int ii = tokens.Count;
        tokens.AddRange(CreateTokens(limit - start, onAdd));
        for (int i = start; i < limit; ++i) {
            game.maze.PlaceObjectOverTile(tokens[ii++].transform, game.maze.floorTiles[i]);
        }
    }

    public GameObject CreateToken(int color, int shape, Action<Inventory> onAdd) {
        Material mat = tokenMaterials[color];
        GameObject originalItem = tokenPrefabs[shape];
        GameObject go = Instantiate(originalItem);
        TokenMetaData tokId = go.GetComponent<TokenMetaData>();
        tokId.intMetaData = new int[] { color, shape };
        go.name = mat.name;
        Renderer r = go.GetComponent<Renderer>();
        r.material = mat;
        InventoryItem ii = go.GetComponent<InventoryItem>();
        if (onAdd != null) {
            ii.onAddToInventory += onAdd;// GoalCheck;
        }
        ii.onAddToInventory += inv => game.mainDictionaryKeeper.AddTo(mat.name, 1);
        string floatyTextString = mat.name;
        string[] floatyTextOptions = null;
        if (resourceNames == null || (resourceNames.TryGetValue(mat.name, out floatyTextOptions) && floatyTextOptions != null)) {
            floatyTextString = floatyTextOptions[shape];
            ii.itemName = floatyTextString;
        }
        ii.onAddToInventory += inv => {
            Vector3 p = ii.transform.position;
            GameClock.Delay(0, () => { // make sure object creation happens on the main thread
                FloatyText ft = FloatyTextManager.Create(p + (Vector3.up * game.maze.tileSize.y * game.maze.wallHeight), floatyTextString);
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
        };
        ii.onRemoveFromInventory += inv => {
            //Show.Log("losing " + mat.name);
            game.mainDictionaryKeeper.AddTo(mat.name, -1);
        };
        return go;
    }

	public void Clear() {
        if (tokens == null) return;
        for(int i = tokens.Count - 1; i >= 0; --i) {
            Destroy(tokens[i]);
		}
        tokens.Clear();
	}

	public List<GameObject> CreateTokens(int count, Action<Inventory> onAdd) {
        List<GameObject> idols = new List<GameObject>();
        int advancementindex = tokensDistributed;
        for (int i = 0; i < count; ++i) {
            idols.Add(CreateToken(advancementColor[advancementindex], advancementShape[advancementindex], onAdd));
            ++advancementindex;
            if (advancementindex >= advancementShape.Length) { advancementindex = 0; }
        }
        return idols;
    }
}
