using MazeGeneration;
using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using System;
using System.Collections.Generic;
using UnityEngine;
public class MazeLevel : MonoBehaviour {
    public TextAsset mazeSrc;
    public TextAsset resourceNamesText;
    public TextAsset npcNamesText;
    public MazeTile prefab_mazeTile;
    public CharacterMove prefab_npcPlayer;
    Map2d map;
    public float undiscoveredHeight = 0.0625f;
    public float floorHeight = 0;
    public float wallHeight = .5f;
    public float discoveredTime = 30;
    public float animationTime = .25f;
    public Vector3 tileSize = Vector3.one * 4;
    public Color undiscoveredWall = Color.black, undiscoveredFloor = Color.black;

    List<MazeTile> mazeTiles = new List<MazeTile>();
    public Discovery mainDiscoverer;
    public List<CharacterMove> npcs = new List<CharacterMove>();
    private Dictionary<string, string[]> resourceNames;
    private Dictionary<string, string> npcNames;
    [System.Serializable] public class MazeGenArgs {
        public Vector2 size = new Vector2(21, 21), start = Vector2.one, step = Vector2.one, wall = Vector2.one;
        public int seed = -1, erosion = 0;
    }
    public MazeGenArgs mazeGenerationArguments = new MazeGenArgs();
    public List<GameObject> tokenPrefabs = new List<GameObject>();
    public List<Material> tokenMaterials = new List<Material>();
    public List<GameObject> idols;
    // Start is called before the first frame update
    void Start() {
        Tokenizer tokenizer = new Tokenizer();
        CodeConvert.TryParse(resourceNamesText.text, out resourceNames, null, tokenizer);
        CodeConvert.TryParse(npcNamesText.text, out npcNames, null, tokenizer);
        Generate();
        ConditionCheck cc = Global.Get<ConditionCheck>();
        cc.condition = () => {
            //Show.Log("checking victory " + Global.Get<Team>().members.Count + " / " + (npcs.Count + 1));
            return Global.Get<Team>().members.Count >= npcs.Count+1;
        };
        cc.action = () => {
            DialogManager.Instance.StartDialog("win message");
            DialogManager.Instance.Show();
        };
        //Show.Log("finished initializing " + this);
    }
    public char GetTileSrc(Coord c) { return map[c].letter; }
    public MazeTile GetTile(Coord c) { return mazeTiles[c.Y*map.Width+c.X]; }
    int advancementindex = 0;
    int[] advancementColor, advancementShape;
    public void Generate() {
		if (advancementColor == null) {
            int[] counts = new int[tokenMaterials.Count];
            counts.SetEach(i => tokenPrefabs.Count);
            NumberSequence.GenerateAdvancementOrder2(counts, out advancementColor, out advancementShape);
            //Show.Log(advancementColor.JoinToString(" ", i=>i.ToString("X")) + "\n" + advancementShape.JoinToString(" "));
        }
        if (mazeSrc == null) {
            int seed = mazeGenerationArguments.seed;
            if (seed < 0) {
                seed = (int)Clock.NowRealTicks;
            }
            MazeGenerator mg = new MazeGenerator(seed);
            char[,] map = mg.Generate(mazeGenerationArguments.size, mazeGenerationArguments.start, mazeGenerationArguments.step, mazeGenerationArguments.wall);
            mg.Erode(map, mazeGenerationArguments.erosion);
            this.map = new Map2d(map);
        } else {
            map.LoadFromString(mazeSrc.text);
        }
        int count = map.Height * map.Width;
        while (mazeTiles.Count < count) {
            mazeTiles.Add(Instantiate(prefab_mazeTile.gameObject).GetComponent<MazeTile>());
        }
        //int index = 0;
        Vector3 off = new Vector3(map.Width / -2f * tileSize.x, 0, map.Height / -2f * tileSize.z);
        Transform selfT = transform;
        List<MazeTile> floorTiles = new List<MazeTile>();
        int below2 = 0, below3 = 0;
        map.GetSize().ForEach(c => {
            MazeTile mt = GetTile(c);//mazeTiles[index++];
            mt.maze = this;
            mt.coord = c;
            Transform t = mt.transform;
            t.SetParent(selfT);
            t.localPosition = mt.CalcLocalPosition();
            mt.kind = map[c] == '#' ? MazeTile.Kind.Wall : MazeTile.Kind.Floor;
            mt.SetDiscovered(false, mainDiscoverer, this);
            if (mt.kind == MazeTile.Kind.Floor) {
                floorTiles.Add(mt);
                mt.goalScore = TileScorer(mt, map);
                if (mt.goalScore < 2) { ++below2; } else if (mt.goalScore < 3) { ++below3; }
            } else {
                mt.goalScore = float.PositiveInfinity;
            }
        });
        floorTiles.Sort((a, b) => a.goalScore.CompareTo(b.goalScore));
        //Debug.Log(below2 + " " + below3);
        //Debug.Log(floorTiles.JoinToString(", ", mt => mt.goalScore.ToString()));
        idols = CreateIdols(below2);
        for (int i = 0; i < below2; ++i) {
            PlaceObjectOverTile(idols[i].transform, floorTiles[i]);
        }
        int len = Mathf.Min(below3, tokenMaterials.Count);
        for (int i = npcs.Count; i < len; ++i) {
            Material mat = tokenMaterials[i];
            GameObject npc = Instantiate(prefab_npcPlayer.gameObject);
            ParticleSystem ps = npc.GetComponentInChildren<ParticleSystem>();
            if(ps != null) {
                ps.name = mat.name;
                ParticleSystem.MainModule m = ps.main;
                m.startColor = mat.color;
			}
            npc.name = prefab_npcPlayer.name + i;
            string foundName;
            if(npcNames.TryGetValue(mat.name, out foundName)) {
                npc.name = foundName;
            }
            Interact3dItem i3d = npc.GetComponentInChildren<Interact3dItem>();
            Discovery d = npc.GetComponentInChildren<Discovery>(true);
            d.discoveredFloor = Color.Lerp(d.discoveredFloor, mat.color, 0.5f);
            d.discoveredWall = Color.Lerp(d.discoveredWall, mat.color, 0.5f);
            d.maze = this;
            i3d.Text = npc.name;
            i3d.size = 1.75f;
            i3d.fontCoefficient = .7f;
            i3d.OnInteract = () => {
                DialogManager.Instance.dialogSource = npc;
                DialogManager.Instance.Show();
                DialogManager.Instance.StartDialog("dialog" + mat.name);
                ps.Stop();
            };
            npcs.Add(npc.GetComponent<CharacterMove>());
        }
        for (int i = 0; i < npcs.Count; ++i) {
            PlaceObjectOverTile(npcs[i].transform, floorTiles[below2 + i]);
        }
        Team team = Global.Get<Team>();
        team.AddMember(mainDiscoverer.transform.parent.gameObject);
        PlaceObjectOverTile(team.members[0].transform, floorTiles[floorTiles.Count - 1]);
        Vector3 pos = team.members[0].transform.position;
        for (int i = 1; i < team.members.Count; ++i) {
            team.members[i].transform.position = pos;
        }
    }
    public List<GameObject> CreateIdols(int count) {
        List<GameObject> idols = new List<GameObject>();
        for (int i = 0; i < count; ++i) {
            Material mat = tokenMaterials[advancementColor[advancementindex]];
            GameObject originalItem = tokenPrefabs[advancementShape[advancementindex]];
            GameObject go = Instantiate(originalItem);
            go.name = mat.name;
            Renderer r = go.GetComponent<Renderer>();
            r.material = mat;
            idols.Add(go);
            InventoryItem ii = go.GetComponent<InventoryItem>();
            DictionaryKeeper dk = Global.Get<DictionaryKeeper>();
            ii.onAddToInventory += GoalCheck;
            ii.onAddToInventory += inv => dk.AddTo(mat.name, 1);
            string floatyTextString = mat.name;
            string[] floatyTextOptions = null;
            if(resourceNames == null || (resourceNames.TryGetValue(mat.name, out floatyTextOptions) && floatyTextOptions != null)) {
                floatyTextString = floatyTextOptions[advancementShape[advancementindex]];
                ii.itemName = floatyTextString;
			}
            ii.onAddToInventory += inv => {
                FloatyText ft = FloatyTextManager.Create(go.transform.position + (Vector3.up * tileSize.y * wallHeight), floatyTextString);
                ft.TmpText.faceColor = mat.color;
                // find which NPC wants this, and make them light up
                ParticleSystem ps = null;
                CharacterMove npc = npcs.Find(n=> {
                    ps = n.GetComponentInChildren<ParticleSystem>();
                    if (ps.name == mat.name) return true;
                    ps = null;
                    return false;
                });
                if(npc != null) { ps.Play(); }
            };
            ii.onRemoveFromInventory += inv => {
                //Show.Log("losing " + mat.name);
                dk.AddTo(mat.name, -1);
            };
            ++advancementindex;
            if (advancementindex >= advancementShape.Length) { advancementindex = 0; }
        }
        return idols;
    }
    public void PlaceObjectOverTile(Transform t, MazeTile mt) {
        Vector3 p = mt.CalcVisibilityTarget() + Vector3.up * tileSize.y;
        t.position = p;
    }
    float TileScorer(MazeTile mt, Map2d map) {
        int neighborFloor = -1;// CountTileNeighbors(mt.coord, _allDirs, c => map[c].letter == '#');//-1;
        Coord msize = map.GetSize();
		Coord min = -Coord.One, max = Coord.One;
		Coord.ForEachInclusive(min, max, off => {
			Coord c = mt.coord + off;
			if (c.IsWithin(msize) && map[c].letter != '#') { ++neighborFloor; }
		});
		int maxDist = (msize.row + msize.col) / 2;
        float distFromCenter = Mathf.Abs((msize.row - 1) / 2f - mt.coord.row) + Mathf.Abs((msize.col - 1) / 2f - mt.coord.col);
        return neighborFloor + distFromCenter / maxDist;
    }

