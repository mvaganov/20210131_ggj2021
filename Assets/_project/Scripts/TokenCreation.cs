using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using System.Collections;
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
    public void Generate() {
        tokens = CreateTokens(game.maze.floorTileNeighborHistogram[1]);
        for (int i = 0; i < game.maze.floorTileNeighborHistogram[1]; ++i) {
            game.maze.PlaceObjectOverTile(tokens[i].transform, game.maze.floorTiles[i]);
        }
    }

    public void GenerateMoreIdols() {
        // TODO maze should have a list of unfilled tiles sorted by weight
        int start = game.maze.floorTileNeighborHistogram[1] + game.npcCreator.npcs.Count;
        int limit = game.maze.floorTileNeighborHistogram[1] + game.maze.floorTileNeighborHistogram[2] + game.maze.floorTileNeighborHistogram[3];
        int ii = tokens.Count;
        tokens.AddRange(CreateTokens(limit - start));
        for (int i = start; i < limit; ++i) {
            game.maze.PlaceObjectOverTile(tokens[ii++].transform, game.maze.floorTiles[i]);
        }
    }

    public GameObject CreateToken(int color, int shape) {
        Material mat = tokenMaterials[color];
        GameObject originalItem = tokenPrefabs[shape];
        GameObject go = Instantiate(originalItem);
        TokenId tokId = go.GetComponent<TokenId>();
        tokId.id = new int[] { color, shape };
        go.name = mat.name;
        Renderer r = go.GetComponent<Renderer>();
        r.material = mat;
        InventoryItem ii = go.GetComponent<InventoryItem>();
        ii.onAddToInventory += GoalCheck;
        ii.onAddToInventory += inv => game.mainDictionaryKeeper.AddTo(mat.name, 1);
        string floatyTextString = mat.name;
        string[] floatyTextOptions = null;
        if (resourceNames == null || (resourceNames.TryGetValue(mat.name, out floatyTextOptions) && floatyTextOptions != null)) {
            floatyTextString = floatyTextOptions[shape];
            ii.itemName = floatyTextString;
        }
        ii.onAddToInventory += inv => {
            FloatyText ft = FloatyTextManager.Create(go.transform.position + (Vector3.up * game.maze.tileSize.y * game.maze.wallHeight), floatyTextString);
            ft.TmpText.faceColor = mat.color;
            // find which NPC wants this, and make them light up
            ParticleSystem ps = null;
            CharacterMove npc = game.npcCreator.npcs.Find(n => {
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

    public List<GameObject> CreateTokens(int count) {
        List<GameObject> idols = new List<GameObject>();
        int advancementindex = tokensDistributed;
        for (int i = 0; i < count; ++i) {
            idols.Add(CreateToken(advancementColor[advancementindex], advancementShape[advancementindex]));
            ++advancementindex;
            if (advancementindex >= advancementShape.Length) { advancementindex = 0; }
        }
        return idols;
    }
    public void GoalCheck(Inventory inv) {
        if (tokens == null) return;
        if (tokens.CountEach(i => i != null && i.activeInHierarchy) == 0) {
            game.nextLevelButton.SetActive(true);
            game.timer.Pause();
        }
    }

}
