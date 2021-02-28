using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.GameUi;
using NonStandard.GameUi.Dialog;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public MazeLevel maze;
    public Discovery prefab_discovery;
    public GameObject firstPlayer;
    public GameObject nextLevelButton;
    public DictionaryKeeper mainDictionaryKeeper;
    private Dictionary<string, string[]> resourceNames;
    public NpcCreation npcCreator;
    public TokenCreation tokenCreator;
    public GameTimer timer;
    public Inventory inventory;
    public Text seedLabel;
    public bool testingPickups = false;
    NonStandard.Data.Random random;

    public void Awake() {
        Commander.Instance.SetScope(mainDictionaryKeeper.Dictionary);
        Commander.Instance.AddCommand("claimplayer", ClaimPlayer);
        random = new NonStandard.Data.Random(Clock.NowRealtime);
    }

    void Start() {
        tokenCreator.Init();
        npcCreator.Init();
        ConditionCheck cc = Global.Get<ConditionCheck>();
        cc.condition = () => {
            //Show.Log("checking victory " + Global.Get<Team>().members.Count + " / " + (npcs.Count + 1));
            return Global.Get<Team>().members.Count >= npcCreator.npcs.Count + 1;
        };
        cc.action = () => {
            Tokenizer tok = new Tokenizer();
            DialogManager.Instance.StartDialog(this, tok, "win message");
            tok.ShowErrorTo(DialogManager.ActiveDialog.ShowError);
            DialogManager.Instance.Show();
        };
        //Show.Log("finished initializing " + this);
        Team team = Global.Get<Team>();
        team.AddMember(firstPlayer);
        EnsureExplorer(firstPlayer);
        maze.stage=-1;
        GenerateNext();
    }
    public void GenerateNext() {
        tokenCreator.AdvanceTokens();
        nextLevelButton.SetActive(false);
        maze.mazeGenerationArguments.size += new Vector2(2, 2);
        ++maze.stage;
        LevelGenerate(null);
        timer.Start();
        tokenCreator.GoalCheck(null);
    }
    [System.Serializable]
    public class LevelInitState {
        public int stage;
        public long seed;
        public int tokensDistributed;
        public string tokenInventory;
        public string variables;
        public string allies;
        public int bestTime;
        // TODO log of: player inputs with location and timestamps, character waypoints with location and timestamps
	}

    public List<LevelInitState> levels = new List<LevelInitState>();

    public void LevelGenerate(LevelInitState lvl) {
        Team team = Global.Get<Team>();
        if (lvl == null) {
            lvl = new LevelInitState();
            lvl.stage = maze.stage;
            lvl.seed = random.Seed;
            lvl.tokensDistributed = tokenCreator.tokensDistributed;
            if(inventory.GetItems() != null) {
                lvl.tokenInventory = Show.Stringify(inventory.GetItems().ConvertAll(go => { TokenId t = go.GetComponent<TokenId>(); return t ? t.id : null; }), false);
			} else {
                lvl.tokenInventory = "[]";
            }
            lvl.variables = Show.Stringify(mainDictionaryKeeper.Dictionary,false);
            lvl.allies = Show.Stringify(team.members.ConvertAll(m => npcCreator.npcs.FindIndex(n => n.gameObject == m)),false);
            levels.Add(lvl);
		} else {
			// check if level is valid
			if (levels.IndexOf(lvl) < 0) {
                throw new Exception("TODO validate the level plz!");
			}
            // clear inventory
            // set stage
            // set seed
            // set inventory (objects in front)
            // set variables
		}
        MarkTouchdown.ClearMarkers();
        seedLabel.text = "level "+maze.stage+"." + Convert.ToBase64String(BitConverter.GetBytes(random.Seed));
        maze.Generate(random);
        tokenCreator.Generate();
        int len = Mathf.Min(maze.floorTileNeighborHistogram[2], tokenCreator.tokenMaterials.Count);
        npcCreator.GenerateMore(len);
        if (testingPickups) {
            tokenCreator.GenerateMoreIdols();
        }
        // TODO maze should have a list of unfilled tiles sorted by weight
        for (int i = 0; i < npcCreator.npcs.Count; ++i) {
            maze.PlaceObjectOverTile(npcCreator.npcs[i].transform, maze.floorTiles[maze.floorTileNeighborHistogram[1] + i]);
        }
        team.AddMember(firstPlayer);
        maze.PlaceObjectOverTile(team.members[0].transform, maze.floorTiles[maze.floorTiles.Count - 1]);
        Vector3 pos = team.members[0].transform.position;
        for (int i = 0; i < team.members.Count; ++i) {
            team.members[i].transform.position = pos;
            CharacterMove cm = team.members[i].GetComponent<CharacterMove>();
            if (cm != null) {
                cm.SetAutoMovePosition(pos);
                cm.MoveForwardMovement = 0;
                cm.StrafeRightMovement = 0;
            }
        }
    }

    public Discovery EnsureExplorer(GameObject go) {
        Discovery d = go.GetComponentInChildren<Discovery>();
        if (d == null) {
            d = Instantiate(prefab_discovery.gameObject).GetComponent<Discovery>();
            Transform t = d.transform;
            t.SetParent(go.transform);
            t.localPosition = Vector3.zero;
        }
        return d;
    }

    public void ClaimPlayer(object src, Tokenizer tok) {
        GameObject npc = DialogManager.Instance.dialogWithWho;
        Global.Get<Team>().AddMember(npc);
        MazeLevel ml = Global.Get<MazeLevel>();
        Discovery d = EnsureExplorer(npc);
        ParticleSystem ps = npc.GetComponentInChildren<ParticleSystem>();
        Color color = ps.main.startColor.color;
        d.discoveredFloor = Color.Lerp(d.discoveredFloor, color, 0.25f);
        d.discoveredWall = Color.Lerp(d.discoveredWall, color, 0.25f);
        d.discoveredRamp = Color.Lerp(d.discoveredRamp, color, 0.25f);
        d.maze = ml;
        Global.Get<ConditionCheck>().DoActivateTest();
        InventoryCollector inv = npc.GetComponentInChildren<InventoryCollector>();
        inv.inventory = InventoryManager.main;
        inv.autoPickup = true;
    }

}
