using MazeGeneration;
using NonStandard;
using NonStandard.Character;
using NonStandard.Data;
using System.Collections.Generic;
using UnityEngine;



public class MazeLevel : MonoBehaviour
{
    public TextAsset mazeSrc;
    public MazeTile prefab_mazeTile;
    public CharacterMove prefab_npcPlayer;
    Map2d map;
    public float wallHeight = 4;
    List<MazeTile> mazeTiles = new List<MazeTile>();
    public Discovery mainDiscoverer;
    public List<CharacterMove> players = new List<CharacterMove>();
    public List<CharacterMove> npcs = new List<CharacterMove>();
    [System.Serializable] public class MazeGenArgs {
        public Vector2 size = new Vector2(21, 21), start = Vector2.one, step = Vector2.one, wall = Vector2.one;
        public int seed = -1, erosion = 0;
	}
    public MazeGenArgs mazeGenerationArguments = new MazeGenArgs();
    public List<GameObject> tokenPrefabs = new List<GameObject>();
    public List<Material> tokenMaterials = new List<Material>();
    public List<GameObject> idols;
    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public void Generate() {
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
        while(mazeTiles.Count < count) {
            mazeTiles.Add(Instantiate(prefab_mazeTile.gameObject).GetComponent<MazeTile>());
		}
        int index = 0;
        Vector3 off = new Vector3(map.Width/-2f * mainDiscoverer.tileSize.x, 0, map.Height/-2f * mainDiscoverer.tileSize.z);
        Transform selfT = transform;
        List<MazeTile> floorTiles = new List<MazeTile>();
        int below2 = 0, below3 = 0;
        map.GetSize().ForEach(c => {
            MazeTile mt = mazeTiles[index++];
            mt.coord = c;
            Transform t = mt.transform;
            t.SetParent(selfT);
            t.localPosition = mt.CalcLocalPosition(mainDiscoverer);
            mt.kind = map[c] == '#' ? MazeTile.Kind.Wall : MazeTile.Kind.Floor;
            mt.SetDiscovered(false, mainDiscoverer);
            if(mt.kind == MazeTile.Kind.Floor) {
                floorTiles.Add(mt);
                mt.goalScore = TileScorer(mt, map);
                if (mt.goalScore < 2) { ++below2; } else if (mt.goalScore < 3) { ++below3; }
            }else {
                mt.goalScore = float.PositiveInfinity;
            }
        });
        floorTiles.Sort((a, b) => a.goalScore.CompareTo(b.goalScore));
        //Debug.Log(below2 + " " + below3);
        //Debug.Log(floorTiles.JoinToString(", ", mt => mt.goalScore.ToString()));
        idols = CreateIdols(0, below2);
        for(int i = 0; i < below2; ++i) {
            PlaceObjectOverTile(idols[i].transform, floorTiles[i]);
        }
        PlaceObjectOverTile(players[0].transform, floorTiles[floorTiles.Count-1]);
        Vector3 pos = players[0].transform.position;
        for (int i = 1; i < players.Count; ++i) {
            players[i].transform.position = pos;
		}
        int len = Mathf.Min(below3, tokenMaterials.Count);
        for(int i = npcs.Count; i < len; ++i) {
            Material mat = tokenMaterials[i];
            GameObject npc = Instantiate(prefab_npcPlayer.gameObject);
            npc.name = mat.name;
            Interact3dItem i3d = npc.GetComponentInChildren<Interact3dItem>();
            i3d.interactText = mat.name;
            i3d.onInteract = () => {
                DialogManager.Instance.Show();
                DialogManager.Instance.StartDialog("dialog" + mat.name);
            };
            npcs.Add(npc.GetComponent<CharacterMove>());
		}
        for (int i = 0; i < npcs.Count; ++i) {
            PlaceObjectOverTile(npcs[i].transform, floorTiles[below2+i]);
        }
    }
    public List<GameObject> CreateIdols(int index, int count) {
        List<GameObject> idols = new List<GameObject>();
        for(int i = 0; i < count; ++i) {
            Material mat = tokenMaterials[0];
            GameObject go = Instantiate(tokenPrefabs[i % tokenPrefabs.Count]);
            go.name = mat.name;
            Renderer r = go.GetComponent<Renderer>();
            r.material = mat;
            idols.Add(go);
            InventoryItem ii = go.GetComponent<InventoryItem>();
            DictionaryKeeper dk = Global.Get<DictionaryKeeper>();
            ii.onAddToInventory += GoalCheck;
            ii.onAddToInventory += inv=>dk.AddTo(mat.name, 1);
            ii.onRemoveFromInventory += inv => dk.AddTo(mat.name, -1);
        }
        return idols;
    }
    public void PlaceObjectOverTile(Transform t, MazeTile mt) {
        Vector3 p = mt.CalcVisibilityTarget(mainDiscoverer)+Vector3.up*mainDiscoverer.tileSize.y;
        t.position = p;
    }
    float TileScorer(MazeTile mt, Map2d map) {
        int neighborFloor = -1;
        Coord msize = map.GetSize();
        Coord min = -Coord.One, max = Coord.One;
        Coord.ForEachInclusive(min, max, off => {
            Coord c = mt.coord + off;
            if(c.IsWithin(msize) && map[c].letter != '#') { ++neighborFloor; }
        });
        int maxDist = (msize.row + msize.col) / 2;
        float distFromCenter = Mathf.Abs((msize.row - 1) / 2f - mt.coord.row) + Mathf.Abs((msize.col - 1) / 2f - mt.coord.col);
        return neighborFloor + distFromCenter / maxDist;
	}

    public void GoalCheck(Inventory inv) {
        if (idols == null) return;
        int activeIdols = 0;
        for(int i = 0; i < idols.Count; ++i) {
            if (idols[i].activeInHierarchy) ++activeIdols;
		}
        if(activeIdols == 0) {
            mazeGenerationArguments.size += new Vector2(2, 2);
            Clock.setTimeout(()=>Generate(), 2000);
		}
	}
}
