using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Commands;
using NonStandard.GameUi.Dialog;
using NonStandard.GameUi.Inventory;
using NonStandard.GameUi.Particles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NonStandard.Extension;

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
    public Text bestTimeLabel;
    public ClickToMove clickToMove;
    public bool testingPickups = false;
    NonStandard.Data.Random.NumberGenerator random;

    public void Awake() {
        Commander.Instance.SetScope(mainDictionaryKeeper.Dictionary);
        Commander.Instance.AddCommand(new Command("claimplayer", ClaimPlayer, help:"adds currently dialoging NPC to player's team"));
        random = new NonStandard.Data.Random.NumberGenerator(GameClock.Time);
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
        ++maze.stage;
        LevelGenerate(null);
    }
    [System.Serializable]
    public class LevelState {
        public int stage;
        public long seed;
        public int tokensDistributed;
        public string tokenInventory;
        public string variables;
        public string allies;
        public int bestTime;
        // TODO log of: player inputs with location and timestamps, character waypoints with location and timestamps
	}

    public List<LevelState> levels = new List<LevelState>();
    public void RestartLevel() {
        LevelState lvl = levels.Find(l => l.stage == maze.stage);
        LevelGenerate(lvl);
	}
    public void LevelGenerate(LevelState lvl) {
        Team team = Global.Get<Team>();
        if (lvl == null) {
            lvl = new LevelState();
            lvl.stage = maze.stage;
            lvl.seed = random.Seed;
            lvl.tokensDistributed = tokenCreator.tokensDistributed;
            if(inventory.GetItems() != null) {
                lvl.tokenInventory = CodeConvert.Stringify(inventory.GetItems().ConvertAll(go => { TokenMetaData t = go.GetComponent<TokenMetaData>(); return t ? t.intMetaData : null; }));
			} else {
                lvl.tokenInventory = "";
            }
            lvl.variables = CodeConvert.Stringify(mainDictionaryKeeper.Dictionary);
            lvl.allies = CodeConvert.Stringify(team.members.ConvertAll(m => npcCreator.npcs.FindIndex(n => n.gameObject == m)));
            levels.Add(lvl);
		} else {
            Tokenizer tokenizer = new Tokenizer();
            // check if level is valid
            if (levels.IndexOf(lvl) < 0) {
                throw new Exception("TODO validate the level plz!");
			}
            // set allies
            int[] allies; CodeConvert.TryParse(lvl.allies, out allies, null, tokenizer);
            team.Clear();
            team.AddMember(firstPlayer);
            for(int i = 0; i < allies.Length; ++i) {
                int index = allies[i];
                if (index < 0) continue;
                team.AddMember(npcCreator.npcs[index].gameObject);
			}
            // clear existing tokens
            tokenCreator.Clear();
            // reset inventory to match start state
            inventory.RemoveAllItems();
            int[][] invToLoad;
            CodeConvert.TryParse(lvl.tokenInventory, out invToLoad, null, tokenizer);
            //Debug.Log(Show.Stringify(invToLoad,false));
            Vector3 playerLoc = Global.Get<CharacterControlManager>().localPlayerInterfaceObject.transform.position;
            for (int i = 0; i < invToLoad.Length; ++i) {
                int[] t = invToLoad[i];
                if (t == null || t.Length == 0) continue;
                GameObject token = tokenCreator.CreateToken(t[0], t[1], GoalCheck);
                token.transform.position = playerLoc + Vector3.forward;
                inventory.AddItem(token);
            }
            // set stage
            maze.stage = lvl.stage;
            // set seed
            random.Seed = lvl.seed;
            // set variables
            SensitiveHashTable_stringobject d = mainDictionaryKeeper.Dictionary;
            CodeConvert.TryParse(lvl.variables, out d, null, tokenizer);
            // set 
		}
        MarkTouchdown.ClearMarkers();
        clickToMove.ClearAllWaypoints();
        seedLabel.text = "level "+maze.stage+"." + Convert.ToBase64String(BitConverter.GetBytes(random.Seed));
        maze.Generate(random);
        Discovery.ResetAll();
        tokenCreator.Generate(GoalCheck);
        int len = Mathf.Min(maze.floorTileNeighborHistogram[2], tokenCreator.tokenMaterials.Count);
        npcCreator.GenerateMore(len);
        if (testingPickups) {
            tokenCreator.GenerateMoreIdols(GoalCheck);
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
        UiText.SetColor(timer.gameObject, Color.white);
        timer.Start();
        GoalCheck(null);
        nextLevelButton.SetActive(false);
        bestTimeLabel.gameObject.SetActive(false);
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

    public void ClaimPlayer(Command.Exec e) {
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
    public void GoalCheck(Inventory inv) {
        if (tokenCreator.tokens == null) return;
        if (tokenCreator.tokens.CountEach(i => i != null && i.activeInHierarchy) == 0) {
            nextLevelButton.SetActive(true);
            timer.Pause();
            LevelState lvl = levels.Find(l => l.stage == maze.stage);
			if (lvl == null) {
                throw new Exception("level not properly initialized");
			}
            int t = timer.GetDuration();
            bool firstTime = lvl.bestTime == 0;
            bool betterTime = t < lvl.bestTime;
            if (firstTime || betterTime) {
                lvl.bestTime = t;
            }
			if (betterTime) {
                UiText.SetColor(timer.gameObject, Color.green);
			} else if (!firstTime) {
                UiText.SetColor(timer.gameObject, Color.gray);
            }
            bestTimeLabel.text = "best time: " + GameTimer.TimingToString(lvl.bestTime,true);
            bestTimeLabel.gameObject.SetActive(true);
        }
    }

}