    private static Coord[] _cardinalDirs = new Coord[]{Coord.Up, Coord.Left, Coord.Right, Coord.Down};
    private static Coord[] _allDirs = new Coord[] { 
    new Coord(-1,-1),new Coord( 0,-1),new Coord( 1,-1),
    new Coord(-1, 0),                 new Coord( 1, 0),
    new Coord(-1, 1),new Coord( 0, 1),new Coord( 1, 1),
    };
    int CountTileNeighbors(Coord coord, Coord[] neighborOffsets, Func<Coord,bool> predicate) {
        Coord msize = map.GetSize();
        int count = 0;
        for(int i = 0; i < neighborOffsets.Length; ++i) {
            Coord c = coord + neighborOffsets[i];
            if (c.IsWithin(msize) && predicate.Invoke(c)) { ++count; }
        }
        return count;
	}
    public int CountDiscoveredNeighborWalls(Coord coord) {
        return CountTileNeighbors(coord, _cardinalDirs, c => GetTile(c).discovered && map[c] == '#');
	}
    public void GoalCheck(Inventory inv) {
        if (idols == null) return;
        int activeIdols = 0;
        for(int i = 0; i < idols.Count; ++i) {
            if (idols[i] != null && idols[i].activeInHierarchy) ++activeIdols;
		}
        if(activeIdols == 0) {
            mazeGenerationArguments.size += new Vector2(2, 2);
            Clock.setTimeout(()=>Generate(), 2000);
		} 
        //else { Show.Log(activeIdols + " active idols left"); }
	}
}
